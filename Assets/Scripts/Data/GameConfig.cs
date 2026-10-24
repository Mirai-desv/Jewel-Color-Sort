using UnityEngine;

// Cấu hình chung: tốc độ animation, phần thưởng, công thức tính điểm. Chỉnh ở 1 chỗ, không cần sửa code
[CreateAssetMenu(fileName = "GameConfig", menuName = "BeadSort/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Animation hạt bay")]
    [SerializeField, Min(0.01f)] private float flyDuration = 0.3f;
    [SerializeField, Min(0f)] private float staggerStep = 0.02f;
    [SerializeField, Min(0f)] private float maxTotalStagger = 0.4f;
    [SerializeField, Range(0.2f, 1f)] private float trayFlyScale = 0.7f;
    [SerializeField] private int flySortingOrder = 100;

    [Header("Thưởng (chỉ nhận lần thắng đầu tiên của mỗi level)")]
    [SerializeField, Min(0)] private int winReward = 10;

    [Header("Tính điểm")]
    [Tooltip("Điểm cơ bản khi qua màn.")]
    [SerializeField, Min(0)] private int baseScore = 1000;
    [Tooltip("Thưởng thời gian tối đa (qua càng nhanh càng nhiều).")]
    [SerializeField, Min(0)] private int maxTimeBonus = 500;
    [Tooltip("Sau bấy nhiêu giây thì thưởng thời gian về 0 (giảm tuyến tính).")]
    [SerializeField, Min(1f)] private float timeBonusWindow = 120f;
    [Tooltip("Thưởng số nước đi tối đa (khi không vượt Par Moves).")]
    [SerializeField, Min(0)] private int maxMoveBonus = 500;
    [Tooltip("Mỗi nước đi vượt Par Moves bị trừ bấy nhiêu điểm thưởng.")]
    [SerializeField, Min(0)] private int movePenalty = 25;
    [Tooltip("Mỗi lần Undo bị trừ bấy nhiêu điểm.")]
    [SerializeField, Min(0)] private int undoPenalty = 50;
    [Tooltip("Par Moves mặc định khi LevelData để Par Moves = 0.")]
    [SerializeField, Min(1)] private int defaultParMoves = 30;

    public float FlyDuration => flyDuration;
    public float StaggerStep => staggerStep;
    public float MaxTotalStagger => maxTotalStagger;
    public float TrayFlyScale => trayFlyScale;
    public int FlySortingOrder => flySortingOrder;
    public int WinReward => winReward;

    public int BaseScore => baseScore;
    public int MaxTimeBonus => maxTimeBonus;
    public float TimeBonusWindow => timeBonusWindow;
    public int MaxMoveBonus => maxMoveBonus;
    public int MovePenalty => movePenalty;
    public int UndoPenalty => undoPenalty;
    public int DefaultParMoves => defaultParMoves;
}