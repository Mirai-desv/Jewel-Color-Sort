using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Giữ danh sách level theo thứ tự chơi (phần tử 0 = Level 1) và nạp level vào LevelSession.
/// Việc "khi nào nạp / khi nào chuyển level" do GameManager quyết định.
/// Hai công tắc bên dưới chỉ dùng để TEST nhanh khi chưa dựng GameManager và UI.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] private LevelSession levelSession;

    [Header("Danh sách level, đúng thứ tự chơi (phần tử 0 = Level 1)")]
    [SerializeField] private List<LevelData> levels = new List<LevelData>();

    [Header("Chế độ test (không dùng GameManager)")]
    [SerializeField] private bool autoLoadOnStart = false;
    [SerializeField] private bool autoAdvanceOnWin = false;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    /// <summary>Chỉ số level hiện tại (0 = Level 1). -1 nếu chưa nạp level nào.</summary>
    public int CurrentIndex { get; private set; } = -1;
    public int LevelCount => levels.Count;
    public bool HasNext => CurrentIndex + 1 < levels.Count;

    private void OnEnable()
    {
        if (levelSession != null) levelSession.Won += OnLevelWon;
    }

    private void OnDisable()
    {
        if (levelSession != null) levelSession.Won -= OnLevelWon;
    }

    private void Start()
    {
        if (autoLoadOnStart) LoadLevel(0);
    }

    private void OnLevelWon()
    {
        if (autoAdvanceOnWin && HasNext) LoadNext();
    }

    /// <summary>Nạp level theo chỉ số (0 = Level 1). Trả về false nếu không nạp được.</summary>
    public bool LoadLevel(int index)
    {
        if (levelSession == null)
        {
            Debug.LogError("[LevelManager] Chưa gán Level Session.", this);
            return false;
        }

        if (index < 0 || index >= levels.Count)
        {
            Log("Chỉ số level nằm ngoài danh sách.");
            return false;
        }

        LevelData data = levels[index];
        if (data == null)
        {
            Debug.LogError($"[LevelManager] Level ở vị trí {index} trong danh sách đang để trống.", this);
            return false;
        }

        CurrentIndex = index; // gán trước để HUD đọc được số level khi LevelStarted bắn ra
        Log($"Nạp Level {index + 1}: {data.name}");
        levelSession.StartLevel(data);
        return true;
    }

    public bool LoadNext() => LoadLevel(CurrentIndex + 1);

    [ContextMenu("Reload Current Level")]
    public bool ReloadCurrent() => LoadLevel(Mathf.Max(0, CurrentIndex));

    private void Log(string message)
    {
        if (debugLog) Debug.Log($"[LevelManager] {message}", this);
    }
}
