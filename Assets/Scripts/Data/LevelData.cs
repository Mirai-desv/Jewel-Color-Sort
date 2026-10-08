using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Lớp dữ liệu (màu, x, y)")]
    [SerializeField] private List<CellData> background = new List<CellData>();
    [SerializeField] private List<CellData> beads = new List<CellData>();

    [Header("Khay chứa")]
    [SerializeField, Min(1)] private int traySlotCount = 3;

    public IReadOnlyList<CellData> Background => background;
    public IReadOnlyList<CellData> Beads => beads;
    public int TraySlotCount => traySlotCount;

    // Chiều rộng lưới = x max của lớp nền + 1
    public int Width => MaxCoord(true) + 1;
    public int Height => MaxCoord(false) + 1;

    private int MaxCoord(bool useX)
    {
        int max = -1;
        for (int i = 0; i < background.Count; i++)
        {
            int v = useX ? background[i].x : background[i].y;
            if (v > max) max = v;
        }
        return max;
    }

    // Kiểm tra lỗi nhập tay
    [ContextMenu("Validate")]
    public bool Validate()
    {
        if (background.Count == 0)
        {
            Debug.LogError($"[{name}] Lớp nền đang trống.", this);
            return false;
        }
        
        bool ok = true;
        int w = Width;

        var bgByIndex = new Dictionary<int, Type>();
        var bgCount = new Dictionary<Type, int>();

        for (int i = 0; i < background.Count; i++)
        {
            CellData c = background[i];
            if (c.type == Type.None)
            {
                Debug.LogWarning($"[{name}] Nền #{i} ({c.x},{c.y}) có màu None, sẽ bị bỏ qua.", this);
                continue;
            }
            if (c.x < 0 || c.y < 0)
            {
                Debug.LogError($"[{name}] Nền #{i} có toạ độ âm ({c.x},{c.y}).", this);
                ok = false;
                continue;
            }

            int idx = c.y * w + c.x;
            if (bgByIndex.ContainsKey(idx))
            {
                Debug.LogError($"[{name}] Nền bị trùng toạ độ ({c.x},{c.y}).", this);
                ok = false;
                continue;
            }
            bgByIndex[idx] = c.type;
            bgCount[c.type] = bgCount.TryGetValue(c.type, out int n) ? n + 1 : 1;
        }

        var beadSeen = new HashSet<int>();
        var beadCount = new Dictionary<Type, int>();

        for (int i = 0; i < beads.Count; i++)
        {
            CellData c = beads[i];
            if (c.type == Type.None) continue;
            if (c.x < 0 || c.y < 0 || c.x >= w)
            {
                Debug.LogError($"[{name}] Hạt #{i} nằm ngoài lưới ({c.x},{c.y}).", this);
                ok = false;
                continue;
            }
            int idx = c.y * w + c.x;
            if (!bgByIndex.ContainsKey(idx))
            {
                Debug.LogError($"[{name}] Hạt ({c.x},{c.y}) nằm ở ô không có nền.", this);
                ok = false;
                continue;
            }
            if (!beadSeen.Add(idx))
            {
                Debug.LogError($"[{name}] Hạt bị trùng toạ độ ({c.x},{c.y}).", this);
                ok = false;
                continue;
            }
            beadCount[c.type] = beadCount.TryGetValue(c.type, out int n) ? n + 1 : 1;
        }

        foreach (var pair in bgCount)
        {
            beadCount.TryGetValue(pair.Key, out int have);
            if (have != pair.Value)
            {
                Debug.LogError($"[{name}] Màu {pair.Key}: nền {pair.Value} ô nhưng có {have} hạt.", this);
                ok = false;
            }
        }
        foreach (var pair in beadCount)
        {
            if (!bgCount.ContainsKey(pair.Key))
            {
                Debug.LogError($"[{name}] Có hạt màu {pair.Key} nhưng nền không có màu này.", this);
                ok = false;
            }
        }

        if (ok) Debug.Log($"[{name}] Level hợp lệ: {bgByIndex.Count} ô, {bgCount.Count} màu.", this);
        return ok;
    }
}
