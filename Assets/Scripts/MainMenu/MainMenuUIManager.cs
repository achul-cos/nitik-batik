using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Mengatur UI scene Main Menu: perpindahan panel (menu utama, pengaturan, kredit, pilih chapter),
/// konfirmasi keluar, dan pemilihan phase/chapter (klik atau tombol panah atas/bawah).
/// Pengaturan volume ditangani oleh komponen <see cref="PengaturanAudio"/> pada panel pengaturan.
/// </summary>
public class MainMenuUIManager : MonoBehaviour
{
    private const int JumlahPhase = 4;

    // ======================================================
    // Panel
    // ======================================================

    [Header("Panel")]
    [SerializeField] private GameObject _firstPanel;
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _settingPanel;
    [SerializeField] private GameObject _creditPanel;
    [SerializeField] private GameObject _chapterPanel;

    [Header("Sub Panel")]
    [SerializeField] private GameObject _quitSubPanel;

    // ======================================================
    // Tombol
    // ======================================================

    [Header("Main Menu Panel")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _creditButton;
    [SerializeField] private Button _settingButton;

    [Header("Tombol Kembali")]
    [SerializeField] private Button _settingBackButton;
    [SerializeField] private Button _creditBackButton;
    [SerializeField] private Button _chapterBackButton;

    [Header("Quit Sub Panel")]
    [SerializeField] private Button _quitSubPanelConfirmButton;
    [SerializeField] private Button _quitSubPanelCancelButton;

    // ======================================================
    // Pilih phase
    // ======================================================

    [Header("Pilih Phase")]
    public GameObject[] phaseSelections = new GameObject[JumlahPhase];
    public int phaseSelectionIndex = 1;
    public RawImage SelectionPhaseImage;
    public TextMeshProUGUI SelectionPhaseTitle;
    public TextMeshProUGUI SelectionPhaseDesc;
    public GameObject PhaseSelectionLock;
    public GameObject SelectionPhase;
    public Button PhaseSelectionPlayButton;

    private PhaseSelection[] _pilihanPhase;
    private GameObject[] _semuaPanel;
    private CanvasGroup _selectionPhaseGroup;
    private RectTransform _selectionPhaseRect;
    private Vector2 _posisiAwalSelectionPhase;

    // ======================================================
    // Lifecycle
    // ======================================================

    private void Awake()
    {
        _semuaPanel = new[] { _mainMenuPanel, _settingPanel, _creditPanel, _chapterPanel };

        CacheKomponenPilihPhase();

        LoadPanel(_firstPanel);
    }

    private void Start()
    {
        HubungkanTombol();

        GameManager.Instance.InitializeCurrentPhaseAndDay();

        SelectPhase();
    }

    private void Update()
    {
        UpdatePhaseSelection();
    }

    private void CacheKomponenPilihPhase()
    {
        _pilihanPhase = new PhaseSelection[phaseSelections.Length];

        for (int i = 0; i < phaseSelections.Length; i++)
        {
            _pilihanPhase[i] = phaseSelections[i].GetComponent<PhaseSelection>();
            _pilihanPhase[i].Inisialisasi(this);
        }

        if (SelectionPhase != null)
        {
            _selectionPhaseGroup = SelectionPhase.GetComponent<CanvasGroup>();
            _selectionPhaseRect = SelectionPhase.GetComponent<RectTransform>();

            // anchoredPosition tidak bergantung pada resolusi layar, berbeda dengan transform.position
            _posisiAwalSelectionPhase = _selectionPhaseRect.anchoredPosition;
        }
    }

    private void HubungkanTombol()
    {
        _playButton.onClick.AddListener(() => KlikPindahPanel(_chapterPanel));
        _settingButton.onClick.AddListener(() => KlikPindahPanel(_settingPanel));
        _creditButton.onClick.AddListener(() => KlikPindahPanel(_creditPanel));

        _settingBackButton.onClick.AddListener(() => KlikPindahPanel(_mainMenuPanel));
        _creditBackButton.onClick.AddListener(() => KlikPindahPanel(_mainMenuPanel));
        _chapterBackButton.onClick.AddListener(() => KlikPindahPanel(_mainMenuPanel));

        _quitButton.onClick.AddListener(OnQuitButton);
        _quitSubPanelCancelButton.onClick.AddListener(OnQuitSubPanelCancelButton);
        _quitSubPanelConfirmButton.onClick.AddListener(OnQuitSubPanelConfirmButton);

        PhaseSelectionPlayButton.onClick.AddListener(OnPhaseSelectionPlayButton);
    }

    // ======================================================
    // Panel
    // ======================================================

    /// <summary>
    /// Menampilkan satu panel utama dan menyembunyikan panel utama lainnya.
    /// </summary>
    public void LoadPanel(GameObject panel)
    {
        if (System.Array.IndexOf(_semuaPanel, panel) < 0)
        {
            Debug.LogWarning($"MainMenuUIManager [LoadPanel] : Panel {(panel != null ? panel.name : "null")} tidak terdaftar sebagai panel Main Menu.");
            return;
        }

        foreach (GameObject p in _semuaPanel)
        {
            p.SetActive(p == panel);
        }
    }

    private void KlikPindahPanel(GameObject panel)
    {
        AudioManager.Instance.PlaySFXName("click");

        LoadPanel(panel);
    }

    private void OnQuitButton()
    {
        AudioManager.Instance.PlaySFXName("click");

        _quitSubPanel.SetActive(true);
        AnimationManager.Instance.ClipOn(_quitSubPanel);
        AudioManager.Instance.PlaySFXName("sweep");
    }

    private void OnQuitSubPanelCancelButton()
    {
        AudioManager.Instance.PlaySFXName("click");

        AudioManager.Instance.PlaySFXName("sweep_2");
        AnimationManager.Instance.ClipOut(_quitSubPanel);
    }

    private void OnQuitSubPanelConfirmButton()
    {
        AudioManager.Instance.PlaySFXName("click");

        GameManager.Instance.Quit();
    }

    // ======================================================
    // Pilih phase
    // ======================================================

    private void OnPhaseSelectionPlayButton()
    {
        AudioManager.Instance.PlaySFXName("click");

        if (GameManager.Instance.development == true) GameManager.Instance.Play();
        else GameManager.Instance.CutScene();
    }

    public void SelectPhase()
    {
        phaseSelectionIndex = Mathf.Clamp(phaseSelectionIndex, 1, JumlahPhase);

        // Phase yang tidak dipilih kembali ke ukuran asli dan tanda pilihnya dimatikan
        for (int i = 0; i < _pilihanPhase.Length; i++)
        {
            PhaseSelection pilihan = _pilihanPhase[i];

            if (pilihan.PhaseIndex == phaseSelectionIndex) continue;

            pilihan.transform.DOKill();
            pilihan.transform.DOScale(pilihan.originalScale, 0.1f).SetEase(Ease.OutCubic);

            pilihan.selection.SetActive(false);
        }

        // Phase yang dipilih diperbesar
        PhaseSelection terpilih = _pilihanPhase[phaseSelectionIndex - 1];

        terpilih.selection.SetActive(true);

        terpilih.transform.DOKill();
        terpilih.transform.DOScale(terpilih.originalScale * 1.15f, 0.2f).SetEase(Ease.InCubic);

        TampilkanDetailPhase(GameManager.Instance.PhasesConfigDatabase.GetPhaseConfigByPhase(phaseSelectionIndex));

        // Phase terkunci kecuali phase yang sedang berjalan
        // (aturan buka-kunci dan putar ulang phase lama menunggu keputusan desain GDD)
        bool terkunci = GameManager.Instance.CurrentPhase != phaseSelectionIndex;

        PhaseSelectionLock.SetActive(terkunci);
        PhaseSelectionPlayButton.gameObject.SetActive(!terkunci);
    }

    private void TampilkanDetailPhase(PhaseConfig phase)
    {
        if (phase != null)
        {
            SelectionPhaseImage.texture = phase.phaseImage;
            SelectionPhaseTitle.text = phase.phaseName;
            SelectionPhaseDesc.text = phase.phaseDesc;
        }

        if (_selectionPhaseGroup == null || _selectionPhaseRect == null) return;

        // Animasi muncul dari kiri; tween lama dihentikan agar tidak saling tumpuk saat tombol ditekan cepat
        _selectionPhaseGroup.DOKill();
        _selectionPhaseRect.DOKill();

        _selectionPhaseGroup.alpha = 0f;
        _selectionPhaseGroup.DOFade(1.0f, 0.25f).SetEase(Ease.OutCubic);

        _selectionPhaseRect.anchoredPosition = _posisiAwalSelectionPhase + new Vector2(-40f, 0f);
        _selectionPhaseRect.DOAnchorPos(_posisiAwalSelectionPhase, 0.25f).SetEase(Ease.OutCubic);
    }

    public void SetPhaseSelection(int phase)
    {
        phaseSelectionIndex = phase;

        AudioManager.Instance.PlaySFXName("click");
        AudioManager.Instance.PlaySFXName("sweep", 0.5f);

        SelectPhase();
    }

    public void SelectDownPhase()
    {
        phaseSelectionIndex++;

        if (phaseSelectionIndex > JumlahPhase) phaseSelectionIndex = 1;

        AudioManager.Instance.PlaySFXName("sweep", 0.5f);

        SelectPhase();
    }

    public void SelectUpPhase()
    {
        phaseSelectionIndex--;

        if (phaseSelectionIndex < 1) phaseSelectionIndex = JumlahPhase;

        AudioManager.Instance.PlaySFXName("sweep", 0.5f);

        SelectPhase();
    }

    public void UpdatePhaseSelection()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SelectUpPhase();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SelectDownPhase();
        }
    }
}
