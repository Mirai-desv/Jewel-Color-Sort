using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePanel : BasePanel
{
    [Header("UI")]
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;

    [SerializeField] private Image portraitImage;

    [SerializeField] private Button nextButton;

    [Header("Typing")]
    [SerializeField, Min(0.001f)]
    private float characterDelay = 0.025f;

    private DialogueData _dialogue;
    private int _lineIndex;

    private Coroutine _typingCoroutine;

    private bool _isTyping;

    private Action _onFinished;

    private void Awake()
    {
        base.Awake();

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextClicked);
    }

    public void Setup(DialogueData dialogue, Action onFinished)
    {
        StopTyping();

        _dialogue = dialogue;
        _lineIndex = 0;
        _onFinished = onFinished;
        _isTyping = false;

        if (_dialogue == null || !_dialogue.HasDialogue)
        {
            Finish();
            return;
        }

        Show();
        ShowCurrentLine();
    }

    private void OnNextClicked()
    {
        if (_dialogue == null || !_dialogue.HasDialogue)
            return;

        if (_isTyping)
        {
            CompleteCurrentLine();
            return;
        }

        _lineIndex++;

        if (_lineIndex >= _dialogue.Lines.Count)
        {
            Finish();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (_dialogue == null)
            return;

        DialogueLine line = _dialogue.Lines[_lineIndex];

        if (speakerNameText != null)
            speakerNameText.SetText(line.SpeakerName ?? string.Empty);

        if (portraitImage != null)
        {
            portraitImage.sprite = line.Portrait;
            portraitImage.enabled = line.Portrait != null;
        }

        if (dialogueText != null)
            dialogueText.SetText(string.Empty);

        StopTyping();

        _typingCoroutine = StartCoroutine(TypeLine(line.Text));
    }

    private IEnumerator TypeLine(string text)
    {
        _isTyping = true;

        if (dialogueText == null)
        {
            _isTyping = false;
            yield break;
        }

        text ??= string.Empty;

        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text = text.Substring(0, i + 1);

            yield return new WaitForSecondsRealtime(characterDelay);
        }

        _isTyping = false;
        _typingCoroutine = null;
    }

    private void CompleteCurrentLine()
    {
        StopTyping();

        if (_dialogue == null ||
            _lineIndex < 0 ||
            _lineIndex >= _dialogue.Lines.Count)
        {
            return;
        }

        DialogueLine line = _dialogue.Lines[_lineIndex];

        if (dialogueText != null)
            dialogueText.SetText(line.Text ?? string.Empty);

        _isTyping = false;
    }

    private void StopTyping()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        _isTyping = false;
    }

    private void Finish()
    {
        StopTyping();

        _dialogue = null;

        Hide();

        Action callback = _onFinished;
        _onFinished = null;

        callback?.Invoke();
    }

    protected override void OnHide()
    {
        StopTyping();
        base.OnHide();
    }
}