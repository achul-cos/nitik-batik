using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Satu kartu pilihan phase di Main Menu. Diinisialisasi oleh <see cref="MainMenuUIManager"/>.
/// </summary>
public class PhaseSelection : MonoBehaviour, IPointerClickHandler
{
    public int PhaseIndex;

    public Vector3 originalScale;

    public GameObject selection;

    [HideInInspector] public MainMenuUIManager mainMenuUIManager;

    private bool _skalaTersimpan;

    private void Awake()
    {
        SimpanSkalaAsli();
    }

    /// <summary>
    /// Dipanggil manager sebelum animasi apa pun berjalan. Aman dipanggil walau objek ini belum pernah aktif.
    /// </summary>
    public void Inisialisasi(MainMenuUIManager manager)
    {
        mainMenuUIManager = manager;
        SimpanSkalaAsli();
    }

    // Skala asli hanya disimpan sekali, supaya skala hasil animasi (diperbesar) tidak ikut tersimpan sebagai skala asli.
    private void SimpanSkalaAsli()
    {
        if (_skalaTersimpan) return;

        originalScale = transform.localScale;
        _skalaTersimpan = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (mainMenuUIManager != null) mainMenuUIManager.SetPhaseSelection(PhaseIndex);
    }
}
