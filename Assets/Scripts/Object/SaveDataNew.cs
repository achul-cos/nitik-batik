using System.Collections.Generic;
using UnityEngine;

// ============================================================================
// Kelas penyimpanan data utama
// ============================================================================

/// <summary>
/// Menyimpan seluruh data progres pemain, mencakup semua fase dan hari,
/// serta menyediakan berbagai metode helper untuk mengakses dan memanipulasi data.
/// </summary>
[System.Serializable]
public class SaveDataNew
{
    /// <summary>Daftar data semua fase.</summary>
    public List<PhaseSaveData> phaseSaveDatas = new List<PhaseSaveData>();

    /// <summary>Indeks fase yang sedang aktif.</summary>
    public int phaseIndex;

    // ------------------------------------------------------------------------
    // Metode helper untuk akses dan validasi
    // ------------------------------------------------------------------------

    /// <summary>
    /// Menentukan apakah fase dengan indeks tertentu terkunci.
    /// </summary>
    public bool IsPhaseLocked(int phaseIndex)
    {
        if (phaseIndex < 0 || phaseIndex >= phaseSaveDatas.Count)
            return true;

        if (phaseIndex == 0)
            return false;

        PhaseSaveData phaseSebelumnya = phaseSaveDatas[phaseIndex - 1];

        if (phaseSebelumnya == null)
            return false;

        int jumlahHariPhaseSebelumnya = phaseSebelumnya.daySaveDatas.Count;

        if (phaseSebelumnya.dayIndex + 1 >= jumlahHariPhaseSebelumnya)
            return false;

        return true;
    }

    /// <summary>
    /// Menghitung jumlah fase yang tersimpan.
    /// </summary>
    public int GetTotalPhase()
    {
        return phaseSaveDatas.Count;
    }

    // ------------------------------------------------------------------------
    // Properti untuk akses data saat ini
    // ------------------------------------------------------------------------

    /// <summary>
    /// Mendapatkan data fase saat ini berdasarkan <see cref="phaseIndex"/>.
    /// </summary>
    public PhaseSaveData CurrentPhase
    {
        get
        {
            if (phaseIndex < 0 || phaseIndex >= phaseSaveDatas.Count)
                return null;
            return phaseSaveDatas[phaseIndex];
        }
    }

    /// <summary>
    /// Mendapatkan data hari saat ini (dari fase saat ini dan indeks hari saat ini).
    /// </summary>
    public DaySaveData CurrentDay
    {
        get
        {
            if (CurrentPhase == null)
                return null;
            if (CurrentPhase.dayIndex < 0 || CurrentPhase.dayIndex >= CurrentPhase.daySaveDatas.Count)
                return null;
            return CurrentPhase.daySaveDatas[CurrentPhase.dayIndex];
        }
    }

    /// <summary>
    /// Menghitung indeks hari global dari posisi saat ini.
    /// </summary>
    public int CurrentGlobalDayIndex
    {
        get
        {
            int total = 0;
            for (int i = 0; i < phaseIndex && i < phaseSaveDatas.Count; i++)
            {
                total += phaseSaveDatas[i].daySaveDatas.Count;
            }
            if (CurrentPhase != null) total += CurrentPhase.dayIndex;
            return total;
        }
    }

    // ------------------------------------------------------------------------
    // Metode untuk mengubah posisi fase/hari
    // ------------------------------------------------------------------------

    public void SetCurrentPhase(int index)
    {
        if (index < 0) return;
        if (index >= phaseSaveDatas.Count) return;
        phaseIndex = index;
    }

    public void SetCurrentDay(int index)
    {
        if (CurrentPhase == null)
            return;
        if (index < 0)
            return;
        if (index >= CurrentPhase.daySaveDatas.Count)
            return;
        CurrentPhase.dayIndex = index;
    }

    public bool NextDay()
    {
        if (CurrentPhase == null)
            return false;
        if (CurrentPhase.dayIndex + 1 >= CurrentPhase.daySaveDatas.Count)
            return false;
        CurrentPhase.dayIndex++;
        return true;
    }

    public bool NextPhase()
    {
        if (phaseIndex + 1 >= phaseSaveDatas.Count)
            return false;
        phaseIndex++;
        return true;
    }

    // ------------------------------------------------------------------------
    // Metode untuk memperbarui data hari/fase
    // ------------------------------------------------------------------------

    /// <summary>
    /// Memperbarui data hari tertentu pada fase tertentu.
    /// </summary>
    public void SetDaySaveData(
        int phaseIndex,
        int dayIndex,
        SaveTime timestamp = null,   // ← Ganti Time → SaveTime
        int? customerServed = null,
        long? balance = null)
    {
        if (phaseIndex < 0 || phaseIndex >= phaseSaveDatas.Count)
            return;

        PhaseSaveData phase = phaseSaveDatas[phaseIndex];
        if (dayIndex < 0 || dayIndex >= phase.daySaveDatas.Count)
            return;

        DaySaveData day = phase.daySaveDatas[dayIndex];

        if (timestamp != null)
            day.timestamp = timestamp;
        if (customerServed.HasValue)
            day.customerServed = customerServed.Value;
        if (balance.HasValue)
            day.balance = balance.Value;
    }

