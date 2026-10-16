using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform _rect;
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;

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

    private void Apply()
    {
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
