using UnityEngine;
using System.Collections.Generic;

public class GridView : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] private BackgroundView backgroundView;
    [SerializeField] private BeadPool beadPool;
    [SerializeField] private ColorPalette palette;
    [SerializeField] private Transform beadsRoot; //gom hạt lại cho gọn
    public Transform BeadsRoot => beadsRoot;

    [Header("Hiển thị")]
    [SerializeField] private int beadSortingOrder = 10;
    
    private BeadView[] _beads;
    private int _width;
    private int _height;
    private float _cellSize = 1f;

    public int Width => _width;
    public int Height => _height;
    public float CellSize => _cellSize;

    // Dùng cho CameraController (đại khái là phạm vi của lưới)
    public Bounds WorldBounds
    {
        get
        {
            var size = new Vector3(_width * _cellSize, _height * _cellSize, 0f);
            return new Bounds(transform.position + size * 0.5f, size);
        }
    }

    // Dựng và dọn Level => khi từ Level này chuyển sang Level khác
    public void Build(LevelData level)
    {
        Clear();

        _width = level.Width;
        _height = level.Height;
        _cellSize = Constants.CellSize;
        _beads = new BeadView[_width * _height];
        backgroundView.Build(level.Background, _width, _height, palette, _cellSize);

        IReadOnlyList<CellData> beads = level.Beads;
        for(int i = 0; i < beads.Count; i++)
        {
            CellData c = beads[i];
            if(c.type == Type.None || !InBounds(c.x, c.y)) continue;
            PlaceBead(ToIndex(c.x, c.y), c.type);
        }
    }

    public void Clear()
    {
        if(_beads != null)
        {
            for(int i = 0; i < _beads.Length; i++)
            {
                if(_beads[i] == null) continue;
                beadPool.Release(_beads[i]);
                _beads[i] = null;
            }
        }
        backgroundView.Clear();
    }

    // Remember Recheck
    public BeadView PlaceBead(int index, Type type)
    {
        if(_beads[index] != null) RemoveBead(index);
        BeadView bead = beadPool.Get(beadsRoot);
        bead.Setup(type, palette.GetColor(type), palette.BeadSprite, _cellSize, beadSortingOrder);
        bead.transform.position = CellToWorld(index);

        _beads[index] = bead;
        return bead;
    }

    public BeadView TakeBead(int index)
    {
        BeadView bead = _beads[index];
        _beads[index] = null;
        return bead;
    }

    public void RemoveBead(int index)
    {
        BeadView bead = TakeBead(index);
        if(bead != null) beadPool.Release(bead);
    }

    public BeadView GetBead(int index) => _beads[index];

    public void SetSelected(int index, bool selected)
    {
        if(_beads[index] != null) _beads[index].SetSelected(selected);
    }

    public void SetSelected(IReadOnlyList<int> indices, bool selected)
    {
        for(int i = 0; i < indices.Count; i++) SetSelected(indices[i], selected);
    }

    public void SetLocked(int index, bool locked)
    {
        if(_beads[index] != null) _beads[index].SetLocked(locked);
    }

    // Quy đổi tọa độ
    public bool InBounds(int x, int y) => x >= 0 && x < _width && y >= 0 && y < _height;
    
    public int ToIndex(int x, int y) => y * _width + x;

    public Vector3 CellToWorld(int x, int y) => transform.position + new Vector3((x + 0.5f) * _cellSize, (y + 0.5f) * _cellSize, 0f);

    public Vector3 CellToWorld(int index) => CellToWorld(index % _width, index / _width);

    // Đổi tọa độ thế giới (từ Input) sang ô (x, y), trả về false nếu chạm ngoài lưới
    public bool TryWorldToCell(Vector2 world, out int x, out int y)
    {
        Vector2 local = world - (Vector2)transform.position;
        x = Mathf.FloorToInt(local.x / _cellSize);
        y = Mathf.FloorToInt(local.y / _cellSize);
        return InBounds(x, y);
    }
}
