using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class BaseButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField, Range(0.8f, 1f)] private float pressedScale = 0.92f;

    private Button _button;
    private Vector3 _baseScale;

    protected GameManager Game => GameManager.Instance;

    protected virtual void Awake()
    {
        _button = GetComponent<Button>();
        _baseScale = transform.localScale;
        _button.onClick.AddListener(HandleClick);
    }

    protected virtual void OnDestroy()
    {
        if (_button != null) _button.onClick.RemoveListener(HandleClick);
    }

    private void OnDisable() => transform.localScale = _baseScale; // tránh kẹt scale nếu bị tắt lúc đang nhấn

    public void SetInteractable(bool interactable) => _button.interactable = interactable;

    private void HandleClick()
    {
        AudioManager.PlaySfx(Sfx.Click);
        OnClicked();
    }

    // Việc nút này làm khi được bấm
    protected abstract void OnClicked();

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_button.interactable) transform.localScale = _baseScale * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData) => transform.localScale = _baseScale;
}
