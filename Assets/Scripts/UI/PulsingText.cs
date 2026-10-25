using TMPro;
using UnityEngine;

/// <summary>
/// Hiệu ứng chữ nhấp nháy mờ dần / sáng dần (Alpha Breathing Pulse) êm dịu.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class PulsingText : MonoBehaviour
{
    [Tooltip("Tốc độ nhịp thở")]
    [SerializeField] private float speed = 2.5f;

    [Tooltip("Độ mờ thấp nhất (0 = vô hình, 1 = rõ nét)")]
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.25f;

    [Tooltip("Độ rõ cao nhất")]
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 1.0f;

    [Tooltip("Sử dụng unscaledTime để không bị ảnh hưởng khi dừng game/pause")]
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Đường viền chữ (Outline / Stroke)")]
    [SerializeField] private bool enableOutline = true;
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField, Range(0f, 1f)] private float outlineWidth = 0.22f;

    private TMP_Text _text;
    private Color _baseColor;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        if (_text != null)
        {
            _baseColor = _text.color;
            ApplyOutline();
        }
    }

    public void ApplyOutline()
    {
        if (_text == null || !enableOutline) return;
        _text.outlineColor = outlineColor;
        _text.outlineWidth = outlineWidth;
    }

    private void OnEnable()
    {
        if (_text != null)
        {
            ApplyOutline();
            Color c = _text.color;
            c.a = maxAlpha;
            _text.color = c;
        }
    }

    private void Update()
    {
        if (_text == null) return;

        float time = useUnscaledTime ? Time.unscaledTime : Time.time;
        // Sóng sin chuyển đổi mượt mà từ 0 đến 1
        float t = (Mathf.Sin(time * speed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = _text.color;
        c.a = alpha;
        _text.color = c;
    }

    private void OnDisable()
    {
        if (_text != null)
        {
            Color c = _text.color;
            c.a = _baseColor.a > 0f ? _baseColor.a : 1f;
            _text.color = c;
        }
    }
}
