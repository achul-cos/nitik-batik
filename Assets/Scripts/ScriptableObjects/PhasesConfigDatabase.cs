using UnityEngine;

/// <summary>
/// Database konfigurasi seluruh phase dan hari di dalam game.
/// Semua nomor phase dan hari di class ini dimulai dari 1. Nomor hari bersifat global:
/// hari terakhir phase sebelumnya dilanjutkan oleh hari pertama phase berikutnya
/// (misal phase 1 punya 3 hari, maka hari ke-4 adalah hari pertama phase 2).
/// </summary>
[CreateAssetMenu(fileName = "PhasesConfigDatabases", menuName = "AksaraBatik/PhasesConfigDatabases")]
public class PhasesConfigDatabase : ScriptableObject
{
    public PhaseConfig[] phaseConfigs = new PhaseConfig[0];

    // ======================================================
    // Jumlah
    // ======================================================

    /// <summary>
    /// Memberikan jumlah phase didalam game
    /// </summary>
    public int GetPhaseLength()
    {
        return phaseConfigs != null ? phaseConfigs.Length : 0;
    }

    /// <summary>
    /// Memberikan berapa banyak hari pada suatu phase (0 jika phase tidak valid)
    /// </summary>
    public int GetPhaseLengthDay(int phase)
    {
        if (!PhaseValid(phase))
        {
            Debug.LogError($"PhasesConfigDatabase [GetPhaseLengthDay] : Phase ke-{phase} tidak valid (jumlah phase: {GetPhaseLength()}).");
            return 0;
        }

        return JumlahHari(phaseConfigs[phase - 1]);
    }

    /// <summary>
    /// Memberikan jumlah seluruh hari dari semua phase
    /// </summary>
    public int GetDayLength()
    {
        int total = 0;

        for (int i = 0; i < GetPhaseLength(); i++)
        {
            total += JumlahHari(phaseConfigs[i]);
        }

        return total;
    }

    // ======================================================
    // Nomor hari global
    // ======================================================

    /// <summary>
    /// Nomor hari (global) pertama pada phase yang diberikan, atau 0 jika phase tidak valid.
    /// </summary>
    public int GetPhaseStartDay(int phase)
    {
        if (!PhaseValid(phase))
        {
            Debug.LogWarning($"PhasesConfigDatabase [GetPhaseStartDay] : Tidak ditemukan phase ke-{phase}");
            return 0;
        }

        int hari = 0;

        for (int i = 0; i < phase - 1; i++)
        {
            hari += JumlahHari(phaseConfigs[i]);
        }

        return hari + 1;
    }

    /// <summary>
    /// Nomor hari (global) terakhir pada phase yang diberikan, atau 0 jika phase tidak valid.
    /// </summary>
    public int GetPhaseEndDay(int phase)
    {
        if (!PhaseValid(phase))
        {
            Debug.LogWarning($"PhasesConfigDatabase [GetPhaseEndDay] : Tidak ditemukan phase ke-{phase}");
            return 0;
        }

        return GetPhaseStartDay(phase) + JumlahHari(phaseConfigs[phase - 1]) - 1;
    }

    /// <summary>
    /// Memberikan nomor phase berdasarkan nomor hari global, atau 0 jika hari tidak valid.
    /// </summary>
    public int GetPhaseFromDay(int day)
    {
        return CariPosisi(day, out int phase, out _) ? phase : 0;
    }

    // ======================================================
    // Pengambilan data
    // ======================================================

    /// <summary>
    /// Memberikan data phase berdasarkan nomor phase, atau null jika tidak valid.
    /// </summary>
    public PhaseConfig GetPhaseConfigByPhase(int phase)
    {
        if (!PhaseValid(phase))
        {
            Debug.LogError($"PhasesConfigDatabase [GetPhaseConfigByPhase] : Tidak dapat memberikan data phase ke-{phase}, jumlah phase di game {GetPhaseLength()} (nomor mulai dari 1).");
            return null;
        }

        return phaseConfigs[phase - 1];
    }

    /// <summary>
    /// Memberikan data phase yang memuat hari global tertentu, atau null jika hari tidak valid.
    /// </summary>
    public PhaseConfig GetPhaseConfigByDay(int day)
    {
        if (!CariPosisi(day, out int phase, out _))
        {
            Debug.LogError($"PhasesConfigDatabase [GetPhaseConfigByDay] : Hari ke-{day} tidak valid pada phase apa pun (jumlah hari di game {GetDayLength()}).");
            return null;
        }

        return phaseConfigs[phase - 1];
    }

    /// <summary>
    /// Memberikan data hari berdasarkan nomor hari global, atau null jika tidak valid.
    /// </summary>
    public DayConfig GetDayConfig(int day)
    {
        if (!CariPosisi(day, out int phase, out int hariDiPhase))
        {
            Debug.LogError($"PhasesConfigDatabase [GetDayConfig] : Hari ke-{day} tidak valid (jumlah hari di game {GetDayLength()}, nomor mulai dari 1).");
            return null;
        }

        return phaseConfigs[phase - 1].phaseDay[hariDiPhase - 1];
    }

    /// <summary>
    /// Memberikan data hari berdasarkan nomor phase dan nomor hari di dalam phase itu (keduanya mulai dari 1), atau null jika tidak valid.
    /// </summary>
    public DayConfig GetDayConfigByPhase(int phase, int day)
    {
        if (!PhaseValid(phase))
        {
            Debug.LogError($"PhasesConfigDatabase [GetDayConfigByPhase] : Phase ke-{phase} tidak valid.");
            return null;
        }

        PhaseConfig phaseConfig = phaseConfigs[phase - 1];

        if (day < 1 || day > JumlahHari(phaseConfig))
        {
            Debug.LogError($"PhasesConfigDatabase [GetDayConfigByPhase] : Hari ke-{day} pada phase ke-{phase} tidak valid.");
            return null;
        }

        return phaseConfig.phaseDay[day - 1];
    }

    /// <summary>
    /// Memberikan nama hari (dayName) berdasarkan nomor hari global, atau null jika tidak valid.
    /// </summary>
    public string GetDayName(int day)
    {
        return GetDayConfig(day)?.dayName;
    }

    // ======================================================
    // Helper
    // ======================================================

    private bool PhaseValid(int phase)
    {
        return phase >= 1 && phase <= GetPhaseLength();
    }

    private static int JumlahHari(PhaseConfig phaseConfig)
    {
        return phaseConfig != null && phaseConfig.phaseDay != null ? phaseConfig.phaseDay.Length : 0;
    }

    /// <summary>
    /// Mencari phase (mulai dari 1) dan hari di dalam phase itu (mulai dari 1) untuk sebuah nomor hari global.
    /// </summary>
    private bool CariPosisi(int day, out int phase, out int hariDiPhase)
    {
        phase = 0;
        hariDiPhase = 0;

        if (day < 1) return false;

        int sisa = day;

        for (int i = 0; i < GetPhaseLength(); i++)
        {
            int jumlahHari = JumlahHari(phaseConfigs[i]);

            if (sisa <= jumlahHari)
            {
                phase = i + 1;
                hariDiPhase = sisa;
                return true;
            }

            sisa -= jumlahHari;
        }

        return false;
    }
}
