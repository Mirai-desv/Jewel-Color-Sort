using System;
using UnityEngine;

public class PostLevelManager : MonoBehaviour
{
    [SerializeField] private DialoguePanel dialoguePanel;
    [SerializeField] private RewardPanel rewardPanel;

    private bool _dialogueRunning;
    private bool _rewardRunning;
    private Action _onFinished;

    public bool IsRunning => _dialogueRunning || _rewardRunning;

    public bool HasDialogue(LevelData level) =>
        level != null && dialoguePanel != null && level.Dialogue != null && level.Dialogue.HasDialogue;

    public bool HasReward(LevelData level) =>
        level != null && rewardPanel != null && level.Reward != null;

    // Dialogue chạy TRƯỚC khi chơi level
    public void PlayDialogue(LevelData level, Action onFinished)
    {
        if (!HasDialogue(level)) { onFinished?.Invoke(); return; }

        _dialogueRunning = true;
        _onFinished = onFinished;
        dialoguePanel.Setup(level.Dialogue, () =>
        {
            _dialogueRunning = false;
            Action cb = _onFinished; _onFinished = null;
            cb?.Invoke();
        });
    }

    // Reward chạy SAU khi thắng
    public void StartFlow(LevelData level, Action onFinished)
    {
        if (!HasReward(level)) { onFinished?.Invoke(); return; }

        _rewardRunning = true;
        _onFinished = onFinished;
        rewardPanel.Setup(level.Reward, () =>
        {
            _rewardRunning = false;
            Action cb = _onFinished; _onFinished = null;
            cb?.Invoke();
        });
    }

    public void Abort()
    {
        if (!IsRunning) return;
        _dialogueRunning = _rewardRunning = false;
        _onFinished = null;
        if (dialoguePanel != null) dialoguePanel.Hide();
        if (rewardPanel != null) rewardPanel.Hide();
    }
}