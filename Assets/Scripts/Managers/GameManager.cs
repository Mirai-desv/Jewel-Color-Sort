using UnityEngine;

/// Điều phối trạng thái tổng của game (Menu / Playing / Paused / Win / Lose), bật tắt các panel tương ứng, và là nơi các Button gọi vào.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Tham chiếu")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private LevelSession levelSession;
    [SerializeField] private GameConfig config;

    [Header("Panels")]
    [SerializeField] private MainMenuPanel mainMenuPanel;
    [SerializeField] private HUDPanel hudPanel;
    [SerializeField] private SettingPanel settingPanel;
    [SerializeField] private WinPanel winPanel;
    [SerializeField] private LosePanel losePanel;

    [Header("Test")]
    [SerializeField] private bool skipMenu = false; // bật để vào thẳng game, bỏ qua Main Menu

    public GameState State { get; private set; } = GameState.Menu;
    public event System.Action<GameState> StateChanged;

    private GameState _resumeState = GameState.Menu; // trạng thái quay lại khi đóng Setting

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

    /// <summary>Bắt đầu chơi từ level đã lưu.</summary>
    public void StartGame()
    {
        if (levelManager.LevelCount == 0)
        {
            Debug.LogError("[GameManager] Danh sách level trống.", this);
            return;
        }

        int index = Mathf.Clamp(SaveManager.CurrentLevelIndex, 0, levelManager.LevelCount - 1);
        if (levelManager.LoadLevel(index)) SetState(GameState.Playing);
    }

    public void RestartLevel()
    {
        if (State == GameState.Menu) return;
        if (State == GameState.Paused && _resumeState == GameState.Menu) return;

        if (levelManager.ReloadCurrent()) SetState(GameState.Playing);
    }

    public void NextLevel()
    {
        if (State != GameState.Win) return;

        // Hết danh sách thì quay lại Level 1
        bool loaded = levelManager.HasNext ? levelManager.LoadNext() : levelManager.LoadLevel(0);
        if (loaded) SetState(GameState.Playing);
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
        if (State == GameState.Paused || State == GameState.Win || State == GameState.Lose) return;

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
        if (levelSession != null) levelSession.ClearSession();
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
        int reward = config != null ? config.WinReward : 0;
        SaveManager.Coins += reward;
        SaveManager.CurrentLevelIndex = levelManager.HasNext ? levelManager.CurrentIndex + 1 : 0;
        SaveManager.Save();

        AudioManager.PlaySfx(Sfx.Win);

        if (winPanel != null) winPanel.Setup(levelManager.CurrentIndex + 1, reward, levelManager.HasNext);
        SetState(GameState.Win);
    }

    private void OnLost()
    {
        AudioManager.PlaySfx(Sfx.Lose);
        SetState(GameState.Lose);
    }

    // ---------- Trạng thái ----------

    private void SetState(GameState next)
    {
        State = next;

        if (levelSession != null) levelSession.InputEnabled = next == GameState.Playing;

        bool inMenu = next == GameState.Menu || (next == GameState.Paused && _resumeState == GameState.Menu);
        SetPanel(mainMenuPanel, inMenu);
        SetPanel(hudPanel, !inMenu);
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
