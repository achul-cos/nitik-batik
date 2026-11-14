using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

/// <summary>
/// GameManager adalah Singleton utama yang mengkoordinasikan seluruh state game,
/// meliputi fase, hari, saldo, alur scene, dan manajemen save/load dengan format baru.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    // ======================================================
    // Singleton
    // ======================================================

    protected override bool PersistBetweenScenes => true;

    // ======================================================
    // State & Data
    // ======================================================

    private GameState _state = GameState.MainMenu;
    public GameState State
    {
        get => _state;
        private set => _state = value;
    }

    [Header("Game Data")]
    [SerializeField] private int _currentPhase = 1;
    [SerializeField] private int _currentDay = 1;
    [SerializeField] private long _currentBalance = 0;
    [SerializeField] public bool development = false;

    [Header("Phase Configuration")]
    [Expandable, SerializeField] private PhasesConfigDatabase _phasesConfigDatabase;

    private bool _isGameOver = false;
    private bool _isPaused = false;

    // ======================================================
    // Save Data (NEW)
    // ======================================================

    /// <summary>
    /// Menyimpan referensi ke objek SaveDataNew yang sedang dimuat/digunakan.
    /// Dapat diakses oleh komponen lain untuk membaca/memanipulasi data save secara langsung.
    /// </summary>
    private SaveDataNew _currentSaveData;

    // ======================================================
    // Events
    // ======================================================

    public delegate void OnGameStateChanged();
    public delegate void DialogStartedHandler(Customer customer);

    public event OnGameStateChanged OnPhaseChanged;
    public event OnGameStateChanged OnDayChanged;
    public event OnGameStateChanged OnBalanceChanged;
    public event OnGameStateChanged OnGameOver;
    public event OnGameStateChanged OnPauseGame;
    public event DialogStartedHandler OnDialogStarted;

    // ======================================================
    // Properties
    // ======================================================

    public int CurrentPhase => _currentPhase;
    public int CurrentDay => _currentDay;
    public long CurrentBalance => _currentBalance;
    public bool IsGameOver => _isGameOver;
    public bool IsPaused => _isPaused; // Perbaikan: IsPause → IsPaused
    public PhasesConfigDatabase PhasesConfigDatabase => _phasesConfigDatabase;
    public bool IsGamePlay => !(TimeManager.Instance?.IsDayEnded ?? true);
    public string SceneName => _state.ToString();

    // ======================================================
    // Lifecycle
    // ======================================================

    protected override void Awake()
    {
        base.Awake();
        InitializePhasesConfig();
    }

    private void Start()
    {
        AudioManager.Instance?.PlayBGMName("satu");
    }

    // ======================================================
    // Inisialisasi
    // ======================================================

    private void InitializePhasesConfig()
    {
        if (_phasesConfigDatabase == null)
        {
            // Tanpa database, game tidak punya data fase/hari. Buat database kosong agar tidak null-reference,
            // tetapi beri error yang jelas: isi field "Phases Config Database" pada GameManager di scene 00_MainMenu.
            Debug.LogError("GameManager : PhasesConfigDatabase belum diisi di Inspector. Game tidak punya data fase dan hari.");
            _phasesConfigDatabase = ScriptableObject.CreateInstance<PhasesConfigDatabase>();
            _phasesConfigDatabase.phaseConfigs = new PhaseConfig[0];
        }
    }

    public void InitializeCurrentPhaseAndDay()
    {
        if (SaveManager.Instance.HasSaveNew())
            LoadGameDataNew();
        else
            InitializeNewGame();
    }

    private void InitializeNewGame()
    {
        _currentPhase = 1;
        _currentDay = 1;
        _currentBalance = 0;
        _isGameOver = false;
        _isPaused = false;

        // Buat save data baru dan simpan referensi
        _currentSaveData = SaveManager.Instance.CreateNewSave(_phasesConfigDatabase.phaseConfigs);

        Debug.Log($"GameManager : Game baru dimulai pada Phase {_currentPhase}, Day {_currentDay}, Balance {_currentBalance}");
    }

    public void LoadGameDataNew()
    {
        SaveDataNew saveData = SaveManager.Instance.LoadGameNew();
        if (saveData != null)
        {
            // Samakan struktur save dengan konfigurasi game sekarang (jumlah fase/hari bisa berubah antar versi)
            SaveDataNewDefault.Sinkronkan(saveData, _phasesConfigDatabase.phaseConfigs);

            _currentSaveData = saveData; // Simpan referensi

            _currentPhase = saveData.phaseIndex + 1; // 0-based → 1-based

            PhaseSaveData currentPhaseSave = saveData.CurrentPhase;
            if (currentPhaseSave != null)
            {
                int phaseStartDay = GetPhaseStartDay(_currentPhase);
                _currentDay = phaseStartDay + currentPhaseSave.dayIndex;

                DaySaveData currentDaySave = (currentPhaseSave.dayIndex >= 0 && currentPhaseSave.dayIndex < currentPhaseSave.daySaveDatas.Count)
                    ? currentPhaseSave.daySaveDatas[currentPhaseSave.dayIndex]
                    : null;
                if (currentDaySave != null)
                    _currentBalance = currentDaySave.balance;
                else
                    _currentBalance = 0;
            }
            else
            {
                _currentDay = 1;
                _currentBalance = 0;
            }

            _isGameOver = false;
            _isPaused = false;
            Debug.Log($"GameManager : Load SaveDataNew sukses. Phase {_currentPhase}, Day {_currentDay}, Balance {_currentBalance}");
        }
        else
        {
            Debug.LogWarning("GameManager : SaveDataNew tidak ditemukan, memulai game baru.");
            InitializeNewGame();
        }
    }

    // ======================================================
    // Save
    // ======================================================

    /// <summary>
    /// Menyimpan data game saat ini ke format baru (<see cref="SaveDataNew"/>).
    /// </summary>
    public void SaveGameData()
    {
        // Pastikan _currentSaveData tidak null
        if (_currentSaveData == null)
        {
            _currentSaveData = SaveManager.Instance.CreateNewSave(_phasesConfigDatabase.phaseConfigs);
        }

        // Update posisi fase dan hari
        _currentSaveData.phaseIndex = _currentPhase - 1;

        PhaseSaveData phaseSave = _currentSaveData.GetPhaseSave(_currentSaveData.phaseIndex);
        if (phaseSave != null)
        {
            int phaseStartDay = GetPhaseStartDay(_currentPhase);
            int dayIndexInPhase = _currentDay - phaseStartDay;
            phaseSave.dayIndex = Mathf.Clamp(dayIndexInPhase, 0, phaseSave.daySaveDatas.Count - 1);

            DaySaveData daySave = phaseSave.daySaveDatas[phaseSave.dayIndex];
            if (daySave != null)
            {
                daySave.balance = _currentBalance;
                daySave.timestamp = new SaveTime(); // Menggunakan konstruktor Time()
                // customerServed bisa diupdate dari CustomerManager jika diperlukan
            }
        }

        SaveManager.Instance.SaveGameNew(_currentSaveData);
        Debug.Log($"GameManager : Data game disimpan. Phase {_currentPhase}, Day {_currentDay}, Balance {_currentBalance}");
    }

    // ======================================================
    // Manipulasi Data
    // ======================================================

    public void AddBalance(long amount)
    {
        _currentBalance += amount;
        OnBalanceChanged?.Invoke();
        SaveGameData();
    }

    public void SubtractBalance(long amount)
    {
        _currentBalance = Math.Max(0, _currentBalance - amount);
        OnBalanceChanged?.Invoke();
        SaveGameData();
    }

    public void NextDay()
    {
        _currentDay++;
        if (IsPhaseEnd()) NextPhase();

        OnDayChanged?.Invoke();

        // Setelah fase terakhir selesai, game berakhir; jangan memulai hari baru.
        if (_isGameOver) return;

        SaveGameData();
        Play();
        Debug.Log($"GameManager : Pindah ke hari {_currentDay} di Phase {_currentPhase}");
    }

    public void NextPhase()
    {
        int totalPhase = GetTotalPhase();
        if (_currentPhase < totalPhase)
        {
            _currentPhase++;
            _currentDay = GetPhaseStartDay(_currentPhase);
            OnPhaseChanged?.Invoke();
            SaveGameData();
            Debug.Log($"GameManager : Pindah ke Phase {_currentPhase}, Day {_currentDay}");
        }
        else
        {
            _isGameOver = true;
            OnGameOver?.Invoke();
            Debug.Log("GameManager : Game selesai! Semua fase telah dilalui.");
        }
    }

    public void ResetToPhaseStart()
    {
        _currentDay = GetPhaseStartDay(_currentPhase);
        _isGameOver = false;
        OnDayChanged?.Invoke();
        SaveGameData();
        Debug.Log($"GameManager : Reset ke awal Phase {_currentPhase}, Day {_currentDay}");
    }

    public void GameOver()
    {
        _isGameOver = true;
        OnGameOver?.Invoke();
        Debug.Log($"GameManager : Game Over di Phase {_currentPhase}");
    }

    // ======================================================
    // Pause / Resume
    // ======================================================

    public void Pause()
    {
        TimeManager.Instance?.PauseGameTime();
        CustomerManager.Instance?.StopSpawning();
        _isPaused = true;
        OnPauseGame?.Invoke();
        Debug.Log("GameManager : Game dijeda.");
    }

    public void Resume()
    {
        TimeManager.Instance?.ResumeGameTime();
        CustomerManager.Instance?.StartSpawning(true);
        _isPaused = false;
        OnPauseGame?.Invoke();
        Debug.Log("GameManager : Game dilanjutkan.");
    }

    public void TogglePause()
    {
        if (_isPaused) Resume();
        else Pause();
    }

    // ======================================================
    // Navigasi Scene
    // ======================================================

    public void LoadGameScene(
        GameState gs,
        bool isTransition = true,
        float transitionDuration = 1f,
        float delayTransition = 1f,
        float delayToTransition = 0f,
        string sceneBGM = "",
        bool isBGMFadeOut = false)
    {
        string sceneName = GetSceneNameFromState(gs);
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"GameManager : Scene untuk state {gs} tidak ditemukan.");
            return;
        }

        if (!isTransition)
            SceneManager.LoadScene(sceneName);
        else
            SceneTransitionManager.Instance?.LoadScene(sceneName, transitionDuration, delayTransition, delayToTransition);

        if (!string.IsNullOrEmpty(sceneBGM))
            AudioManager.Instance?.PlayBGMName(sceneBGM, isFadeOut: isBGMFadeOut);
        else if (isBGMFadeOut)
            AudioManager.Instance?.StopBGM(true);

        State = gs;
    }

    private string GetSceneNameFromState(GameState gs)
    {
        return gs switch
        {
            GameState.MainMenu => "00_MainMenu",
            GameState.CutScene => "01_CutScene",
            GameState.Lobby => "02_Lobby",
            GameState.Dialog => "03_Dialog",
            GameState.ChoosingFabric => "04_ChoosingFabric",
            GameState.Drawing => "05_Drawing",
            GameState.Boiling => "07_Boiling",
            GameState.Drying => "08_Drying",
            _ => null
        };
    }

    // ======================================================
    // Shortcut Navigasi
    // ======================================================

    public void CutScene() => LoadGameScene(GameState.CutScene);

    public void Play()
    {
        // Hari yang dimuat dari save dipakai apa adanya (GDD 1.1: hari berjalan diulang dari awal, bukan dari awal phase).
        InitializeCurrentPhaseAndDay();
        LoadGameScene(GameState.Lobby, sceneBGM: "dua", isBGMFadeOut: true);
        TimeManager.Instance?.HandleDayStart();
        CustomerManager.Instance?.StartSpawning();
    }

    public void Exit()
    {
        SaveGameData();
        LoadGameScene(GameState.MainMenu, sceneBGM: "satu", isBGMFadeOut: true);
        TimeManager.Instance?.HandleDayEnd();
        CustomerManager.Instance?.StopSpawning();
    }

    public void Quit()
    {
        Debug.Log("Game is Quit");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Memproses dialog dengan customer selanjutnya.
    /// Menampilkan UI dialog jika ada customer yang bisa dilayani.
    /// </summary>
    /// <returns>True jika ada customer dan dialog ditampilkan, false jika tidak ada customer.</returns>
    public bool NextDialog()
    {
        // Cek apakah ada customer saat ini
        if (CustomerManager.Instance.CustomerCurrent == null)
        {
            // Ambil customer berikutnya dari antrian
            Customer currentCustomer = CustomerManager.Instance.GetNextCustomer();

            // Jika ada customer, tampilkan dialog
            if (currentCustomer != null)
            {
                // Dialog ditampilkan oleh Lobby (DialogPanel) yang subscribe ke event ini
                OnDialogStarted?.Invoke(currentCustomer);

                return true;
            }
            else
            {
                Debug.Log("GameManager : Tidak ada customer dalam antrian.");
                return false;
            }
        }
        else
        {
            // Jika sudah ada customer yang sedang dilayani
            Debug.Log("GameManager : Customer sedang dilayani.");
            return false;
        }
    }
    public void NextFabric() => LoadGameScene(GameState.ChoosingFabric, transitionDuration: 0.25f, delayToTransition: 0.25f);
    public void NextBatik() => LoadGameScene(GameState.Drawing, delayTransition: 0.25f, transitionDuration: 0.25f, delayToTransition: 1f);
    public void NextRebus() => LoadGameScene(GameState.Boiling, delayTransition: 0.25f, transitionDuration: 0.25f, delayToTransition: 1f);
    public void NextDrying() => LoadGameScene(GameState.Drying, delayTransition: 0.25f, transitionDuration: 0.25f, delayToTransition: 1f);
    public void NextResult() => LoadGameScene(GameState.Lobby, delayTransition: 0.25f, transitionDuration: 0.25f, delayToTransition: 1f);

    // ======================================================
    // Helper untuk Konfigurasi
    // ======================================================

    public DayConfig GetCurrentDayConfig() => _phasesConfigDatabase?.GetDayConfig(_currentDay);

    public DayConfig GetNextDayConfig()
    {
        int nextDay = _currentDay + 1;
        if (nextDay > GetTotalDay())
        {
            Debug.LogWarning("GameManager : Tidak ada hari berikutnya.");
            return null;
        }
        return _phasesConfigDatabase?.GetDayConfig(nextDay);
    }

    public PhaseConfig GetNextPhaseConfig()
    {
        int nextPhase = _currentPhase + 1;
        if (nextPhase <= GetTotalPhase())
            return _phasesConfigDatabase.GetPhaseConfigByPhase(nextPhase);
        return null;
    }

    public DayConfig GetDayConfigByIndex(int dayIndex)
    {
        if (dayIndex < 1 || dayIndex > GetTotalDay())
        {
            Debug.LogWarning($"GameManager : Indeks hari {dayIndex} tidak valid.");
            return null;
        }
        return _phasesConfigDatabase?.GetDayConfig(dayIndex);
    }

    // ======================================================
    // Perhitungan Internal
    // ======================================================

    private int GetTotalPhase() => _phasesConfigDatabase.GetPhaseLength();

    private int GetTotalDay() => _phasesConfigDatabase.GetDayLength();

    private int GetPhaseStartDay(int phase) => Mathf.Max(1, _phasesConfigDatabase.GetPhaseStartDay(phase));

    private bool IsPhaseEnd() => _currentDay > _phasesConfigDatabase.GetPhaseEndDay(_currentPhase);
}