using System.Collections.Generic;
using UnityEngine;

public enum TutorialTarget
{
    Grid, // người chơi phải chạm vào lưới
    Tray  // người chơi phải chạm vào khay
}

public enum TutorialAdvance
{
    SelectionMade, // qua bước khi đã nhấc được nhóm hạt (lưới hoặc khay, theo Target)
    MoveDone       // qua bước khi 1 nước đi đã hoàn tất và hạt đã bay xong
}

/// <summary>Một bước hướng dẫn. Toạ độ ô là toạ độ lưới (x, y) giống LevelData, y tăng dần lên trên.</summary>
[System.Serializable]
public class TutorialStep
{
    [TextArea(1, 3)] public string message;
    public TutorialTarget target = TutorialTarget.Grid;
    public TutorialAdvance advanceOn = TutorialAdvance.SelectionMade;

    [Header("Khi Target = Grid (đều tính cả 2 đầu mút)")]
    [Tooltip("Góc dưới-trái của vùng được làm sáng, và cũng là vùng duy nhất được phép chạm")]
    public Vector2Int regionMin;
    [Tooltip("Góc trên-phải của vùng được làm sáng")]
    public Vector2Int regionMax;
    [Tooltip("Ô mà bàn tay/vòng tròn chỉ vào")]
    public Vector2Int handCell;

    [Header("Khi Target = Tray")]
    [Tooltip("Slot khay mà bàn tay chỉ vào")]
    [Min(0)] public int handTraySlot = 0;

    public bool ContainsCell(int x, int y) =>
        x >= regionMin.x && x <= regionMax.x && y >= regionMin.y && y <= regionMax.y;
}

[CreateAssetMenu(fileName = "TutorialData", menuName = "Scriptable Objects/TutorialData")]
public class TutorialData : ScriptableObject
{
    [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();

    public IReadOnlyList<TutorialStep> Steps => steps;
}