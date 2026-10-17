/*using System;
using UnityEngine;

public class PostLevelManager : MonoBehaviour
{
    [SerializeField] private DialoguePanel dialoguePanel;

    [SerializeField] private RewardPanel rewardPanel;

    private enum FlowState
    {
        None,
        Dialogue,
        Reward
    }

    private FlowState _state = FlowState.None;

    private LevelData _levelData;

    private string _npcName;

    public bool IsRunning =>
        _state != FlowState.None;

    public void StartFlow(
        LevelData levelData,
        string npcName = null)
    {
        if (IsRunning)
        {
            Debug.LogWarning(
                "PostLevelManager: Flow is already running."
            );

            return;
        }

        if (levelData == null)
        {
            Debug.LogWarning(
                "PostLevelManager: LevelData is null."
            );

            FinishFlow();
            return;
        }

        _levelData = levelData;
        _npcName = npcName;

        DialogueData dialogue = _levelData.Dialogue;

        if (dialogue != null && dialogue.HasDialogue)
        {
            StartDialogue(dialogue);
            return;
        }

        StartReward();
    }

    private void StartDialogue(DialogueData dialogue)
    {
        if (dialoguePanel == null)
        {
            Debug.LogWarning(
                "PostLevelManager: DialoguePanel is missing."
            );

            StartReward();
            return;
        }

        _state = FlowState.Dialogue;

        dialoguePanel.Setup(
            dialogue,
            StartReward
        );
    }

    private void StartReward()
    {
        RewardData reward =
            _levelData != null
                ? _levelData.Reward
                : null;

        if (reward == null)
        {
            FinishFlow();
            return;
        }

        if (rewardPanel == null)
        {
            Debug.LogWarning(
                "PostLevelManager: RewardPanel is missing."
            );

            FinishFlow();
            return;
        }

        _state = FlowState.Reward;

        rewardPanel.Setup(
            reward,
            _npcName,
            FinishFlow
        );
    }

    private void FinishFlow()
    {
        _state = FlowState.None;

        LevelData completedLevel = _levelData;

        _levelData = null;

        string npcName = _npcName;

        _npcName = null;

        PostLevelCompleted?.Invoke(
            completedLevel,
            npcName
        );
    }

    public event Action<LevelData, string> PostLevelCompleted;
}
*/