using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform _rect;
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;
    private CanvasScaler _scaler;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        Apply();
    }

    private void Update()
    {
        // Đổi hướng màn hình / đổi độ phân giải thì tính lại
        if (Screen.safeArea != _lastSafeArea ||
            Screen.width != _lastScreenSize.x ||
            Screen.height != _lastScreenSize.y)
        {
            Apply();
        }
    }

    private void AdjustScaler()
    {
        if (_scaler == null) _scaler = GetComponentInParent<CanvasScaler>();
        if (_scaler == null || _scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize) return;
        Vector2 refRes = _scaler.referenceResolution;
        if (refRes.x <= 0 || refRes.y <= 0) return;
        float refAspect = refRes.x / refRes.y;
        float screenAspect = (float)Screen.width / Screen.height;
        // Màn hình dài/hẹp (tall phone 19.5:9, 20:9): match theo Chiều rộng để không bị bóp ngang
        // Màn hình vuông/rộng (tablet/iPad 4:3, 3:2): match theo Chiều cao để không bị cắt trên/dưới
        _scaler.matchWidthOrHeight = screenAspect < refAspect ? 0f : 1f;
    }

    private void Apply()
    {
        AdjustScaler();

        Rect safe = Screen.safeArea;
        _lastSafeArea = safe;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        if (Screen.width <= 0 || Screen.height <= 0) return;

        Vector2 min = safe.position;
        Vector2 max = safe.position + safe.size;
        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;

        _rect.anchorMin = min;
        _rect.anchorMax = max;
        _rect.offsetMin = Vector2.zero;
        _rect.offsetMax = Vector2.zero;
    }
}
