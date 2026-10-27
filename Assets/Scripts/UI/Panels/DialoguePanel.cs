using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Đoạn hội thoại kiểu visual novel: chữ chạy dần, bấm Next để hiện đủ câu rồi sang câu kế.
/// Có thể bật/tắt nút Skip (bỏ qua cả đoạn thoại).
/// </summary>
public class DialoguePanel : BasePanel, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image backgroundImage;   // tuỳ chọn
    [SerializeField] private Button nextButton;       // nên là nút phủ cả màn hình để bấm đâu cũng được

    [Header("Portrait Dimming")]
    [Tooltip("Màu của portrait khi NPC đang thoại")]
    [SerializeField] private Color activeSpeakerColor = Color.white;
    [Tooltip("Màu của portrait khi không phải NPC thoại (Ichira/người dẫn chuyện thoại)")]
    [SerializeField] private Color dimmedSpeakerColor = new Color(0.52f, 0.50f, 0.48f, 1f);

    [Header("Skip")]
    [Tooltip("Tắt = người chơi không thể bỏ qua thoại.")]
    [SerializeField] private bool allowSkip = false;
    [SerializeField] private Button skipButton;

    [Header("Typing")]
    [SerializeField, Min(0.001f)] private float characterDelay = 0.025f;

    private DialogueData _dialogue;
    private int _lineIndex;
    private Coroutine _typing;
    private bool _isTyping;
    private Action _onFinished;

    protected override void Awake()
    {
        base.Awake();

        if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipClicked);
            skipButton.gameObject.SetActive(allowSkip);
        }
    }

    private void OnDestroy()
    {
        if (nextButton != null) nextButton.onClick.RemoveListener(OnNextClicked);
        if (skipButton != null) skipButton.onClick.RemoveListener(OnSkipClicked);
    }

    public void Setup(DialogueData dialogue, Action onFinished)
    {
        StopTyping();

        _dialogue = dialogue;
        _lineIndex = 0;
        _onFinished = onFinished;

        if (_dialogue == null || !_dialogue.HasDialogue)
        {
            Finish();
            return;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }

        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(false);
        }

        if (skipButton != null) skipButton.gameObject.SetActive(allowSkip);

        Show();
        ShowCurrentLine();
    }

    private void OnNextClicked()
    {
        if (_dialogue == null) return;
        AudioManager.PlaySfx(Sfx.Click);

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

    public void OnPointerClick(PointerEventData eventData)
    {
        OnNextClicked();
    }

    private void OnSkipClicked()
    {
        if (!allowSkip || _dialogue == null) return;
        AudioManager.PlaySfx(Sfx.Click);
        Finish();
    }

    private bool IsNpcSpeaking(string speakerName)
    {
        if (string.IsNullOrWhiteSpace(speakerName)) return false;
        if (speakerName.Equals("Ichira", StringComparison.OrdinalIgnoreCase)) return false;
        if (speakerName.Equals("Chủ tiệm", StringComparison.OrdinalIgnoreCase)) return false;
        if (speakerName.Equals("Tôi", StringComparison.OrdinalIgnoreCase)) return false;
        return true;
    }

    private void ShowCurrentLine()
    {
        DialogueLine line = _dialogue.Lines[_lineIndex];

        if (speakerNameText != null) speakerNameText.SetText(line.SpeakerName ?? string.Empty);

        if (portraitImage != null)
        {
            if (line.Portrait != null)
            {
                portraitImage.sprite = line.Portrait;
                portraitImage.enabled = true;
            }
            else if (portraitImage.sprite == null)
            {
                portraitImage.enabled = false;
            }

            bool isNpc = IsNpcSpeaking(line.SpeakerName);
            portraitImage.color = isNpc ? activeSpeakerColor : dimmedSpeakerColor;
        }

        if (backgroundImage != null)
        {
            bool hasBg = line.Background != null;
            backgroundImage.gameObject.SetActive(hasBg);
            if (hasBg) backgroundImage.sprite = line.Background;
        }

        StopTyping();
        _typing = StartCoroutine(TypeLine(line.Text ?? string.Empty));
    }

    private IEnumerator TypeLine(string text)
    {
        if (dialogueText == null) yield break;

        _isTyping = true;

        // Đặt cả câu rồi mở dần từng ký tự: đúng cả với thẻ rich text
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();
        int total = dialogueText.textInfo.characterCount;

        var wait = new WaitForSecondsRealtime(characterDelay);
        for (int i = 1; i <= total; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return wait;
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        _isTyping = false;
        _typing = null;
    }

    private void CompleteCurrentLine()
    {
        StopTyping();
        if (dialogueText != null) dialogueText.maxVisibleCharacters = int.MaxValue;
    }

    private void StopTyping()
    {
        if (_typing != null)
        {
            StopCoroutine(_typing);
            _typing = null;
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
        _dialogue = null;
        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }
        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(false);
        }
        base.OnHide();
    }
}