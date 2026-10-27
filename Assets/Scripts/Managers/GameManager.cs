using UnityEngine;

/// Điều phối trạng thái tổng của game (Menu / Playing / Paused / Win / Lose), bật tắt các panel tương ứng, và là nơi các Button gọi vào.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Tham chiếu")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private LevelSession levelSession;
    [SerializeField] private PostLevelManager postLevelManager;
    [SerializeField] private GameConfig config;

    [Header("Panels")]
    [SerializeField] private MainMenuPanel mainMenuPanel;
    [SerializeField] private LevelSelectPanel levelSelectPanel;
    [SerializeField] private HUDPanel hudPanel;
    [SerializeField] private SettingPanel settingPanel;
    [SerializeField] private WinPanel winPanel;
    [SerializeField] private LosePanel losePanel;

    [Header("Test")]
    [SerializeField] private bool skipMenu = false; // bật để vào thẳng game, bỏ qua Main Menu

    public GameState State { get; private set; } = GameState.Menu;
    public event System.Action<GameState> StateChanged;

    private GameState _resumeState = GameState.Menu; // trạng thái quay lại khi đóng Setting
    private bool _runPostFlow; // vừa thắng lần đầu, bấm Next là phải xem dialogue (cấm tua)

    private void Awake() => Instance = this;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnEnable()
    {
        if (levelSession == null) return;
        levelSession.Won += OnWon;
        levelSession.Lost += OnLost;
    }

    private void OnDisable()
    {
        if (levelSession == null) return;
        levelSession.Won -= OnWon;
        levelSession.Lost -= OnLost;
    }

    private void Start()
    {
        if (skipMenu) StartGame();
        else SetState(GameState.Menu);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveManager.Save();
    }

    private void OnApplicationQuit() => SaveManager.Save();

    // ---------- Các hành động mà Button gọi ----------

    // Bắt đầu chơi từ level đang mở khóa cao nhất (có thể chơi lại level cũ)
    public void StartGame()
    {
        int count = levelManager.LevelCount;
        if(count == 0)
        {
            Debug.LogError("[GameManager] Danh sách level đang trống", this);
            return;
        }

        int highest = SaveManager.HighestUnlockedLevel;
        int index = highest >= count ? 0 : highest;
        if (levelManager.LoadLevel(index)) SetState(GameState.Playing);
    }

    public void OpenLevelSelect()
    {
        if(State != GameState.Menu) return;
        SetState(GameState.LevelSelect);
    }

    public void CloseLevelSelect()
    {
        if(State != GameState.LevelSelect) return;
        SetState(GameState.Menu);
    }

    // Chơi level được chọn trong bảng Level
    public void PlayLevel(int index)
    {
        if(State != GameState.LevelSelect && State != GameState.Menu) return;
        if(index < 0 || index >= levelManager.LevelCount) return;
        if(index > SaveManager.HighestUnlockedLevel) return; // level chưa mở

        if(levelManager.LoadLevel(index)) SetState(GameState.Playing);
    }

    public void RestartLevel()
    {
        if (State == GameState.Menu || State == GameState.LevelSelect || State == GameState.PostLevel) return;
        if (State == GameState.Paused && _resumeState == GameState.Menu) return;

        if (levelManager.ReloadCurrent()) SetState(GameState.Playing);
    }

    // Nút Next Level trên WinPanel. Luôn chiếu dialogue và vật phẩm (nếu có) để tiện test game liên tục
    public void NextLevel()
    {
        if (State != GameState.Win) return;

        LevelData level = levelManager.CurrentLevel;
        if(postLevelManager != null && postLevelManager.HasContent(level))
        {
            _runPostFlow = false;
            SetState(GameState.PostLevel);
            postLevelManager.StartFlow(level, GoToNextLevel);
            return;
        }
        _runPostFlow = false;
        GoToNextLevel();
    }

    private void GoToNextLevel()
    {
        bool loaded = levelManager.HasNext ? levelManager.LoadNext() : levelManager.LoadLevel(0);
        if(loaded) SetState(GameState.Playing);
        else SetState(GameState.Menu);
    }

    public void Undo()
    {
        if (State != GameState.Playing && State != GameState.Lose) return;
        if (!levelSession.CanUndo) return;

        levelSession.Undo();
        if (State == GameState.Lose) SetState(GameState.Playing);
    }

    public void OpenSettings()
    {
        if (State == GameState.Paused || State == GameState.Win || State == GameState.Lose || State == GameState.LevelSelect || State == GameState.PostLevel) return;

        _resumeState = State;
        SetState(GameState.Paused);
    }

    public void CloseSettings()
    {
        if (State != GameState.Paused) return;
        SetState(_resumeState);
    }

    public void GoToMenu()
    {
        if (postLevelManager != null) postLevelManager.Abort();
        SetState(GameState.Menu);
    }

    public void QuitGame()
    {
        SaveManager.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- Sự kiện từ LevelSession ----------

    private void OnWon()
    {
        int index = levelManager.CurrentIndex;
        LevelData level = levelManager.CurrentLevel;

        bool firstClear = index >= SaveManager.HighestUnlockedLevel;

        ScoreResult score = ScoreCalculator.Calculate(config, level, levelSession.ElapsedTime, levelSession.MoveCount, levelSession.UndoCount);

        int coins = 0;
        if(firstClear)
        {
            coins = config != null ? config.WinReward : 0;
            RewardData reward = level != null ? level.Reward : null;
            if(reward != null)
            {
                coins += reward.BonusCoins;
                SaveManager.AddItem(reward.ItemId);
            }
            SaveManager.HighestUnlockedLevel = index + 1;
            SaveManager.Coins += coins;
            SaveManager.Save();
        }

        _runPostFlow = true; // Luôn bật post-flow cốt truyện để test liên tục

        AudioManager.PlaySfx(Sfx.Win);

        if (winPanel != null) winPanel.Setup(index + 1, coins, score, firstClear, SaveManager.TotalScore, levelManager.HasNext);
        SetState(GameState.Win);
    }

    private void OnLost()
    {
        AudioManager.PlaySfx(Sfx.Lose);
        SetState(GameState.Lose);
    }

    // Trạng thái

    private void SetState(GameState next)
    {
        State = next;

        if (levelSession != null) levelSession.InputEnabled = next == GameState.Playing;

        bool inMenu = next == GameState.Menu || next == GameState.LevelSelect || (next == GameState.Paused && _resumeState == GameState.Menu);
        SetPanel(mainMenuPanel, inMenu);
        SetPanel(levelSelectPanel, next == GameState.LevelSelect);
        SetPanel(hudPanel, !inMenu && next != GameState.PostLevel && next != GameState.Win);
        SetPanel(settingPanel, next == GameState.Paused);
        SetPanel(winPanel, next == GameState.Win);
        SetPanel(losePanel, next == GameState.Lose);

        StateChanged?.Invoke(next);
    }

    private static void SetPanel(BasePanel panel, bool visible)
    {
        if (panel == null) return;

        if (visible) panel.Show();
        else panel.Hide();
    }
}
