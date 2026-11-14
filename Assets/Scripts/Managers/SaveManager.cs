using System;
using System.IO;
using UnityEngine;

/// <summary>
/// SaveManager menangani semua operasi penyimpanan dan pemuatan data game menggunakan JSON.
/// Data disimpan di <see cref="Application.persistentDataPath"/> dengan dua format berbeda:
/// <list type="bullet">
///     <item><description>Format lama (<see cref="SaveData"/>) - digunakan oleh metode <see cref="SaveGame"/> dan <see cref="LoadGame"/>.</description></item>
///     <item><description>Format baru (<see cref="SaveDataNew"/>) - digunakan oleh metode <see cref="SaveGameNew"/> dan <see cref="LoadGameNew"/>.</description></item>
/// </list>
/// Penulisan dilakukan secara atomik (file sementara lalu diganti) dan file sebelumnya disimpan sebagai cadangan (.bak),
/// sehingga crash saat menyimpan tidak merusak save yang ada.
/// </summary>
public class SaveManager : Singleton<SaveManager>
{
    // ======================================================
    // Konfigurasi file
    // ======================================================

    private const string SAVE_FILE_NAME_LEGACY = "aksara_batik_save.json";
    private const string SAVE_FILE_NAME_NEW = "aksara_batik_save_new.json";
    private const string BACKUP_EXTENSION = ".bak";
    private const string TEMP_EXTENSION = ".tmp";

    private string _savePathLegacy;
    private string _savePathNew;

    // ======================================================
    // Singleton behavior
    // ======================================================

    protected override bool PersistBetweenScenes => true;

    protected override void Awake()
    {
        base.Awake();

        _savePathLegacy = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME_LEGACY);
        _savePathNew = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME_NEW);

        Debug.Log($"SaveManager : Lokasi save legacy: {_savePathLegacy}");
        Debug.Log($"SaveManager : Lokasi save new    : {_savePathNew}");
    }

    // ======================================================
    // Penulisan dan pembacaan file (atomik + cadangan)
    // ======================================================

    /// <summary>
    /// Menulis ke file sementara dulu, lalu mengganti file tujuan. File lama menjadi cadangan (.bak).
    /// </summary>
    private static void TulisAtomik(string path, string json)
    {
        string pathSementara = path + TEMP_EXTENSION;

        File.WriteAllText(pathSementara, json);

        if (File.Exists(path))
            File.Replace(pathSementara, path, path + BACKUP_EXTENSION);
        else
            File.Move(pathSementara, path);
    }

    /// <summary>
    /// Membaca dan mengurai JSON. Jika file utama rusak atau tidak bisa dibaca, mencoba cadangan (.bak).
    /// </summary>
    private static T BacaDenganCadangan<T>(string path, string label) where T : class
    {
        T hasil = CobaBaca<T>(path, label);

        if (hasil != null) return hasil;

        string pathCadangan = path + BACKUP_EXTENSION;

        if (File.Exists(pathCadangan))
        {
            Debug.LogWarning($"SaveManager : {label} utama tidak bisa dibaca, mencoba cadangan.");
            hasil = CobaBaca<T>(pathCadangan, label + " (cadangan)");
        }

        return hasil;
    }

    private static T CobaBaca<T>(string path, string label) where T : class
    {
        if (!File.Exists(path)) return null;

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager : Gagal membaca {label}. Error: {e.Message}");
            return null;
        }
    }

    // ======================================================
    // Metode untuk format lama (Legacy)
    // ======================================================

    public bool HasSaveLegacy() => File.Exists(_savePathLegacy);

    public void SaveGame(int currentPhase, int currentDay, long currentBalance)
    {
        SaveData data = new SaveData
        {
            phase = currentPhase,
            day = currentDay,
            balance = currentBalance,
            timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        string json = JsonUtility.ToJson(data, prettyPrint: true);

        try
        {
            TulisAtomik(_savePathLegacy, json);
            Debug.Log($"SaveManager : Save legacy berhasil (Phase {currentPhase}, Day {currentDay}, Balance {currentBalance})");
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager [SaveGame] : Gagal menyimpan save legacy. Error: {e.Message}");
        }
    }

    public SaveData LoadGame()
    {
        if (!HasSaveLegacy())
        {
            Debug.LogWarning("SaveManager [LoadGame] : Tidak ada save legacy.");
            return null;
        }

        SaveData saveData = BacaDenganCadangan<SaveData>(_savePathLegacy, "save legacy");

        if (saveData != null)
            Debug.Log($"SaveManager : Load legacy sukses (Phase {saveData.phase}, Day {saveData.day}, Balance {saveData.balance})");

        return saveData;
    }

    public SaveData GetGameData() => LoadGame();

    public void DeleteSaveLegacy()
    {
        HapusFile(_savePathLegacy);
        Debug.Log("SaveManager : Save legacy berhasil dihapus.");
    }

    // ======================================================
    // Metode untuk format baru (SaveDataNew)
    // ======================================================

    public bool HasSaveNew() => File.Exists(_savePathNew);

    public void SaveGameNew(SaveDataNew data)
    {
        if (data == null)
        {
            Debug.LogWarning("SaveManager [SaveGameNew] : Data tidak boleh null.");
            return;
        }

        string json = JsonUtility.ToJson(data, prettyPrint: true);

        try
        {
            TulisAtomik(_savePathNew, json);
            Debug.Log("SaveManager : SaveDataNew berhasil disimpan.");
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager [SaveGameNew] : Gagal menyimpan. Error: {e.Message}");
        }
    }

    public SaveDataNew LoadGameNew()
    {
        if (!HasSaveNew())
        {
            Debug.LogWarning("SaveManager [LoadGameNew] : Tidak ada save baru.");
            return null;
        }

        SaveDataNew data = BacaDenganCadangan<SaveDataNew>(_savePathNew, "save baru");

        if (data != null) Debug.Log("SaveManager : LoadGameNew sukses.");

        return data;
    }

    /// <summary>
    /// Membuat save baru yang strukturnya (jumlah fase dan hari) mengikuti konfigurasi game yang sedang dipakai.
    /// </summary>
    public SaveDataNew CreateNewSave(PhaseConfig[] phaseConfigs) => SaveDataNewDefault.Create(phaseConfigs);

    public void DeleteSaveNew()
    {
        HapusFile(_savePathNew);
        Debug.Log("SaveManager : Save baru berhasil dihapus.");
    }

    // ======================================================
    // Metode utilitas umum
    // ======================================================

    public bool HasAnySave() => HasSaveLegacy() || HasSaveNew();

    public void DeleteAllSaves()
    {
        DeleteSaveLegacy();
        DeleteSaveNew();
        Debug.Log("SaveManager : Semua file save telah dihapus.");
    }

    /// <summary>
    /// Menghapus file save beserta cadangan dan file sementaranya.
    /// </summary>
    private static void HapusFile(string path)
    {
        foreach (string target in new[] { path, path + BACKUP_EXTENSION, path + TEMP_EXTENSION })
        {
            if (File.Exists(target)) File.Delete(target);
        }
    }

    // ======================================================
    // Metode kompatibilitas (opsional)
    // ======================================================

    [Obsolete("Gunakan HasSaveLegacy() atau HasAnySave().")]
    public bool HasSave() => HasSaveLegacy();

    [Obsolete("Gunakan DeleteSaveLegacy().")]
    public void DeleteSave() => DeleteSaveLegacy();
}
