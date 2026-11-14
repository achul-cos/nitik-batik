using System;

/// <summary>
/// Representasi waktu yang digunakan dalam data save.
/// Nama SaveTime digunakan agar tidak bentrok dengan UnityEngine.Time.
/// </summary>
[System.Serializable]
public class SaveTime
{
    public string time;

    public SaveTime()
    {
        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public SaveTime(string timeString)
    {
        time = timeString;
    }
}