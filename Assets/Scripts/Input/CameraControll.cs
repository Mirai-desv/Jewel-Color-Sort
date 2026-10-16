using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Điều khiển Camera 2D (Orthographic): tự canh GIỮA lưới và fit vừa màn hình khi nạp level,
/// cho phép pan (kéo) và zoom (pinch / lăn chuột), luôn giữ lưới nằm trong tầm nhìn.
/// Nhận cử chỉ từ GestureDetector. Gắn script này lên Main Camera.
/// LevelSession gọi FitToGrid() sau mỗi lần dựng lưới.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraControll : MonoBehaviour
{
    [SerializeField] private GridView gridView;
    [SerializeField] private GestureDetector gesture; // để trống thì tự tìm trong scene

    [Header("Zoom")]
    [SerializeField] private float minZoomMultiplier = 1f; // 1 = không cho zoom OUT quá khung fit ban đầu
    [SerializeField] private float maxZoomMultiplier = 3f; // cho zoom IN tối đa gấp 3 lần khung fit
    [SerializeField] private float scrollZoomSpeed = 0.5f; // chỉ để test bằng lăn chuột trong Editor

    [Header("Lề")]
    [SerializeField] private float paddingCells = 0.5f; // chừa viền quanh lưới khi fit (tính theo số ô)

    private Camera _camera;
    private Bounds _bounds;
    private float _fitSize;
    private float _minSize; // zoom in nhiều nhất
    private float _maxSize; // zoom out nhiều nhất

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (gesture == null) gesture = FindFirstObjectByType<GestureDetector>();
    }

    private void OnEnable()
    {
        if (gesture == null) return;
        gesture.Dragged += OnDragged;
        gesture.Pinched += OnPinched;
    }

    private void OnDisable()
    {
        if (gesture == null) return;
        gesture.Dragged -= OnDragged;
        gesture.Pinched -= OnPinched;
    }

    /// <summary>Canh giữa camera vào tâm lưới và tính lại khung zoom. Gọi lại mỗi khi nạp level mới.</summary>
    public void FitToGrid()
    {
        _bounds = gridView.WorldBounds;

        float halfH = _bounds.size.y * 0.5f + paddingCells;
        float halfWByAspect = (_bounds.size.x * 0.5f + paddingCells) / _camera.aspect;
        _fitSize = Mathf.Max(halfH, halfWByAspect);

        _minSize = _fitSize / maxZoomMultiplier;
        _maxSize = _fitSize * minZoomMultiplier;

        _camera.orthographicSize = _fitSize;
        transform.position = new Vector3(_bounds.center.x, _bounds.center.y, transform.position.z);

        ClampPosition();
    }

    private void Update()
    {
        if (_bounds.size == Vector3.zero) return;

        // Lăn chuột (chỉ để test trong Editor)
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        ApplyZoom(_camera.orthographicSize - scroll * scrollZoomSpeed * 0.01f);
    }

    private void OnDragged(Vector2 deltaPixels)
    {
        if (_bounds.size == Vector3.zero) return;

        // Đổi pixel -> đơn vị thế giới theo mức zoom hiện tại
        float unitsPerPixel = (_camera.orthographicSize * 2f) / Screen.height;
        transform.position -= new Vector3(deltaPixels.x, deltaPixels.y, 0f) * unitsPerPixel;

        ClampPosition();
    }

    private void OnPinched(float factor)
    {
        if (_bounds.size == Vector3.zero) return;
        ApplyZoom(_camera.orthographicSize / factor); // 2 ngón xa nhau (factor > 1) -> size giảm -> zoom in
    }

    private void ApplyZoom(float size)
    {
        _camera.orthographicSize = Mathf.Clamp(size, _minSize, _maxSize);
        ClampPosition();
    }

    private void ClampPosition()
    {
        float halfH = _camera.orthographicSize;
        float halfW = halfH * _camera.aspect;

        float minX = _bounds.min.x + halfW;
        float maxX = _bounds.max.x - halfW;
        float minY = _bounds.min.y + halfH;
        float maxY = _bounds.max.y - halfH;

        // Khung nhìn rộng/cao hơn cả lưới thì không có khoảng để kéo -> giữ ở giữa
        float x = minX > maxX ? _bounds.center.x : Mathf.Clamp(transform.position.x, minX, maxX);
        float y = minY > maxY ? _bounds.center.y : Mathf.Clamp(transform.position.y, minY, maxY);

        transform.position = new Vector3(x, y, transform.position.z);
    }
}
