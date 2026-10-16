using UnityEngine;

/// <summary>Cấu hình chung: tốc độ animation, phần thưởng... Chỉnh ở 1 chỗ, không cần sửa code.</summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "BeadSort/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Animation hạt bay")]
    [SerializeField, Min(0.01f)] private float flyDuration = 0.3f;        // thời gian bay của 1 hạt (giây)
    [SerializeField, Min(0f)] private float staggerStep = 0.02f;          // hạt sau xuất phát trễ hơn hạt trước bấy nhiêu giây
    [SerializeField, Min(0f)] private float maxTotalStagger = 0.4f;       // tổng độ trễ tối đa của cả nhóm (nhóm lớn thì tự rút ngắn)
    [SerializeField, Range(0.2f, 1f)] private float trayFlyScale = 0.7f;  // hạt thu nhỏ còn bấy nhiêu khi bay vào/ra khay
    [SerializeField] private int flySortingOrder = 100;                   // hạt đang bay nằm trên mọi thứ

    [Header("Thưởng")]
    [SerializeField, Min(0)] private int winReward = 10;

    public float FlyDuration => flyDuration;
    public float StaggerStep => staggerStep;
    public float MaxTotalStagger => maxTotalStagger;
    public float TrayFlyScale => trayFlyScale;
    public int FlySortingOrder => flySortingOrder;
    public int WinReward => winReward;
}
