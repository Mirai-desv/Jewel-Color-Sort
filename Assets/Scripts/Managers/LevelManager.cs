using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] private LevelSession levelSession;

    [Header("Danh sách level, đúng thứ tự chơi (phần tử 0 = Level 1)")]
    [SerializeField] private List<LevelData> levels = new List<LevelData>();

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    // Chỉ số level hiện tại (0 = Level 1), -1 nếu chưa nạp level
    public int CurrentIndex { get; private set; } = -1;
    public int LevelCount => levels.Count;

    private void OnEnable()
    {
        if (levelSession != null) levelSession.Won += OnLevelWon;
    }

    private void OnDisable()
    {
        if (levelSession != null) levelSession.Won -= OnLevelWon;
    }

    private void Start() => LoadLevel(0);

    private void OnLevelWon()
    {
        Log($"Hoàn thành Level {CurrentIndex + 1}.");
        LoadLevel(CurrentIndex + 1);
    }

    // Nạp level theo chỉ số (0 = Level 1). Hết danh sách thì dừng lại và log thông báo
    public void LoadLevel(int index)
    {
        if (levelSession == null)
        {
            Debug.LogError("[LevelManager] Chưa gán Level Session.", this);
            return;
        }

        if (index < 0 || index >= levels.Count)
        {
            Log("Đã hết danh sách level.");
            return;
        }

        LevelData data = levels[index];
        if (data == null)
        {
            Debug.LogError($"[LevelManager] Level ở vị trí {index} trong danh sách đang để trống.", this);
            return;
        }

        CurrentIndex = index;
        Log($"Nạp Level {index + 1}: {data.name}");
        levelSession.StartLevel(data);
    }

    // Tải lại level hiện tại từ đầu
    [ContextMenu("Reload Current Level")]
    public void ReloadCurrentLevel() => LoadLevel(CurrentIndex);

    private void Log(string message)
    {
        if (debugLog) Debug.Log($"[LevelManager] {message}", this);
    }
}