using System;
using UnityEngine;

public class PostLevelManager : MonoBehaviour
{
    [SerializeField] private DialoguePanel dialoguePanel;

    private bool _dialogueRunning;
    private Action _onFinished;

    public bool IsRunning => _dialogueRunning;

    public bool HasDialogue(LevelData level) =>
        level != null && dialoguePanel != null && level.Dialogue != null && level.Dialogue.HasDialogue;

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

    public void Abort()
    {
        if (!IsRunning) return;
        _dialogueRunning = false;
        _onFinished = null;
        if (dialoguePanel != null) dialoguePanel.Hide();
    }
}