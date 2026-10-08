using System.Collections.Generic;

public class GridModel
{
    private Type[] _background = new Type[0];
    private Type[] _beads = new Type[0];
    private bool[] _locked = new bool[0];

    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Size => Width * Height;

    // Tổng số ô thuộc hình (ô nền khác None)
    public int TotalCells { get; private set; }

    // Số ô đang có hạt đúng màu nền (cập nhật dần, không quét lại lưới)
    public int CorrectCount { get; private set; }

    public bool IsSolved => TotalCells > 0 && CorrectCount == TotalCells;

    public void Init(LevelData level)
    {
        Width = level.Width;
        Height = level.Height;

        int size = Width * Height;
        _background = new Type[size];
        _beads = new Type[size];
        _locked = new bool[size];
        TotalCells = 0;
        CorrectCount = 0;

        IReadOnlyList<CellData> bg = level.Background;
        for (int i = 0; i < bg.Count; i++)
        {
            CellData c = bg[i];
            if (c.type == Type.None || !InRange(c.x, c.y)) continue;

            int idx = c.y * Width + c.x;
            if (_background[idx] != Type.None) continue; // trùng toạ độ: giữ ô đầu tiên

            _background[idx] = c.type;
            TotalCells++;
        }

        IReadOnlyList<CellData> beads = level.Beads;
        for (int i = 0; i < beads.Count; i++)
        {
            CellData c = beads[i];
            if (c.type == Type.None || !InRange(c.x, c.y)) continue;

            int idx = c.y * Width + c.x;
            if (_background[idx] == Type.None) continue; // hạt ngoài hình: bỏ qua

            _beads[idx] = c.type;
        }

        // Hạt nào vốn đã đúng chỗ thì khoá luôn
        for (int i = 0; i < size; i++)
        {
            if (_background[i] != Type.None && _beads[i] == _background[i])
            {
                _locked[i] = true;
                CorrectCount++;
            }
        }
    }

    private bool InRange(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    // Ô có thuộc hình không
    public bool IsInside(int index) => index >= 0 && index < _background.Length && _background[index] != Type.None;

    public Type GetBackground(int index) => _background[index];
    public Type GetBead(int index) => _beads[index];
    public bool HasBead(int index) => _beads[index] != Type.None;
    public bool IsLocked(int index) => _locked[index];

    // Ô thuộc hình nhưng đang không có hạt
    public bool IsEmpty(int index) => _background[index] != Type.None && _beads[index] == Type.None;

    // Rút hạt khỏi ô. Nếu hạt đang khoá (đúng chỗ) thì mở khoá và trừ biến đếm
    public void RemoveBead(int index)
    {
        if (_beads[index] == Type.None) return;

        if (_locked[index])
        {
            _locked[index] = false;
            CorrectCount--;
        }
        _beads[index] = Type.None;
    }

    // Đặt hạt vào ô rỗng. Nếu đúng màu nền thì tự khoá và tăng biến đếm
    public void PlaceBead(int index, Type type)
    {
        _beads[index] = type;

        if (type != Type.None && _background[index] == type)
        {
            _locked[index] = true;
            CorrectCount++;
        }
    }
}