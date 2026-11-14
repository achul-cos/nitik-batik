using UnityEngine;

/// <summary>
/// Kelas dasar untuk manager yang hanya boleh ada satu di dalam game (singleton).
/// <para />
/// Cara pakai: <c>public class GameManager : Singleton&lt;GameManager&gt;</c>, lalu akses lewat <c>GameManager.Instance</c>.
/// <para />
/// Perilaku:
/// <list type="bullet">
///   <item><description>Instance dicari di scene; jika tidak ada, GameObject baru dibuat otomatis (dan muncul log). Pasang manager di scene agar data Inspector terisi.</description></item>
///   <item><description>Jika ada duplikat saat Awake, duplikatnya dihancurkan.</description></item>
///   <item><description>Jika <see cref="PersistBetweenScenes"/> true, manager tidak dihancurkan saat pindah scene.</description></item>
///   <item><description>Setelah aplikasi ditutup, Instance yang sudah hancur tidak dibuat ulang (mengembalikan null).</description></item>
/// </list>
/// Class turunan yang menimpa <see cref="Awake"/> wajib memanggil <c>base.Awake()</c>.
/// </summary>
/// <typeparam name="T">Class manager yang menjadi singleton</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _applicationIsQuitting = false;

    /// <summary>
    /// True = tidak dihapus saat berpindah scene. Bisa ditimpa: <c>protected override bool PersistBetweenScenes =&gt; false;</c>
    /// </summary>
    protected virtual bool PersistBetweenScenes => true;

    /// <summary>
    /// Instance singleton. Dibuat otomatis jika belum ada, kecuali aplikasi sedang ditutup.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;

            // Saat aplikasi ditutup, jangan membuat manager baru dari OnDestroy/OnDisable objek lain.
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton<{typeof(T)}>] Instance diminta setelah aplikasi ditutup, mengembalikan null.");
                return null;
            }

            _instance = FindAnyObjectByType<T>();

            if (_instance == null)
            {
                GameObject go = new GameObject(typeof(T).Name);
                _instance = go.AddComponent<T>();

                Debug.Log($"[Singleton<{typeof(T)}>] Dibuat otomatis karena tidak ada di scene.");
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        _applicationIsQuitting = false;

        if (_instance == null)
        {
            _instance = this as T;

            if (PersistBetweenScenes)
                DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[Singleton<{typeof(T)}>] Duplikat ditemukan pada {gameObject.name}, duplikat dihancurkan.");
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
