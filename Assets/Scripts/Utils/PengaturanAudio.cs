using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel pengaturan volume (master, musik, SFX). Dipakai bersama oleh menu utama dan menu jeda.
/// Pasang di GameObject panel pengaturan lalu isi slider dan labelnya di Inspector.
/// Nilai slider disegarkan dari <see cref="AudioManager"/> setiap panel dibuka.
/// </summary>
public class PengaturanAudio : MonoBehaviour
{
    [Header("Master")]
    [SerializeField] private Slider _sliderMaster;
    [SerializeField] private TextMeshProUGUI _labelMaster;

    [Header("Musik")]
    [SerializeField] private Slider _sliderMusik;
    [SerializeField] private TextMeshProUGUI _labelMusik;

    [Header("SFX")]
    [SerializeField] private Slider _sliderSfx;
    [SerializeField] private TextMeshProUGUI _labelSfx;

    private bool _terhubung;

    private void Awake()
    {
        Hubungkan();
    }

    private void OnEnable()
    {
        Hubungkan();
        SegarkanDariAudioManager();
    }

    private void OnDestroy()
    {
        if (_sliderMaster != null) _sliderMaster.onValueChanged.RemoveListener(OnMasterBerubah);
        if (_sliderMusik != null) _sliderMusik.onValueChanged.RemoveListener(OnMusikBerubah);
        if (_sliderSfx != null) _sliderSfx.onValueChanged.RemoveListener(OnSfxBerubah);
        _terhubung = false;
    }

    private void Hubungkan()
    {
        if (_terhubung) return;

        if (_sliderMaster != null) _sliderMaster.onValueChanged.AddListener(OnMasterBerubah);
        if (_sliderMusik != null) _sliderMusik.onValueChanged.AddListener(OnMusikBerubah);
        if (_sliderSfx != null) _sliderSfx.onValueChanged.AddListener(OnSfxBerubah);

        _terhubung = true;
    }

    /// <summary>
    /// Menyamakan slider dan label dengan volume yang sedang berlaku di AudioManager (tanpa memicu perubahan).
    /// </summary>
    public void SegarkanDariAudioManager()
    {
        AudioManager audio = AudioManager.Instance;

        if (audio == null) return;

        Tampilkan(_sliderMaster, _labelMaster, audio.MasterVolume);
        Tampilkan(_sliderMusik, _labelMusik, audio.MusicVolumeSetting);
        Tampilkan(_sliderSfx, _labelSfx, audio.SfxVolumeSetting);
    }

    private void OnMasterBerubah(float nilai)
    {
        AudioManager.Instance.MasterVolume = nilai;
        Tampilkan(null, _labelMaster, AudioManager.Instance.MasterVolume);
    }

    private void OnMusikBerubah(float nilai)
    {
        AudioManager.Instance.MusicVolume = nilai;
        Tampilkan(null, _labelMusik, AudioManager.Instance.MusicVolumeSetting);
    }

    private void OnSfxBerubah(float nilai)
    {
        AudioManager.Instance.SfxVolume = nilai;
        Tampilkan(null, _labelSfx, AudioManager.Instance.SfxVolumeSetting);
    }

    private static void Tampilkan(Slider slider, TextMeshProUGUI label, float nilai)
    {
        if (slider != null) slider.SetValueWithoutNotify(nilai);
        if (label != null) label.text = $"{Mathf.FloorToInt(nilai * 100f)}%";
    }
}