    /// <summary>
    /// Memperbarui data hari saat ini dengan parameter opsional.
    /// </summary>
    public void SetCurrentDaySaveData(
        SaveTime timestamp = null,   // ← Ganti Time → SaveTime
        int? customerServed = null,
        long? balance = null)
    {
        if (CurrentPhase == null)
            return;

        SetDaySaveData(
            phaseIndex,
            CurrentPhase.dayIndex,
            timestamp,
            customerServed,
            balance);
    }

    /// <summary>
    /// Memperbarui data fase tertentu.
    /// </summary>
    public void SetPhaseSaveData(
        int phaseIndex,
        SaveTime timestamp = null,   // ← Ganti Time → SaveTime
        int? dayIndex = null)
    {
        if (phaseIndex < 0 || phaseIndex >= phaseSaveDatas.Count)
            return;

        PhaseSaveData phase = phaseSaveDatas[phaseIndex];
        if (timestamp != null)
            phase.timestamp = timestamp;
        if (dayIndex.HasValue)
            phase.dayIndex = dayIndex.Value;
    }

    /// <summary>
    /// Memperbarui data fase saat ini dengan parameter opsional.
    /// </summary>
    public void SetCurrentPhaseSaveData(
        SaveTime timestamp = null,   // ← Ganti Time → SaveTime
        int? dayIndex = null)
    {
        SetPhaseSaveData(phaseIndex, timestamp, dayIndex);
    }

    // ------------------------------------------------------------------------
    // Metode untuk pengecekan status
    // ------------------------------------------------------------------------

    public bool IsLastDay()
    {
        if (CurrentPhase == null)
            return false;

        return CurrentPhase.dayIndex == CurrentPhase.daySaveDatas.Count - 1;
    }

    public bool IsLastPhase()
    {
        return phaseIndex == phaseSaveDatas.Count - 1;
    }

    public bool IsGameFinished()
    {
        return IsLastPhase() && IsLastDay();
    }

    public bool CanGoNextDay()
    {
        return !IsLastDay();
    }

    public bool CanGoNextPhase()
    {
        return !IsLastPhase();
    }

    // ------------------------------------------------------------------------
    // Metode untuk agregasi data
    // ------------------------------------------------------------------------

    public long GetTotalBalance()
    {
        long total = 0;
        foreach (var phase in phaseSaveDatas)
        {
            foreach (var day in phase.daySaveDatas)
            {
                total += day.balance;
            }
        }
        return total;
    }

    public int GetTotalCustomerServed()
    {
        int total = 0;
        foreach (var phase in phaseSaveDatas)
        {
            foreach (var day in phase.daySaveDatas)
            {
                total += day.customerServed;
            }
        }
        return total;
    }

    public int GetPlayedDayCount()
    {
        int total = 0;
        foreach (var phase in phaseSaveDatas)
        {
            total += phase.dayIndex + 1;
        }
        return total;
    }

    // ------------------------------------------------------------------------
    // Metode untuk mengambil data mentah
    // ------------------------------------------------------------------------

    public PhaseSaveData GetPhaseSave(int phase)
    {
        if (phase < 0 || phase >= phaseSaveDatas.Count)
            return null;
        return phaseSaveDatas[phase];
    }

    public DaySaveData GetDaySave(int phase, int day)
    {
        PhaseSaveData phaseSave = GetPhaseSave(phase);
        if (phaseSave == null)
            return null;
        if (day < 0 || day >= phaseSave.daySaveDatas.Count)
            return null;
        return phaseSave.daySaveDatas[day];
    }

    public DaySaveData GetDaySaveByGlobalIndex(int globalIndex)
    {
        int current = 0;
        foreach (var phase in phaseSaveDatas)
        {
            if (globalIndex < current + phase.daySaveDatas.Count)
            {
                return phase.daySaveDatas[globalIndex - current];
            }
            current += phase.daySaveDatas.Count;
        }
        return null;
    }
}

// ============================================================================
// Kelas penyimpanan untuk fase dan hari
// ============================================================================

/// <summary>
/// Menyimpan data untuk satu fase (progres per hari). Konfigurasi fase/hari tidak disimpan di sini,
/// karena selalu dibaca dari <see cref="PhasesConfigDatabase"/> yang sedang dipakai game.
/// </summary>
[System.Serializable]
public class PhaseSaveData
{
    public List<DaySaveData> daySaveDatas = new List<DaySaveData>();
    public SaveTime timestamp;   // ← Ganti Time → SaveTime
    public int dayIndex;
}

/// <summary>
/// Menyimpan data untuk satu hari.
/// </summary>
[System.Serializable]
public class DaySaveData
{
    public SaveTime timestamp;   // ← Ganti Time → SaveTime
    public int customerServed;
    public long balance;
}

// ============================================================================
// Kelas untuk membuat data default
// ============================================================================

/// <summary>
/// Kelas pembantu untuk membuat instance <see cref="SaveDataNew"/> dengan data default.
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