using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class BasePanel : MonoBehaviour
{
    [SerializeField, Min(0f)] private float fadeDuration = 0.2f; // 0 = hiện/ẩn ngay

    private CanvasGroup _group;
    private Coroutine _fade;

    public bool IsVisible { get; private set; }

    private CanvasGroup Group => _group != null ? _group : (_group = GetComponent<CanvasGroup>());

    protected virtual void Awake() => ApplyInstant(0f);

    public void Show()
    {
        if (IsVisible) return;
        IsVisible = true;

        gameObject.SetActive(true);
        CanvasGroup g = Group;
        g.blocksRaycasts = true;
        g.interactable = true;

        OnShow();
        FadeTo(1f);
    }

    public void Hide()
    {
        if (!IsVisible) return;
        IsVisible = false;

        OnHide();

        // Ngừng nhận click ngay lập tức khi bắt đầu ẩn
        CanvasGroup g = Group;
        g.blocksRaycasts = false;
        g.interactable = false;

        FadeTo(0f);
    }

    protected virtual void OnShow() { }
    protected virtual void OnHide() { }

    private void FadeTo(float target)
    {
        if (_fade != null) StopCoroutine(_fade);

        if (fadeDuration <= 0f || !isActiveAndEnabled)
        {
            ApplyInstant(target);
            return;
        }

        _fade = StartCoroutine(Fade(target));
    }

    private IEnumerator Fade(float target)
    {
        CanvasGroup g = Group;
        float start = g.alpha;

        if (target > 0.5f)
        {
            g.blocksRaycasts = true;
            g.interactable = true;
        }

        for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            g.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        ApplyInstant(target);
        _fade = null;
    }

    private void ApplyInstant(float alpha)
    {
        CanvasGroup g = Group;
        g.alpha = alpha;

        bool on = alpha > 0.5f;
        g.blocksRaycasts = on;
        g.interactable = on;
    }
}
