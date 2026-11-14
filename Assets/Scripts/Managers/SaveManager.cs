using System;
using System.IO;
using UnityEngine;

/// <summary>
/// SaveManager menangani penyimpanan dan pemuatan data game (<see cref="SaveDataNew"/>) dalam format JSON
/// di <see cref="Application.persistentDataPath"/>.
/// Penulisan dilakukan secara atomik (file sementara lalu diganti) dan file sebelumnya disimpan sebagai cadangan (.bak),
/// sehingga crash saat menyimpan tidak merusak save yang ada.
/// </summary>
public class SaveManager : Singleton<SaveManager>
{
    // ======================================================
    // Konfigurasi file
    // ======================================================

    private const string SAVE_FILE_NAME = "aksara_batik_save_new.json";
    private const string BACKUP_EXTENSION = ".bak";
    private const string TEMP_EXTENSION = ".tmp";

    private string _savePathNew;

    // ======================================================
    // Singleton behavior
    // ======================================================

    protected override bool PersistBetweenScenes => true;

    protected override void Awake()
    {
        base.Awake();

        _savePathNew = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        Debug.Log($"SaveManager : Lokasi save: {_savePathNew}");
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
    // Save game (SaveDataNew)
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
    // Utilitas
    // ======================================================

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
}
