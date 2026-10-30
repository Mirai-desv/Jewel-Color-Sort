using System.Collections.Generic;
using UnityEngine;

public class LevelSession : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] private GridView gridView;
    [SerializeField] private TrayView trayView;
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private BeadFlyAnimator flyAnimator;
    [SerializeField] private CameraControll cameraController; // tuỳ chọn
    [SerializeField] private GameConfig config;                  // tuỳ chọn (thiếu thì dùng giá trị mặc định)
    [SerializeField] private Camera worldCamera;                 // để trống thì dùng Camera.main

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    public event System.Action Won;
    public event System.Action Lost;
    public event System.Action LevelStarted;
    public event System.Action<float> ProgressChanged; // 0..1

    // Dành cho Tutorial
    public event System.Action SelectionMade;  // vừa nhấc 1 nhóm hạt (lưới hoặc khay)
    public event System.Action MoveCommitted;  // vừa thực hiện xong 1 nước đi (hạt có thể còn đang bay)

    // Cổng chặn click: trả về false thì click bị bỏ qua hoàn toàn. Để null = không chặn
    public System.Func<int, int, bool> CellClickFilter;
    public System.Func<int, bool> SlotClickFilter;

    public bool HasGridSelection => _kind == SelectionKind.Grid;
    public bool HasTraySelection => _kind == SelectionKind.Tray;

    public bool HasStartedLevel { get; private set; }

    // GameManager bật/tắt: chỉ nhận thao tác khi đang ở trạng thái Playing
    public bool InputEnabled { get; set; } = true;

    public bool IsBusy => flyAnimator != null && flyAnimator.IsBusy;
    public float ElapsedTime { get; private set; }
    public int MoveCount { get; private set; }
    public int UndoCount { get; private set; }

    public bool CanUndo =>
        _undo.Count > 0 && !IsBusy && _pendingEnd == EndState.None && _ended != EndState.Win;

    private bool CanAcceptInput =>
        InputEnabled && !IsBusy && _pendingEnd == EndState.None && _ended == EndState.None;

    private enum SelectionKind { None, Grid, Tray }

    private readonly GridModel _grid = new GridModel();
    private readonly TrayModel _tray = new TrayModel();
    private readonly RegionFinder _finder = new RegionFinder();
    private readonly WinLoseChecker _checker = new WinLoseChecker();
    private readonly UndoStack _undo = new UndoStack();

    // Danh sách dùng lại, tránh cấp phát mỗi lần click
    private readonly List<int> _selection = new List<int>(256);
    private readonly List<int> _source = new List<int>(256);
    private readonly List<int> _dest = new List<int>(256);
    private readonly List<int> _traySelection = new List<int>(64);
    private readonly List<int> _trayEmptySlots = new List<int>(64);
    private readonly List<int> _traySource = new List<int>(64);

    private LevelData _level;
    private SelectionKind _kind = SelectionKind.None;
    private Type _selectedType = Type.None;
    private int _selectedOrigin;
    private EndState _pendingEnd = EndState.None; // đã đạt thắng/thua, chờ animation xong mới báo
    private EndState _ended = EndState.None;      // đã báo thắng/thua

    private Camera Cam => worldCamera != null ? worldCamera : (worldCamera = Camera.main);

    // ---------- Vòng đời ----------

    private void OnEnable()
    {
        if (inputHandler != null) inputHandler.CellClicked += OnCellClicked;
        if (trayView != null) trayView.SlotClicked += OnSlotClicked;
        if (flyAnimator != null) flyAnimator.ArrivedAtTray += OnTrayArrived;
    }

    private void OnDisable()
    {
        if (inputHandler != null) inputHandler.CellClicked -= OnCellClicked;
        if (trayView != null) trayView.SlotClicked -= OnSlotClicked;
        if (flyAnimator != null) flyAnimator.ArrivedAtTray -= OnTrayArrived;
    }

    private void Update()
    {
        if(InputEnabled && _ended == EndState.None && _pendingEnd == EndState.None) ElapsedTime += Time.deltaTime;
        // Báo thắng/thua khi hạt đã bay xong hết
        if (_pendingEnd == EndState.None || IsBusy) return;

        EndState end = _pendingEnd;
        _pendingEnd = EndState.None;
        _ended = end;

        if (end == EndState.Win)
        {
            Log("THẮNG!");
            Won?.Invoke();
        }
        else
        {
            Log("THUA: không còn nước đi.");
            Lost?.Invoke();
        }
    }

    // Nạp 1 level và bắt đầu chơi (do LevelManager / GameManager gọi)
    public void StartLevel(LevelData data)
    {
        if (data == null)
        {
            Debug.LogError("[LevelSession] LevelData bị trống.", this);
            return;
        }

        if (!CheckReferences()) return;

        _level = data;
        _level.Validate(); // báo lỗi nhập tay ngay trong Console

        if (flyAnimator != null) flyAnimator.CancelAll();

        _selection.Clear();
        _traySelection.Clear();
        _kind = SelectionKind.None;
        _selectedType = Type.None;
        _pendingEnd = EndState.None;
        _ended = EndState.None;
        _undo.Clear();

        ElapsedTime = 0f;
        MoveCount = 0;
        UndoCount = 0;

        _grid.Init(_level);
        _tray.Init(_level.TraySlotCount);

        gridView.Build(_level);
        trayView.Build(_tray.SlotCount);
        if (cameraController != null) cameraController.FitToGrid();

        for (int i = 0; i < _grid.Size; i++)
        {
            if (_grid.IsInside(i) && _grid.IsLocked(i)) gridView.SetLocked(i, true);
        }

        Debug.Log("[Debug] Sắp phát sự kiện LevelStarted.");
        Debug.Log($"[Debug] Phát LevelStarted từ Session ID={GetInstanceID()}", this);
        HasStartedLevel = true;
        LevelStarted?.Invoke();
        Debug.Log("[Debug] Đã phát sự kiện LevelStarted.");
        RaiseProgress();
        EvaluateEnd();
    }

    // Kiểm tra các tham chiếu bắt buộc, báo lỗi rõ ràng thay vì để NullReferenceException khó hiểu
    private bool CheckReferences()
    {
        bool ok = true;
        if (gridView == null) { Debug.LogError("[LevelSession] Chưa gán Grid View.", this); ok = false; }
        if (trayView == null) { Debug.LogError("[LevelSession] Chưa gán Tray View.", this); ok = false; }
        if (inputHandler == null) { Debug.LogError("[LevelSession] Chưa gán Input Handler.", this); ok = false; }
        if (flyAnimator == null) { Debug.LogError("[LevelSession] Chưa gán Fly Animator: cần 1 object trong scene có component BeadFlyAnimator.", this); ok = false; }

        if (cameraController != null && !cameraController.gameObject.scene.IsValid())
        {
            Debug.LogError("[LevelSession] Camera Controller đang trỏ vào PREFAB trong Project, không phải Main Camera trong scene. Hãy kéo Main Camera từ Hierarchy vào ô này.", this);
            ok = false;
        }
        
        if (FindFirstObjectByType<GestureDetector>() == null)
        {
            Debug.LogError("[LevelSession] Scene chưa có GestureDetector nên sẽ không nhận được click. Hãy thêm component GestureDetector vào object Session.", this);
            ok = false;
        }
        return ok;
    }

    [ContextMenu("Restart Level")]
    public void Restart()
    {
        if (_level != null) StartLevel(_level);
    }

    public void ClearSession()
    {
        HasStartedLevel = false;
        if (flyAnimator != null) flyAnimator.CancelAll();
        _selection.Clear();
        _traySelection.Clear();
        _kind = SelectionKind.None;
        _selectedType = Type.None;
        _pendingEnd = EndState.None;
        _ended = EndState.None;
        _undo.Clear();
        if (gridView != null) gridView.Clear();
        if (trayView != null) trayView.Clear();
    }

    // Hoàn tác nước đi gần nhất (hạt bay ngược về chỗ cũ)
    public void Undo()
    {
        if (!CanUndo) return;
        MoveCount = Mathf.Max(0, MoveCount - 1);
        UndoCount++;

        ClearSelection();
        MoveCommand command = _undo.Pop();
        IReadOnlyList<MoveStep> steps = command.Steps;
        float step = StaggerStep(steps.Count);

        int n = 0;
        for (int i = steps.Count - 1; i >= 0; i--, n++)
        {
            MoveStep s = steps[i];
            float delay = n * step;

            // Đảo chiều từng bước
            switch (s.Kind)
            {
                case MoveKind.GridToGrid: BeadGridToGrid(s.To, s.From, s.BeadType, delay); break;
                case MoveKind.GridToTray: BeadTrayToGrid(s.To, s.From, s.BeadType, delay); break;
                case MoveKind.TrayToGrid: BeadGridToTray(s.To, s.From, s.BeadType, delay); break;
            }
        }

        _ended = EndState.None; // undo được cả khi đang ở trạng thái thua
        RaiseProgress();
        EvaluateEnd();
    }

    // Truy vấn trạng thái (Tutorial dùng để quyết định cú chạm nào hợp lệ)

    public bool IsUnlockedBeadAt(int x, int y)
    {
        if (!gridView.InBounds(x, y)) return false;
        int index = gridView.ToIndex(x, y);
        return _grid.IsInside(index) && _grid.HasBead(index) && !_grid.IsLocked(index);
    }

    public bool IsEmptyCellAt(int x, int y)
    {
        if (!gridView.InBounds(x, y)) return false;
        int index = gridView.ToIndex(x, y);
        return _grid.IsInside(index) && _grid.IsEmpty(index);
    }

    public bool IsTraySlotFilled(int slot) => _tray.IsValid(slot) && !_tray.IsEmpty(slot);

    // Xử lý click trên lưới

    private void OnCellClicked(int x, int y)
    {
        if (!CanAcceptInput) return;
        if (CellClickFilter != null && !CellClickFilter(x, y)) return;

        int index = gridView.ToIndex(x, y);
        if (!_grid.IsInside(index)) return;

        if (_grid.HasBead(index)) HandleBeadClick(index);
        else HandleEmptyCellClick(index);
    }

    private void HandleBeadClick(int index)
    {
        if (_grid.IsLocked(index))
        {
            // Bấm vào ô đã hoàn thành: nếu đang nhấc hạt thì hạ hạt xuống
            if (_kind != SelectionKind.None)
            {
                ClearSelection();
            }
            return;
        }

        if (_kind == SelectionKind.Grid && _selection.Contains(index))
        {
            ClearSelection();
            return;
        }

        ClearSelection();
        SelectGridRegion(index);
    }

    private void HandleEmptyCellClick(int index)
    {
        if (_kind == SelectionKind.None) return;

        if (_grid.GetBackground(index) != _selectedType)
        {
            // Bấm sai màu: hạ hạt xuống và báo invalid
            ClearSelection();
            Invalid("Màu nền của khu vực này không khớp màu hạt.");
            return;
        }

        if (_kind == SelectionKind.Grid) MoveGridToGrid(index);
        else MoveTrayToGrid(index);
    }

    // Xử lý click trên khay

    private void OnSlotClicked(int slot)
    {
        if (!CanAcceptInput || !_tray.IsValid(slot)) return;
        if (SlotClickFilter != null && !SlotClickFilter(slot)) return;

        if (_kind == SelectionKind.Grid)
        {
            if (!_tray.IsEmpty(slot))
            {
                // Vị trí đang ấn vào đã có hạt: hạ hạt xuống và báo invalid
                ClearSelection();
                Invalid("Ô khay đã có hạt, không nhét thêm được.");
                return;
            }

            if (_tray.CountEmpty() == 0)
            {
                // Khay đã đầy: hạ hạt xuống và báo invalid
                ClearSelection();
                Invalid("Khay đã đầy, không còn ô trống.");
                return;
            }

            MoveGridToTray();
            return;
        }

        if (_kind == SelectionKind.Tray)
        {
            if (_traySelection.Contains(slot))
            {
                ClearSelection();
                return;
            }

            if (_tray.IsEmpty(slot))
            {
                ClearSelection();
                Invalid("Ô khay trống, không có gì để chọn.");
                return;
            }
        }

        ClearSelection();
        SelectTrayGroup(slot);
    }

    // Chọn, bỏ chọn

    private void SelectGridRegion(int index)
    {
        _selectedType = _grid.GetBead(index);
        _selectedOrigin = index;
        _kind = SelectionKind.Grid;

        _finder.FindBeadRegion(_grid, index, 0, _selection);
        gridView.SetSelected(_selection, true);
        AudioManager.PlaySfx(Sfx.Select);
        SelectionMade?.Invoke();
    }

    // Chọn tất cả ô khay đang cùng màu với ô vừa click (không cần kề nhau)
    private void SelectTrayGroup(int slot)
    {
        _selectedType = _tray.GetSlotType(slot);
        _kind = SelectionKind.Tray;

        _traySelection.Clear();
        for (int i = 0; i < _tray.SlotCount; i++)
        {
            if (_tray.GetSlotType(i) == _selectedType) _traySelection.Add(i);
        }

        for (int i = 0; i < _traySelection.Count; i++) trayView.SetSelected(_traySelection[i], true);
        AudioManager.PlaySfx(Sfx.Select);
        SelectionMade?.Invoke();
    }

    private void ClearSelection()
    {
        if (_kind == SelectionKind.Grid)
        {
            gridView.SetSelected(_selection, false);
        }
        else if (_kind == SelectionKind.Tray)
        {
            for (int i = 0; i < _traySelection.Count; i++) trayView.SetSelected(_traySelection[i], false);
            _traySelection.Clear();
        }

        _selection.Clear();
        _kind = SelectionKind.None;
        _selectedType = Type.None;
    }

    // Lưới -> lưới: hạt đang chọn bay vào vùng ô rỗng cùng màu nền
    private void MoveGridToGrid(int destOrigin)
    {
        _finder.FindEmptyRegion(_grid, destOrigin, _selection.Count, _dest);
        int k = _dest.Count;
        if (k == 0) return;

        BuildSource(k);
        Type type = _selectedType;

        for (int i = 0; i < k; i++)
        {
            _selection.Remove(_source[i]);
        }

        if (_selection.Count == 0)
        {
            _kind = SelectionKind.None;
            _selectedType = Type.None;
        }
        else
        {
            _selectedOrigin = _selection[0];
        }

        var command = new MoveCommand();
        float step = StaggerStep(k);
        for (int i = 0; i < k; i++)
        {
            BeadGridToGrid(_source[i], _dest[i], type, i * step);
            command.Add(MoveKind.GridToGrid, _source[i], _dest[i], type);
        }

        CommitMove(command);
    }

    // Lưới -> khay: lần lượt lấp hạt đang chọn vào các ô khay đang trống
    private void MoveGridToTray()
    {
        _trayEmptySlots.Clear();
        for (int i = 0; i < _tray.SlotCount; i++)
        {
            if (_tray.IsEmpty(i)) _trayEmptySlots.Add(i);
        }

        int k = Mathf.Min(_selection.Count, _trayEmptySlots.Count);
        if (k <= 0)
        {
            ClearSelection();
            Invalid("Khay đã đầy, không còn ô trống.");
            return;
        }

        BuildSource(k);
        Type type = _selectedType;

        for (int i = 0; i < k; i++)
        {
            _selection.Remove(_source[i]);
        }

        if (_selection.Count == 0)
        {
            _kind = SelectionKind.None;
            _selectedType = Type.None;
        }
        else
        {
            _selectedOrigin = _selection[0];
        }

        var command = new MoveCommand();
        float step = StaggerStep(k);
        for (int i = 0; i < k; i++)
        {
            int slot = _trayEmptySlots[i];
            BeadGridToTray(_source[i], slot, type, i * step);
            command.Add(MoveKind.GridToTray, _source[i], slot, type);
        }

        CommitMove(command);
    }

    // Khay -> lưới: các hạt trong khay đang chọn lấp vào vùng ô rỗng cùng màu nền
    private void MoveTrayToGrid(int destOrigin)
    {
        _finder.FindEmptyRegion(_grid, destOrigin, _traySelection.Count, _dest);
        int k = _dest.Count;
        if (k == 0) return;

        _traySource.Clear();
        for (int i = 0; i < k; i++)
        {
            int slot = _traySelection[i];
            _traySource.Add(slot);
            trayView.SetSelected(slot, false);
        }

        Type type = _selectedType;
        _traySelection.RemoveRange(0, k);

        // Nếu đã thả hết thì hết chọn, nếu còn thì giữ nguyên trạng thái nhấc hạt
        if (_traySelection.Count == 0)
        {
            _kind = SelectionKind.None;
            _selectedType = Type.None;
        }

        var command = new MoveCommand();
        float step = StaggerStep(k);
        for (int i = 0; i < k; i++)
        {
            BeadTrayToGrid(_traySource[i], _dest[i], type, i * step);
            command.Add(MoveKind.TrayToGrid, _traySource[i], _dest[i], type);
        }

        CommitMove(command);
    }

    /// Lập danh sách k ô hạt nguồn (trên lưới). Nếu k nhỏ hơn vùng đang chọn thì BFS lại từ ô đã click với giới hạn k (lấy phần gần điểm click nhất, vẫn liền khối).
    private void BuildSource(int k)
    {
        if (k >= _selection.Count)
        {
            _source.Clear();
            _source.AddRange(_selection);
        }
        else
        {
            _finder.FindBeadRegion(_grid, _selectedOrigin, k, _source);
        }
    }

    private void CommitMove(MoveCommand command)
    {
        _undo.Push(command);
        MoveCount++;
        AudioManager.PlaySfx(Sfx.Move);
        RaiseProgress();
        EvaluateEnd();
        MoveCommitted?.Invoke();
    }

    // Di chuyển 1 hạt: cập nhật MODEL trước, rồi VIEW + animation

    private void BeadGridToGrid(int from, int to, Type type, float delay)
    {
        _grid.RemoveBead(from);
        _grid.PlaceBead(to, type);

        BeadView bead = gridView.TakeBead(from);
        if (bead == null) // phòng hờ view lệch model
        {
            gridView.PlaceBead(to, type).SetLocked(_grid.IsLocked(to));
            return;
        }

        bead.SetSelected(false);
        gridView.AttachBead(to, bead);
        bead.SetLocked(_grid.IsLocked(to));
        flyAnimator.FlyOnGrid(bead, gridView.CellToWorld(to), delay, gridView.BeadSortingOrder);
    }

    private void BeadGridToTray(int from, int slot, Type type, float delay)
    {
        _grid.RemoveBead(from);
        _tray.Set(slot, type);

        BeadView bead = gridView.TakeBead(from);
        if (bead == null)
        {
            trayView.SetSlot(slot, type);
            return;
        }

        bead.SetSelected(false);
        bead.SetLocked(false);
        // Ô khay chỉ hiện icon khi hạt bay tới nơi (sự kiện ArrivedAtTray)
        flyAnimator.FlyToTray(bead, trayView.GetSlotWorldPosition(slot, Cam), delay, slot);
    }

    private void BeadTrayToGrid(int slot, int to, Type type, float delay)
    {
        _tray.Clear(slot);
        trayView.SetSlot(slot, Type.None);
        _grid.PlaceBead(to, type);

        Vector3 start = trayView.GetSlotWorldPosition(slot, Cam);
        BeadView bead = gridView.PlaceBead(to, type);
        bead.SetLocked(_grid.IsLocked(to));
        bead.transform.position = start; // xuất phát từ ô khay
        flyAnimator.FlyFromTray(bead, gridView.CellToWorld(to), delay, gridView.BeadSortingOrder);
    }

    private void OnTrayArrived(int slot) => trayView.SetSlot(slot, _tray.GetSlotType(slot));

    // ---------- Phụ trợ ----------

    private float StaggerStep(int count)
    {
        if (count <= 1) return 0f;

        float step = config != null ? config.StaggerStep : 0.02f;
        float maxTotal = config != null ? config.MaxTotalStagger : 0.4f;
        return Mathf.Min(step, maxTotal / count);
    }

    private void EvaluateEnd()
    {
        EndState state = _checker.Evaluate(_grid, _tray);
        if (state != EndState.None) _pendingEnd = state;
    }

    private void RaiseProgress()
    {
        float progress = _grid.TotalCells > 0 ? (float)_grid.CorrectCount / _grid.TotalCells : 0f;
        ProgressChanged?.Invoke(progress);
    }

    private void Invalid(string message)
    {
        AudioManager.PlaySfx(Sfx.Invalid);
        Log(message);
    }

    private void Log(string message)
    {
        if (debugLog) Debug.Log($"[LevelSession] {message}", this);
    }
}