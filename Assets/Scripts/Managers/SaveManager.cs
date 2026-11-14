using System;
using System.IO;
using UnityEngine;

/// <summary>
/// SaveManager menangani semua operasi penyimpanan dan pemuatan data game menggunakan JSON.
/// Data disimpan di <see cref="Application.persistentDataPath"/> dengan dua format berbeda:
/// <list type="bullet">
///     <item><description>Format lama (<see cref="SaveData"/>) – digunakan oleh metode <see cref="SaveGame"/> dan <see cref="LoadGame"/>.</description></item>
///     <item><description>Format baru (<see cref="SaveDataNew"/>) – digunakan oleh metode <see cref="SaveGameNew"/> dan <see cref="LoadGameNew"/>.</description></item>
/// </list>
/// </summary>
public class SaveManager : Singleton<SaveManager>
{
    // ======================================================
    // Konfigurasi file
    // ======================================================

    private const string SAVE_FILE_NAME_LEGACY = "aksara_batik_save.json";
    private const string SAVE_FILE_NAME_NEW = "aksara_batik_save_new.json";

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
            File.WriteAllText(_savePathLegacy, json);
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

        try
        {
            string json = File.ReadAllText(_savePathLegacy);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"SaveManager : Load legacy sukses (Phase {saveData.phase}, Day {saveData.day}, Balance {saveData.balance})");
            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager [LoadGame] : Gagal membaca save legacy. Error: {e.Message}");
            return null;
        }
    }

    public SaveData GetGameData() => LoadGame();

    public void DeleteSaveLegacy()
    {
        if (HasSaveLegacy())
        {
            File.Delete(_savePathLegacy);
            Debug.Log("SaveManager : Save legacy berhasil dihapus.");
        }
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
            File.WriteAllText(_savePathNew, json);
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

        try
        {
            string json = File.ReadAllText(_savePathNew);
            SaveDataNew data = JsonUtility.FromJson<SaveDataNew>(json);
            Debug.Log("SaveManager : LoadGameNew sukses.");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager [LoadGameNew] : Gagal membaca. Error: {e.Message}");
            return null;
        }
    }

    public SaveDataNew CreateNewSave() => SaveDataNewDefault.Create();

    public void DeleteSaveNew()
    {
        if (HasSaveNew())
        {
            File.Delete(_savePathNew);
            Debug.Log("SaveManager : Save baru berhasil dihapus.");
        }
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

    // ======================================================
    // Metode kompatibilitas (opsional)
    // ======================================================

    [Obsolete("Gunakan HasSaveLegacy() atau HasAnySave().")]
    public bool HasSave() => HasSaveLegacy();

    [Obsolete("Gunakan DeleteSaveLegacy().")]
    public void DeleteSave() => DeleteSaveLegacy();
}