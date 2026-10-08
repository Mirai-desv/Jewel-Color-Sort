using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraControll : MonoBehaviour
{
    [SerializeField] private GridView gridView;

    [Header("Zoom")]
    [SerializeField] private float minZoomMultiplier = 1f; // 1 = không cho zoom OUT nhỏ hơn khung fit ban đầu
    [SerializeField] private float maxZoomMultiplier = 3f; // cho zoom IN tối đa gấp 3 lần khung fit
    [SerializeField] private float scrollZoomSpeed = 0.5f; // chỉ dùng khi test bằng lăn chuột trong Editor

    [Header("Lề")]
    [SerializeField] private float paddingCells = 0.5f; // chừa thêm viền quanh lưới khi fit (tính theo số ô)

    private Camera _camera;
    private Bounds _bounds;
    private float _fitSize; // Orthographic Size khi vừa khít màn hình
    private float _minSize; // size nhỏ nhất (zoom in nhiều nhất)
    private float _maxSize; // size lớn nhất (zoom out nhiều nhất)

    private bool _dragging;
    private Vector2 _lastScreenPos;

    private bool _pinching;
    private float _lastPinchDistance;

    private void Awake() => _camera = GetComponent<Camera>();

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
        if (_bounds.size == Vector3.zero) return; // chưa gọi FitToGrid() thì chưa có gì để xử lý

        HandlePinch();
        if (!_pinching) HandleDrag();
        HandleScrollZoom();
    }

    private void HandleDrag()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null) return;

        bool isPressed = pointer.press.isPressed;
        Vector2 pos = pointer.position.ReadValue();

        if (isPressed && !_dragging)
        {
            _dragging = true;
            _lastScreenPos = pos;
        }
        else if (isPressed && _dragging)
        {
            Vector2 delta = pos - _lastScreenPos;
            _lastScreenPos = pos;

            // Quy đổi delta pixel -> đơn vị thế giới theo mức zoom hiện tại
            float unitsPerPixel = (_camera.orthographicSize * 2f) / Screen.height;
            transform.position -= new Vector3(delta.x, delta.y, 0f) * unitsPerPixel;

            ClampPosition();
        }
        else if (!isPressed && _dragging)
        {
            _dragging = false;
        }
    }

    private void HandlePinch()
    {
        Touchscreen touch = Touchscreen.current;
        if (touch == null || touch.touches.Count < 2) { _pinching = false; return; }

        var t0 = touch.touches[0];
        var t1 = touch.touches[1];
        if (!t0.press.isPressed || !t1.press.isPressed) { _pinching = false; return; }

        float distance = Vector2.Distance(t0.position.ReadValue(), t1.position.ReadValue());

        if (!_pinching)
        {
            _pinching = true;
            _lastPinchDistance = distance;
            return;
        }

        if (_lastPinchDistance > 0.01f)
        {
            float ratio = _lastPinchDistance / distance; // 2 ngón ra xa nhau hơn -> zoom in (size giảm)
            ApplyZoom(_camera.orthographicSize * ratio);
        }
        _lastPinchDistance = distance;
    }

    private void HandleScrollZoom()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        ApplyZoom(_camera.orthographicSize - scroll * scrollZoomSpeed * 0.01f);
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

        // Khung nhìn rộng/cao hơn cả lưới thì không có khoảng để kéo -> giữ nguyên giữa
        float x = minX > maxX ? _bounds.center.x : Mathf.Clamp(transform.position.x, minX, maxX);
        float y = minY > maxY ? _bounds.center.y : Mathf.Clamp(transform.position.y, minY, maxY);

        transform.position = new Vector3(x, y, transform.position.z);
    }
}