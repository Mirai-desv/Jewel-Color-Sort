using System;
using UnityEngine;

// Chạy chuỗi sau khi qua màn: Dialogue (nếu có) -> Reward (nếu có) -> gọi callback.
// Chỉ lo phần trình chiếu, việc cộng điểm / vật phẩm đã làm ở GameManager lúc thắng.
public class PostLevelManager : MonoBehaviour
{
    [SerializeField] private DialoguePanel dialoguePanel;
    [SerializeField] private RewardPanel rewardPanel;

    private enum Step { None, Dialogue, Reward }

    private Step _step = Step.None;
    private LevelData _level;
    private Action _onFinished;

    public bool IsRunning => _step != Step.None;

    // Level này có thưởng sau màn không
    public bool HasReward(LevelData level)
    {
        if (level == null) return false;
        return rewardPanel != null && level.Reward != null;
    }

    public bool HasContent(LevelData level) => HasReward(level);

    public void StartFlow(LevelData level, Action onFinished)
    {
        if (IsRunning)
        {
            Debug.LogWarning("[PostLevelManager] Flow đang chạy.", this);
            return;
        }

        _level = level;
        _onFinished = onFinished;

        StartReward();
    }

    private void StartReward()
    {
        RewardData reward = _level != null ? _level.Reward : null;
        if (reward == null || rewardPanel == null)
        {
            Finish();
            return;
        }

        _step = Step.Reward;
        rewardPanel.Setup(reward, Finish);
    }

    private void Finish()
    {
        _step = Step.None;
        _level = null;

        Action callback = _onFinished;
        _onFinished = null;
        callback?.Invoke();
    }

    // Huỷ giữa chừng (vd người chơi thoát về menu). Không gọi callback.
    public void Abort()
    {
        if (!IsRunning) return;

        _step = Step.None;
        _level = null;
        _onFinished = null;

        if (dialoguePanel != null) dialoguePanel.Hide();
        if (rewardPanel != null) rewardPanel.Hide();
    }
}