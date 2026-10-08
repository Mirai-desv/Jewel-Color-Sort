using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cầu nối duy nhất: Input -> Logic (GridModel, TrayModel, RegionFinder) -> View.
/// Nguyên tắc: cập nhật dữ liệu (model) TRƯỚC, rồi mới cập nhật hiển thị (view).
///
/// Luật chơi:
///  - Click hạt chưa khoá  -> chọn vùng hạt cùng màu kề cạnh (BFS). Click lại vùng đó để bỏ chọn.
///  - Đang chọn hạt trên lưới + click 1 ô khay ĐANG TRỐNG -> lần lượt lấp vào các ô khay trống
///    (mỗi ô chỉ chứa 1 hạt) cho đến khi đủ số hạt hoặc hết ô trống.
///  - Click 1 ô khay ĐANG CÓ HẠT -> tự chọn tất cả ô khay khác đang cùng màu.
///  - Đang chọn nhóm trong khay + click ô lưới đang rỗng có màu nền trùng -> đặt các hạt đó về lưới.
///  - Số lượng lệch nhau thì lấy k = min(nguồn, đích), lan BFS từ điểm click cho đủ k (phía lưới).
///  - Hạt đặt về đúng màu nền sẽ bị khoá. Thắng khi mọi ô đều đúng.
/// </summary>
public class LevelSession : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] private LevelData level;
    [SerializeField] private GridView gridView;
    [SerializeField] private TrayView trayView;
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private CameraControll cameraControll;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    /// <summary>Phát ra khi người chơi hoàn thành level.</summary>
    public event System.Action Won;

    public bool IsWon => _isWon;

    private enum SelectionKind { None, Grid, Tray }

    private readonly GridModel _grid = new GridModel();
    private readonly TrayModel _tray = new TrayModel();
    private readonly RegionFinder _finder = new RegionFinder();

    // Danh sách dùng lại, tránh cấp phát mỗi lần click
    private readonly List<int> _selection = new List<int>(256);     // vùng hạt trên lưới đang chọn
    private readonly List<int> _source = new List<int>(256);        // ô hạt (lưới) sẽ di chuyển
    private readonly List<int> _dest = new List<int>(256);          // ô đích (lưới) sẽ nhận hạt
    private readonly List<int> _traySelection = new List<int>(64);  // ô khay đang chọn
    private readonly List<int> _trayEmptySlots = new List<int>(64); // ô khay đang trống (dùng khi lưới -> khay)
    private readonly List<int> _traySource = new List<int>(64);     // ô khay sẽ di chuyển (dùng khi khay -> lưới)

    private SelectionKind _kind = SelectionKind.None;
    private Type _selectedType = Type.None;
    private int _selectedOrigin;  // ô lưới đã click để chọn (dùng khi cần BFS lại để cắt bớt)
    private bool _isWon;

    private void OnEnable()
    {
        if (inputHandler != null) inputHandler.CellClicked += OnCellClicked;
        if (trayView != null) trayView.SlotClicked += OnSlotClicked;
    }

    private void OnDisable()
    {
        if (inputHandler != null) inputHandler.CellClicked -= OnCellClicked;
        if (trayView != null) trayView.SlotClicked -= OnSlotClicked;
    }

    //private void Start() => StartLevel(level);

    [ContextMenu("Restart Level")]
    private void Restart() => StartLevel(level);

    public void StartLevel(LevelData data)
    {
        if (data == null)
        {
            Debug.LogError("[LevelSession] Chưa gán LevelData.", this);
            return;
        }

        level = data;
        level.Validate(); // báo lỗi nhập tay ngay trong Console

        _selection.Clear();
        _traySelection.Clear();
        _kind = SelectionKind.None;
        _selectedType = Type.None;
        _isWon = false;

        _grid.Init(level);
        _tray.Init(level.TraySlotCount);

        gridView.Build(level);
        cameraControll.FitToGrid();
        trayView.Build(_tray.SlotCount);

        for (int i = 0; i < _grid.Size; i++)
        {
            if (_grid.IsInside(i) && _grid.IsLocked(i)) gridView.SetLocked(i, true);
        }

        CheckWin();
    }

    private void OnCellClicked(int x, int y)
    {
        if (_isWon) return;

        int index = gridView.ToIndex(x, y);
        if (!_grid.IsInside(index)) return; // click ngoài hình

        if (_grid.HasBead(index)) HandleBeadClick(index);
        else HandleEmptyCellClick(index);
    }

    private void HandleBeadClick(int index)
    {
        if (_grid.IsLocked(index)) return; // hạt đã đúng chỗ: không cho chọn

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
            Log("Màu nền của khu vực này không khớp màu hạt.");
            return;
        }

        if (_kind == SelectionKind.Grid) MoveGridToGrid(index);
        else MoveTrayToGrid(index);
    }


    private void OnSlotClicked(int slot)
    {
        if (_isWon || !_tray.IsValid(slot)) return;

        if (_kind == SelectionKind.Grid)
        {
            if (_tray.IsEmpty(slot)) MoveGridToTray(slot);
            else Log("Ô khay đã có hạt, không nhét thêm được.");
            return;
        }

        if (_kind == SelectionKind.Tray && _traySelection.Contains(slot))
        {
            ClearSelection(); // click lại 1 ô trong vùng đang chọn -> bỏ chọn
            return;
        }

        if (_tray.IsEmpty(slot))
        {
            Log("Ô khay trống, không có gì để chọn.");
            return;
        }

        ClearSelection();
        SelectTrayGroup(slot);
    }

    private void SelectGridRegion(int index)
    {
        _selectedType = _grid.GetBead(index);
        _selectedOrigin = index;
        _kind = SelectionKind.Grid;

        _finder.FindBeadRegion(_grid, index, 0, _selection);
        gridView.SetSelected(_selection, true);
    }

    // Chọn tất cả ô khay đang cùng màu với ô vừa click
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
        ClearSelection();

        for (int i = 0; i < k; i++)
        {
            int from = _source[i];
            int to = _dest[i];

            _grid.RemoveBead(from);
            gridView.RemoveBead(from);

            _grid.PlaceBead(to, type);
            gridView.PlaceBead(to, type).SetLocked(true);
        }

        CheckWin();
    }

    // Lưới -> khay: lần lượt lấp hạt đang chọn vào các ô khay đang trống
    private void MoveGridToTray(int clickedSlot)
    {
        _trayEmptySlots.Clear();
        for (int i = 0; i < _tray.SlotCount; i++)
        {
            if (_tray.IsEmpty(i)) _trayEmptySlots.Add(i);
        }

        int k = Mathf.Min(_selection.Count, _trayEmptySlots.Count);
        if (k <= 0)
        {
            Log("Khay đã đầy, không còn ô trống.");
            return;
        }

        BuildSource(k);
        Type type = _selectedType;
        ClearSelection();

        for (int i = 0; i < k; i++)
        {
            _grid.RemoveBead(_source[i]);
            gridView.RemoveBead(_source[i]);

            int slot = _trayEmptySlots[i];
            _tray.Set(slot, type);
            trayView.SetSlot(slot, type);
        }
    }

    // Khay -> lưới: các hạt trong khay đang chọn lấp vào vùng ô rỗng cùng màu nền
    private void MoveTrayToGrid(int destOrigin)
    {
        _finder.FindEmptyRegion(_grid, destOrigin, _traySelection.Count, _dest);
        int k = _dest.Count;
        if (k == 0) return;

        _traySource.Clear();
        for (int i = 0; i < k; i++) _traySource.Add(_traySelection[i]);

        Type type = _selectedType;
        ClearSelection();

        for (int i = 0; i < k; i++)
        {
            int slot = _traySource[i];
            _tray.Clear(slot);
            trayView.SetSlot(slot, Type.None);

            int to = _dest[i];
            _grid.PlaceBead(to, type);
            gridView.PlaceBead(to, type).SetLocked(true);
        }

        CheckWin();
    }

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

    // Đại đại để test trước đã, chuyển sau

    private void CheckWin()
    {
        if (_isWon || !_grid.IsSolved) return;

        _isWon = true;
        Log("THẮNG!");
        Won?.Invoke();
    }

    private void Log(string message)
    {
        if (debugLog) Debug.Log($"[LevelSession] {message}", this);
    }
}