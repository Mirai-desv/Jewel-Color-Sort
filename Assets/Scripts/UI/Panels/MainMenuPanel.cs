using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuPanel : BasePanel, IPointerClickHandler, IPointerDownHandler
{
    [Header("Chế độ chơi")]
    [Tooltip("Bật chế độ Tap To Play (chữ nhấp nháy, chạm bất kỳ đâu để chơi). Tắt đi sẽ quay lại nút Level cũ.")]
    [SerializeField] private bool useTapToPlayMode = true;

    [Header("Tham chiếu UI")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image startButtonImage;
    [SerializeField] private string tapToPlayString = "TAP TO PLAY";

    private Image _panelRaycaster;
    private PulsingText _pulsingEffect;
    private Vector2 _originalStartButtonSize;
    private RectTransform _startButtonRect;
    private Color _originalButtonColor = Color.white;
    private bool _gameStarted;

    protected override void Awake()
    {
        base.Awake();

        // Đảm bảo MainMenuPanel có thể bắt sự kiện chạm toàn màn hình khi ở Menu
        _panelRaycaster = GetComponent<Image>();
        if (_panelRaycaster == null)
        {
            _panelRaycaster = gameObject.AddComponent<Image>();
        }
        _panelRaycaster.enabled = true;
        _panelRaycaster.color = new Color(0, 0, 0, 0); // Trong suốt hoàn toàn
        _panelRaycaster.raycastTarget = useTapToPlayMode;

        // Tìm Image và RectTransform của StartButton nếu chưa gán
        if (levelText != null)
        {
            if (startButtonImage == null)
                startButtonImage = levelText.GetComponentInParent<Image>();

            if (startButtonImage != null)
                _originalButtonColor = startButtonImage.color;

            if (levelText.transform.parent is RectTransform parentRect)
            {
                _startButtonRect = parentRect;
                _originalStartButtonSize = parentRect.sizeDelta;
            }

            _pulsingEffect = levelText.GetComponent<PulsingText>();
            if (_pulsingEffect == null && useTapToPlayMode)
            {
                _pulsingEffect = levelText.gameObject.AddComponent<PulsingText>();
            }
        }
    }

    protected override void OnShow()
    {
        _gameStarted = false;

        if (useTapToPlayMode)
        {
            // Bật raycaster toàn màn hình
            if (_panelRaycaster != null)
            {
                _panelRaycaster.enabled = true;
                _panelRaycaster.color = new Color(0, 0, 0, 0);
                _panelRaycaster.raycastTarget = true;
            }

            // Làm nút hồng trong suốt nhưng VẪN giữ raycastTarget để bấm vào chữ cũng nhận
            if (startButtonImage != null)
            {
                startButtonImage.enabled = true;
                startButtonImage.color = new Color(1, 1, 1, 0); // Trong suốt
                startButtonImage.raycastTarget = true;
            }

            // Mở rộng chiều ngang cho chữ TAP TO PLAY thoáng đẹp
            if (_startButtonRect != null)
            {
                _startButtonRect.sizeDelta = new Vector2(Mathf.Max(600f, _originalStartButtonSize.x), _originalStartButtonSize.y);
            }

            // Đổi chữ thành TAP TO PLAY trắng viền đen
            if (levelText != null)
            {
                levelText.raycastTarget = false;
                levelText.color = Color.white;
                levelText.enableAutoSizing = true;
                levelText.fontSizeMin = 36;
                levelText.fontSizeMax = 64;
                levelText.alignment = TextAlignmentOptions.Center;
                levelText.SetText(tapToPlayString);

                if (_pulsingEffect == null)
                    _pulsingEffect = levelText.GetComponent<PulsingText>() ?? levelText.gameObject.AddComponent<PulsingText>();

                if (_pulsingEffect != null)
                {
                    _pulsingEffect.enabled = true;
                    _pulsingEffect.ApplyOutline();
                }
            }
        }
        else
        {
            // Quay lại hiển thị nút Level truyền thống
            if (startButtonImage != null)
            {
                startButtonImage.enabled = true;
                startButtonImage.color = _originalButtonColor;
                startButtonImage.raycastTarget = true;
            }

            if (_startButtonRect != null)
            {
                _startButtonRect.sizeDelta = _originalStartButtonSize;
            }

            if (levelText != null)
            {
                levelText.SetText("Level {0}", SaveManager.CurrentLevelIndex + 1);
                if (_pulsingEffect != null) _pulsingEffect.enabled = false;
            }

            if (_panelRaycaster != null) _panelRaycaster.raycastTarget = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        TriggerPlay();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerPlay();
    }

    private void TriggerPlay()
    {
        if (!useTapToPlayMode || _gameStarted) return;
        _gameStarted = true;

        AudioManager.PlaySfx(Sfx.Click);
        GameManager.Instance.StartGame();
    }
}
