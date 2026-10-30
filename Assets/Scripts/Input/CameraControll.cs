using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraControll : MonoBehaviour
{
    [SerializeField] private GridView gridView;
    [SerializeField] private GestureDetector gesture; // để trống thì tự tìm trong scene

    [Header("Vùng khả dụng (tính theo % chiều cao / chiều rộng màn hình)")]
    [Tooltip("Gán khay (hoặc cả cụm HUD + khay) nằm ở đỉnh màn hình. Ảnh sẽ nằm phía dưới mép dưới của nó.")]
    [SerializeField] private RectTransform topUIRect;
    [Tooltip("Dùng khi KHÔNG gán Top UI Rect: phần đỉnh màn hình dành cho khay/HUD.")]
    [SerializeField, Range(0f, 0.6f)] private float topReservedFraction = 0.25f;
    [Tooltip("Khoảng hở giữa mép dưới Top UI Rect và ảnh.")]
    [SerializeField, Range(0f, 0.2f)] private float topGapFraction = 0.02f;
    [Tooltip("Phần đáy màn hình dành cho nút (Undo...).")]
    [SerializeField, Range(0f, 0.4f)] private float bottomReservedFraction = 0.08f;
    [Tooltip("Khay ở đáy màn hình. Nếu để trống thì tự tìm TrayView.")]
    [SerializeField] private RectTransform bottomUIRect;
    [Tooltip("Khoảng hở giữa mép trên Bottom UI Rect và ảnh.")]
    [SerializeField, Range(0f, 0.2f)] private float bottomGapFraction = 0.02f;
    [SerializeField, Range(0f, 0.3f)] private float sidePaddingFraction = 0.05f;

    [Header("Kích thước ảnh lúc mới vào level")]
    [Tooltip("Ảnh chiếm bao nhiêu phần vùng khả dụng. Nhỏ hơn = ảnh bé hơn.")]
    [SerializeField, Range(0.3f, 1f)] private float fillRatio = 0.85f;

    [Header("Zoom")]
    [Tooltip("Phóng to tối đa gấp bao nhiêu lần so với lúc mới vào level.")]
    [SerializeField, Min(1f)] private float zoomInMultiplier = 4f;
    [Tooltip("Thu nhỏ tối đa gấp bao nhiêu lần so với lúc mới vào level.")]
    [SerializeField, Min(1f)] private float zoomOutMultiplier = 2f;
    [Tooltip("Mỗi nấc lăn chuột zoom thêm bấy nhiêu % (chỉ để test trong Editor).")]
    [SerializeField, Range(0.02f, 0.5f)] private float scrollZoomStep = 0.1f;

    // Tutorial bật để khoá kéo/zoom (vùng sáng và bàn tay tính theo camera, camera dịch là lệch)
    public bool InputLocked { get; set; }

    private Camera _camera;
    private Bounds _bounds;
    private bool _ready;
    private Vector2 _focus;   // điểm thế giới đang nằm ở TÂM vùng khả dụng
    private float _minSize;   // zoom in nhiều nhất
    private float _maxSize;   // zoom out nhiều nhất
    private float _top;       // phần đỉnh đang dành cho UI (0..1)
    private float _bottom;    // phần đáy đang dành cho UI (0..1)
    private readonly Vector3[] _corners = new Vector3[4];

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (gesture == null) gesture = FindAnyObjectByType<GestureDetector>();
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

    // Đặt lại khung nhìn cho level hiện tại. LevelSession gọi mỗi khi dựng lưới
    public void FitToGrid()
    {
        Canvas.ForceUpdateCanvases(); // để kích thước khay đã được Layout tính xong

        _bounds = gridView.WorldBounds;
        _top = ComputeTopReserved();
        _bottom = ComputeBottomReserved();

        float usableH = Mathf.Max(0.1f, 1f - _top - _bottom);
        float usableW = Mathf.Max(0.1f, 1f - 2f * sidePaddingFraction);

        // Orthographic Size nhỏ nhất để cả bức ảnh lọt vào vùng khả dụng, rồi nới ra theo Fill Ratio
        float sizeByHeight = _bounds.size.y * 0.5f / usableH;
        float sizeByWidth = _bounds.size.x * 0.5f / (_camera.aspect * usableW);
        float fitSize = Mathf.Max(sizeByHeight, sizeByWidth) / fillRatio;

        _minSize = fitSize / zoomInMultiplier;
        _maxSize = fitSize * zoomOutMultiplier;

        _focus = _bounds.center;
        _camera.orthographicSize = fitSize;
        _ready = true;
        ApplyCamera();
    }

    private void Update()
    {
        if (!_ready || InputLocked) return;

        // Lăn chuột (chỉ để test trong Editor): zoom quanh vị trí con trỏ
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        float factor = scroll > 0f ? 1f + scrollZoomStep : 1f / (1f + scrollZoomStep);
        ZoomAt(_camera.orthographicSize / factor, mouse.position.ReadValue());
    }

    // Thao tác
    private void OnDragged(Vector2 deltaPixels)
    {
        if (!_ready || InputLocked) return;

        // Đổi pixel -> đơn vị thế giới theo mức zoom hiện tại, kéo ảnh theo ngón tay
        float unitsPerPixel = (_camera.orthographicSize * 2f) / _camera.pixelHeight;
        _focus -= deltaPixels * unitsPerPixel;
        ApplyCamera();
    }

    private void OnPinched(float factor, Vector2 screenMidpoint)
    {
        if (!_ready || InputLocked) return;
        ZoomAt(_camera.orthographicSize / factor, screenMidpoint); // 2 ngón xa nhau (factor > 1) -> zoom in
    }

    // Zoom sao cho điểm thế giới đang nằm dưới screenPoint vẫn đứng yên
    private void ZoomAt(float newSize, Vector2 screenPoint)
    {
        newSize = Mathf.Clamp(newSize, _minSize, _maxSize);
        if (Mathf.Approximately(newSize, _camera.orthographicSize)) return;

        Vector3 before = ScreenToWorld(screenPoint);
        _camera.orthographicSize = newSize;
        ApplyCamera();
        Vector3 after = ScreenToWorld(screenPoint);

        _focus += (Vector2)(before - after);
        ApplyCamera();
    }

    private Vector3 ScreenToWorld(Vector2 p) =>
        _camera.ScreenToWorldPoint(new Vector3(p.x, p.y, -transform.position.z));

    // Đặt camera

    // Giữ điểm focus (trong phạm vi bức ảnh) nằm ở tâm vùng khả dụng.
    // Vùng khả dụng lệch khỏi tâm màn hình (vì khay ở trên, nút ở dưới) nên camera phải dịch ngược lại.

    private void ApplyCamera()
    {
        _focus.x = Mathf.Clamp(_focus.x, _bounds.min.x, _bounds.max.x);
        _focus.y = Mathf.Clamp(_focus.y, _bounds.min.y, _bounds.max.y);

        float yOffset = (_top - _bottom) * _camera.orthographicSize;
        transform.position = new Vector3(_focus.x, _focus.y + yOffset, transform.position.z);
    }

    // Phần đỉnh màn hình bị UI chiếm (0..1), tính từ mép dưới của Top UI Rect
    private float ComputeTopReserved()
    {
        if (topUIRect == null) return topReservedFraction;

        Canvas canvas = topUIRect.GetComponentInParent<Canvas>();
        Camera uiCamera = null;
        if (canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.rootCanvas.worldCamera;

        topUIRect.GetWorldCorners(_corners);
        float lowest = float.MaxValue;
        for (int i = 0; i < 4; i++)
        {
            float y = RectTransformUtility.WorldToScreenPoint(uiCamera, _corners[i]).y;
            if (y < lowest) lowest = y;
        }

        float reserved = 1f - lowest / _camera.pixelHeight + topGapFraction;
        return Mathf.Clamp(reserved, 0f, 0.6f);
    }

    // Phần đáy màn hình bị UI chiếm (0..1), tính từ mép trên của Bottom UI Rect (khay)
    private float ComputeBottomReserved()
    {
        if (bottomUIRect == null)
        {
            TrayView tray = FindAnyObjectByType<TrayView>();
            if (tray != null && tray.BackgroundRect != null)
                bottomUIRect = tray.BackgroundRect;
        }

        if (bottomUIRect == null) return bottomReservedFraction;

        Canvas canvas = bottomUIRect.GetComponentInParent<Canvas>();
        Camera uiCamera = null;
        if (canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.rootCanvas.worldCamera;

        bottomUIRect.GetWorldCorners(_corners);
        float highest = float.MinValue;
        for (int i = 0; i < 4; i++)
        {
            float y = RectTransformUtility.WorldToScreenPoint(uiCamera, _corners[i]).y;
            if (y > highest) highest = y;
        }

        float reserved = highest / _camera.pixelHeight + bottomGapFraction;
        return Mathf.Clamp(reserved, bottomReservedFraction, 0.5f);
    }
}