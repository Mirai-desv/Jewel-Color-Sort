using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hiện vật phẩm nhận được (ảnh + tên). Sau Continue Delay giây mới hiện nút "Next Level".
/// </summary>
public class RewardPanel : BasePanel
{
    [Header("Vật phẩm")]
    [SerializeField] private TMP_Text titleText;        // tuỳ chọn, vd "You got"
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;  // tuỳ chọn

    [Header("Nút tiếp tục")]
    [Tooltip("Nút (hoặc Image có gắn Button) ghi 'Next Level'. Bị ẩn cho đến khi hết Continue Delay.")]
    [SerializeField] private Button continueButton;
    [SerializeField, Min(0f)] private float continueDelay = 2f;

    private Action _onContinue;
    private Coroutine _delay;

    protected override void Awake()
    {
        base.Awake();
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void OnDestroy()
    {
        if (continueButton != null) continueButton.onClick.RemoveListener(OnContinueClicked);
    }

    public void Setup(RewardData reward, Action onContinue)
    {
        _onContinue = onContinue;

        if (itemImage != null)
        {
            itemImage.sprite = reward != null ? reward.Icon : null;
            itemImage.enabled = itemImage.sprite != null;
        }
        if (itemNameText != null) itemNameText.SetText(reward != null ? reward.ItemName : string.Empty);
        if (descriptionText != null) descriptionText.SetText(reward != null ? reward.Description : string.Empty);

        if (continueButton != null) continueButton.gameObject.SetActive(false);

        Show();

        if (_delay != null) StopCoroutine(_delay);
        _delay = StartCoroutine(ShowContinueAfterDelay());
    }

    private IEnumerator ShowContinueAfterDelay()
    {
        if (continueDelay > 0f) yield return new WaitForSecondsRealtime(continueDelay);

        if (continueButton != null) continueButton.gameObject.SetActive(true);
        _delay = null;
    }

    private void OnContinueClicked()
    {
        if (_onContinue == null) return; // chống bấm 2 lần
        AudioManager.PlaySfx(Sfx.Click);

        Action callback = _onContinue;
        _onContinue = null;

        Hide();
        callback.Invoke();
    }

    protected override void OnHide()
    {
        if (_delay != null)
        {
            StopCoroutine(_delay);
            _delay = null;
        }
        base.OnHide();
    }
}