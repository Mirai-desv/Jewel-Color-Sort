using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class BaseButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [SerializeField, Range(0.85f, 1f)] private float pressedScale = 0.95f;

    private Button _button;
    private Vector3 _baseScale;
    private bool _isPressed;

    protected GameManager Game => GameManager.Instance;

    protected virtual void Awake()
    {
        _button = GetComponent<Button>();
        _baseScale = transform.localScale;
    }

    private void OnDisable()
    {
        transform.localScale = _baseScale;
        _isPressed = false;
    }

    public void SetInteractable(bool interactable)
    {
        if (_button != null) _button.interactable = interactable;
    }

    private void HandleClick()
    {
        AudioManager.PlaySfx(Sfx.Click);
        OnClicked();
    }

    // Việc nút này làm khi được bấm
    protected abstract void OnClicked();

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_button != null && _button.interactable)
        {
            _isPressed = true;
            transform.localScale = _baseScale * pressedScale;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = _baseScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.localScale = _baseScale;
        if (_isPressed && _button != null && _button.interactable)
        {
            _isPressed = false;
            HandleClick();
        }
    }
}
