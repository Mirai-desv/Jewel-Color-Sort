using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "Nhạc trưởng" phần hiển thị lưới: dựng nền, dựng hạt, cập nhật khi Logic báo đổi,
/// và cung cấp quy đổi toạ độ ô <-> thế giới cho Input / Camera / Animator.
///
/// Quy ước: gốc ở góc dưới trái, y hướng lên, index = y * Width + x.
/// Lưu ý: GridView không nên bị scale/rotate (giữ scale = 1, rotation = 0).
/// </summary>
public class GridView : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] private BackgroundView backgroundView;
    [SerializeField] private BeadPool beadPool;
    [SerializeField] private ColorPalette palette;
    [SerializeField] private Transform beadsRoot;

    [Header("Hiển thị")]
    [SerializeField] private int beadSortingOrder = 10;

    private BeadView[] _beads;
    private int _width;
    private int _height;
    private float _cellSize = 1f;

    public int Width => _width;
    public int Height => _height;
    public float CellSize => _cellSize;

    /// <summary>Thứ tự vẽ của hạt đứng yên trên lưới (animator dùng để trả lại sau khi bay xong).</summary>
    public int BeadSortingOrder => beadSortingOrder;

    /// <summary>Vùng thế giới mà lưới chiếm, dùng cho CameraController.</summary>
    public Bounds WorldBounds
    {
        get
        {
            var size = new Vector3(_width * _cellSize, _height * _cellSize, 0f);
            return new Bounds(transform.position + size * 0.5f, size);
        }
    }

    // ---------- Dựng / dọn ----------

    public void Build(LevelData level)
    {
        Clear();

        _width = level.Width;
        _height = level.Height;
        _cellSize = Constants.CellSize;
        _beads = new BeadView[_width * _height];

        backgroundView.Build(level.Background, _width, _height, palette, _cellSize);

        IReadOnlyList<CellData> beads = level.Beads;
        for (int i = 0; i < beads.Count; i++)
        {
            CellData c = beads[i];
            if (c.type == Type.None || !InBounds(c.x, c.y)) continue;
            PlaceBead(ToIndex(c.x, c.y), c.type);
        }
    }

    public void Clear()
    {
        if (_beads != null)
        {
            for (int i = 0; i < _beads.Length; i++)
            {
                if (_beads[i] == null) continue;
                beadPool.Release(_beads[i]);
                _beads[i] = null;
            }
        }
        backgroundView.Clear();
    }

    // ---------- Cập nhật khi Logic báo thay đổi ----------

    /// <summary>Đặt 1 hạt mới vào ô (nếu ô đã có hạt thì thay thế).</summary>
    public BeadView PlaceBead(int index, Type type)
    {
        if (_beads[index] != null) RemoveBead(index);

        BeadView bead = beadPool.Get(beadsRoot);
        bead.Setup(type, palette.GetColor(type), palette.BeadSprite, _cellSize, beadSortingOrder);
        bead.transform.position = CellToWorld(index);

        _beads[index] = bead;
        return bead;
    }

    /// <summary>Gỡ hạt khỏi ô nhưng KHÔNG trả về pool, để animator cho nó bay đi.</summary>
    public BeadView TakeBead(int index)
    {
        BeadView bead = _beads[index];
        _beads[index] = null;
        return bead;
    }

    /// <summary>Gắn 1 hạt có sẵn (đang bay) vào ô đích. Vị trí hạt do animator lo.</summary>
    public void AttachBead(int index, BeadView bead)
    {
        if (_beads[index] != null && _beads[index] != bead) RemoveBead(index);
        _beads[index] = bead;
    }

    /// <summary>Gỡ hạt khỏi ô và trả về pool ngay.</summary>
    public void RemoveBead(int index)
    {
        BeadView bead = TakeBead(index);
        if (bead != null) beadPool.Release(bead);
    }

    public BeadView GetBead(int index) => _beads[index];

    public void SetSelected(int index, bool selected)
    {
        if (_beads[index] != null) _beads[index].SetSelected(selected);
    }

    public void SetSelected(IReadOnlyList<int> indices, bool selected)
    {
        for (int i = 0; i < indices.Count; i++) SetSelected(indices[i], selected);
    }

    public void SetLocked(int index, bool locked)
    {
        if (_beads[index] != null) _beads[index].SetLocked(locked);
    }

    // ---------- Quy đổi toạ độ ----------

    public bool InBounds(int x, int y) => x >= 0 && x < _width && y >= 0 && y < _height;

    public int ToIndex(int x, int y) => y * _width + x;

    public Vector3 CellToWorld(int x, int y) =>
        transform.position + new Vector3((x + 0.5f) * _cellSize, (y + 0.5f) * _cellSize, 0f);

    public Vector3 CellToWorld(int index) => CellToWorld(index % _width, index / _width);

    /// <summary>Đổi toạ độ thế giới (từ Input) sang ô (x, y). Trả về false nếu chạm ngoài lưới.</summary>
    public bool TryWorldToCell(Vector2 world, out int x, out int y)
    {
        Vector2 local = world - (Vector2)transform.position;
        x = Mathf.FloorToInt(local.x / _cellSize);
        y = Mathf.FloorToInt(local.y / _cellSize);
        return InBounds(x, y);
    }
}
