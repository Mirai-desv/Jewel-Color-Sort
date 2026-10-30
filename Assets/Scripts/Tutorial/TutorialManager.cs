using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Điều khiển tutorial theo từng bước (state machine).
/// - Biết tutorial của level hiện tại qua LevelData.Tutorial.
/// - Chỉ cho phép đúng những cú chạm mà bước hiện tại yêu cầu (LevelSession.CellClickFilter / SlotClickFilter).
/// - Qua bước dựa trên KẾT QUẢ trong LevelSession (đã nhấc được hạt, đã đi xong 1 nước), không dựa vào "có click".
/// - Mỗi khung hình tính lại vùng sáng và vị trí bàn tay từ layout thật, nên không lệch khi đổi độ phân giải.
/// Không đặt chung GameObject với TutorialOverlay.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private LevelSession levelSession;
    [SerializeField] private GridView gridView;
    [SerializeField] private TrayView trayView;
    [SerializeField] private TutorialOverlay overlay;
    [SerializeField] private CameraControll cameraController; // tuỳ chọn: khoá kéo/zoom trong lúc hướng dẫn
    [SerializeField] private Camera worldCamera;              // để trống thì dùng Camera.main

    [Header("Khoá trong lúc hướng dẫn")]
    [Tooltip("Kéo nút Undo (component Button) vào đây. Sẽ tắt interactable cho đến khi xong tutorial.")]
    [SerializeField] private Button[] lockWhileRunning;

    [Header("Test")]
    [Tooltip("Luôn chạy tutorial dù đã xem rồi")]
    [SerializeField] private bool forceTutorial = false;
    [SerializeField] private bool disableTutorial = false;
    [SerializeField] private bool debugLog = false;

    private TutorialData _data;
    private int _levelIndex;
    private int _stepIndex;
    private bool _pending;   // level này có tutorial, đang chờ vào trạng thái Playing
    private bool _running;
    private bool _paused;    // đang mở Setting giữa chừng
    private bool _waitIdle;  // nước đi đã xong, đợi hạt bay hết rồi mới qua bước

    private readonly Vector3[] _corners = new Vector3[4];

    public bool IsRunning => _running;

    private TutorialStep Step => _data.Steps[_stepIndex];
    private Camera Cam => worldCamera != null ? worldCamera : (worldCamera = Camera.main);

    // ---------- Vòng đời ----------

    private void OnEnable()
    {
        if (levelSession != null)
        {
            levelSession.LevelStarted += OnLevelStarted;
            levelSession.SelectionMade += OnSelectionMade;
            levelSession.MoveCommitted += OnMoveCommitted;
        }
        if (gameManager != null) gameManager.StateChanged += OnStateChanged;
        if (overlay != null) overlay.SkipClicked += OnSkip;
    }

    private void OnDisable()
    {
        if (levelSession != null)
        {
            levelSession.LevelStarted -= OnLevelStarted;
            levelSession.SelectionMade -= OnSelectionMade;
            levelSession.MoveCommitted -= OnMoveCommitted;
        }
        if (gameManager != null) gameManager.StateChanged -= OnStateChanged;
        if (overlay != null) overlay.SkipClicked -= OnSkip;
        Abort();
    }

    private void Update()
    {
        // Nước đi đã xong và hạt đã bay hết thì mới qua bước tiếp
        if (!_running || _paused || !_waitIdle) return;
        if (levelSession.IsBusy) return;

        _waitIdle = false;
        Advance();
    }

    private void LateUpdate()
    {
        if (!_running || _paused) return;
        if (!TryGetScreenTargets(out Rect hole, out Vector2 pointer)) return;

        overlay.Present(hole, pointer, Step.message, !_waitIdle);
    }

    // ---------- Sự kiện từ game ----------

    // Level vừa được nạp (có thể còn phải xem dialogue). Chỉ đánh dấu "chờ", chưa chạy ngay
    private void OnLevelStarted()
    {
        Abort();
        if (disableTutorial || levelManager == null) return;

        int index = levelManager.CurrentIndex;
        LevelData level = levelManager.CurrentLevel;
        TutorialData data = level != null ? level.Tutorial : null;
        if (data == null || data.Steps.Count == 0) return;
        if (!forceTutorial && SaveManager.HasDoneTutorial(index)) return;

        _data = data;
        _levelIndex = index;
        _pending = true;
        Log($"Level {index + 1} có tutorial ({data.Steps.Count} bước), chờ vào trạng thái Playing.");
    }

    private void OnStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                if (_running)
                {
                    _paused = false; // quay lại từ Setting
                    overlay.Show();
                }
                else if (_pending)
                {
                    Begin();
                }
                break;

            case GameState.Paused:
                if (_running)
                {
                    _paused = true;
                    overlay.Hide();
                }
                break;

            case GameState.Dialogue:
                break; // giữ _pending, vào Playing rồi mới chạy

            case GameState.Win:
                if (_running) Complete(); // thắng nghĩa là đã làm hết các bước
                break;

            default: // Menu, LevelSelect, Lose, PostLevel
                Abort();
                break;
        }
    }

    private void OnSelectionMade()
    {
        if (!_running || _paused || _waitIdle) return;

        TutorialStep step = Step;
        if (step.advanceOn != TutorialAdvance.SelectionMade) return;

        bool selected = step.target == TutorialTarget.Grid
            ? levelSession.HasGridSelection
            : levelSession.HasTraySelection;
        if (selected) Advance();
    }

    private void OnMoveCommitted()
    {
        if (!_running || _paused || _waitIdle) return;
        if (Step.advanceOn == TutorialAdvance.MoveDone) _waitIdle = true;
    }

    private void OnSkip()
    {
        Log("Người chơi bấm Bỏ qua.");
        Complete();
    }

    // ---------- Chạy / kết thúc ----------

    private void Begin()
    {
        _pending = false;
        _running = true;
        _paused = false;
        _waitIdle = false;
        _stepIndex = 0;

        levelSession.CellClickFilter = AllowCell;
        levelSession.SlotClickFilter = AllowSlot;
        if (cameraController != null) cameraController.InputLocked = true;
        SetLockedButtons(false);

        overlay.SetSkipVisible(true);
        overlay.Show();
        Log("Bắt đầu tutorial.");
    }

    private void Advance()
    {
        _stepIndex++;
        if (_stepIndex >= _data.Steps.Count)
        {
            Complete();
            return;
        }
        Log($"Qua bước {_stepIndex + 1}/{_data.Steps.Count}");
    }

    // Xong (hoặc bỏ qua): lưu cờ để lần sau không hiện nữa
    private void Complete()
    {
        if (!_running) return;

        SaveManager.MarkTutorialDone(_levelIndex);
        SaveManager.Save();
        Log("Hoàn thành tutorial.");
        End();
    }

    // Dừng giữa chừng nhưng KHÔNG lưu cờ (thoát ra menu, restart level...)
    private void Abort()
    {
        _pending = false;
        End();
    }

    private void End()
    {
        bool wasRunning = _running;

        _running = false;
        _paused = false;
        _waitIdle = false;

        if (levelSession != null)
        {
            levelSession.CellClickFilter = null;
            levelSession.SlotClickFilter = null;
        }
        if (cameraController != null) cameraController.InputLocked = false;
        if (wasRunning) SetLockedButtons(true);
        if (overlay != null) overlay.Hide();
    }

    private void SetLockedButtons(bool interactable)
    {
        if (lockWhileRunning == null) return;
        for (int i = 0; i < lockWhileRunning.Length; i++)
        {
            if (lockWhileRunning[i] != null) lockWhileRunning[i].interactable = interactable;
        }
    }

    // ---------- Cổng chặn click (LevelSession hỏi trước khi xử lý) ----------

    private bool AllowCell(int x, int y)
    {
        if (!_running || _waitIdle) return false;

        TutorialStep step = Step;
        if (step.target != TutorialTarget.Grid || !step.ContainsCell(x, y)) return false;

        // Bước chọn: phải là hạt chưa khoá. Bước thả: phải là ô đang trống
        return step.advanceOn == TutorialAdvance.SelectionMade
            ? levelSession.IsUnlockedBeadAt(x, y)
            : levelSession.IsEmptyCellAt(x, y);
    }

    private bool AllowSlot(int slot)
    {
        if (!_running || _waitIdle) return false;

        TutorialStep step = Step;
        if (step.target != TutorialTarget.Tray) return false;

        // Bước chọn: slot phải có hạt. Bước cất vào khay: slot phải đang trống
        bool filled = levelSession.IsTraySlotFilled(slot);
        return step.advanceOn == TutorialAdvance.SelectionMade ? filled : !filled;
    }

    // ---------- Đổi toạ độ ô / khay sang toạ độ màn hình ----------

    private bool TryGetScreenTargets(out Rect hole, out Vector2 pointer)
    {
        hole = new Rect();
        pointer = Vector2.zero;

        TutorialStep step = Step;

        if (step.target == TutorialTarget.Grid)
        {
            Camera cam = Cam;
            if (cam == null || gridView == null) return false;

            float half = gridView.CellSize * 0.5f;
            Vector3 bottomLeft = gridView.CellToWorld(step.regionMin.x, step.regionMin.y) - new Vector3(half, half, 0f);
            Vector3 topRight = gridView.CellToWorld(step.regionMax.x, step.regionMax.y) + new Vector3(half, half, 0f);

            Vector2 a = cam.WorldToScreenPoint(bottomLeft);
            Vector2 b = cam.WorldToScreenPoint(topRight);
            hole = Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
            pointer = cam.WorldToScreenPoint(gridView.CellToWorld(step.handCell.x, step.handCell.y));
            return true;
        }

        if (trayView == null) return false;
        RectTransform slot = trayView.GetSlotRect(step.handTraySlot);
        if (slot == null) return false;

        Rect slotRect = ScreenRectOf(slot);
        RectTransform background = trayView.BackgroundRect;
        hole = background != null && background.gameObject.activeInHierarchy ? ScreenRectOf(background) : slotRect;
        pointer = slotRect.center;
        return true;
    }

    // Hình chữ nhật (pixel màn hình) của 1 RectTransform UI, tính theo camera của canvas chứa nó
    private Rect ScreenRectOf(RectTransform rt)
    {
        Canvas canvas = rt.GetComponentInParent<Canvas>();
        Camera uiCamera = null;
        if (canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.rootCanvas.worldCamera;

        rt.GetWorldCorners(_corners);
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        for (int i = 0; i < 4; i++)
        {
            Vector2 p = RectTransformUtility.WorldToScreenPoint(uiCamera, _corners[i]);
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private void Log(string message)
    {
        if (debugLog) Debug.Log($"[Tutorial] {message}", this);
    }
}