using System.Collections.Generic;
using UnityEngine;

// ============================================================================
// Data save
// ============================================================================

/// <summary>
/// Menyimpan progres pemain: posisi fase/hari dan data per hari (uang, customer terlayani).
/// Konfigurasi fase/hari tidak disimpan di sini; selalu dibaca dari <see cref="PhasesConfigDatabase"/>.
/// </summary>
[System.Serializable]
public class SaveDataNew
{
    /// <summary>Daftar data semua fase.</summary>
    public List<PhaseSaveData> phaseSaveDatas = new List<PhaseSaveData>();

    /// <summary>Indeks fase yang sedang aktif (mulai dari 0).</summary>
    public int phaseIndex;

    /// <summary>Data fase yang sedang aktif, atau null jika indeks di luar batas.</summary>
    public PhaseSaveData CurrentPhase => GetPhaseSave(phaseIndex);

    /// <summary>Mengambil data fase berdasarkan indeks (mulai dari 0), atau null jika di luar batas.</summary>
    public PhaseSaveData GetPhaseSave(int phase)
    {
        if (phase < 0 || phase >= phaseSaveDatas.Count)
            return null;

        return phaseSaveDatas[phase];
    }
}

/// <summary>
/// Menyimpan data untuk satu fase (progres per hari).
/// </summary>
[System.Serializable]
public class PhaseSaveData
{
    public List<DaySaveData> daySaveDatas = new List<DaySaveData>();
    public SaveTime timestamp;
    public int dayIndex;
}

/// <summary>
/// Menyimpan data untuk satu hari.
/// </summary>
[System.Serializable]
public class DaySaveData
{
    public SaveTime timestamp;
    public int customerServed;
    public long balance;
}

// ============================================================================
// Pembuatan dan penyelarasan data
// ============================================================================

/// <summary>
/// Kelas pembantu untuk membuat dan menyelaraskan <see cref="SaveDataNew"/> dengan konfigurasi game.
/// </summary>
public static class SaveDataNewDefault
{
    /// <summary>
    /// Membuat save baru dengan jumlah fase dan hari yang sama seperti <paramref name="phaseConfigs"/>.
    /// </summary>
    public static SaveDataNew Create(PhaseConfig[] phaseConfigs)
    {
        var saveData = new SaveDataNew
        {
            phaseIndex = 0,
            phaseSaveDatas = new List<PhaseSaveData>()
        };

        Sinkronkan(saveData, phaseConfigs);

        return saveData;
    }

    /// <summary>
    /// Menyamakan struktur save (jumlah fase dan hari) dengan konfigurasi game yang sekarang.
    /// Dipakai agar save lama tidak menyebabkan indeks di luar batas ketika konfigurasi berubah.
    /// Data yang sudah ada dipertahankan; fase/hari baru ditambahkan, yang berlebih dibuang.
    /// </summary>
    public static void Sinkronkan(SaveDataNew saveData, PhaseConfig[] phaseConfigs)
    {
        if (saveData == null || phaseConfigs == null) return;

        saveData.phaseSaveDatas ??= new List<PhaseSaveData>();

        while (saveData.phaseSaveDatas.Count < phaseConfigs.Length)
            saveData.phaseSaveDatas.Add(null);

        if (saveData.phaseSaveDatas.Count > phaseConfigs.Length)
            saveData.phaseSaveDatas.RemoveRange(phaseConfigs.Length, saveData.phaseSaveDatas.Count - phaseConfigs.Length);

        for (int p = 0; p < phaseConfigs.Length; p++)
        {
            PhaseSaveData phaseSave = saveData.phaseSaveDatas[p];

            if (phaseSave == null)
            {
                phaseSave = new PhaseSaveData { timestamp = new SaveTime() };
                saveData.phaseSaveDatas[p] = phaseSave;
            }

            phaseSave.daySaveDatas ??= new List<DaySaveData>();

            int jumlahHari = phaseConfigs[p].phaseDay != null ? phaseConfigs[p].phaseDay.Length : 0;

            while (phaseSave.daySaveDatas.Count < jumlahHari)
                phaseSave.daySaveDatas.Add(new DaySaveData { timestamp = new SaveTime(), customerServed = 0, balance = 0 });

            if (phaseSave.daySaveDatas.Count > jumlahHari)
                phaseSave.daySaveDatas.RemoveRange(jumlahHari, phaseSave.daySaveDatas.Count - jumlahHari);

            for (int d = 0; d < phaseSave.daySaveDatas.Count; d++)
            {
                if (phaseSave.daySaveDatas[d] == null)
                    phaseSave.daySaveDatas[d] = new DaySaveData { timestamp = new SaveTime() };
            }

            phaseSave.dayIndex = jumlahHari > 0 ? Mathf.Clamp(phaseSave.dayIndex, 0, jumlahHari - 1) : 0;
        }

        saveData.phaseIndex = phaseConfigs.Length > 0 ? Mathf.Clamp(saveData.phaseIndex, 0, phaseConfigs.Length - 1) : 0;
    }
}
