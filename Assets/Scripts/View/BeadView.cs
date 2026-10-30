using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BeadView : MonoBehaviour
{
    [Header("Hiển thị")]
    [SerializeField, Range(0.5f, 1f)] private float fillRatio = 0.92f;      // hạt chiếm bao nhiêu phần của ô
    [SerializeField, Range(0f, 1f)] private float selectedLift = 0.3f;      // nhấc lên bao nhiêu ô
    [SerializeField, Range(0.02f, 0.5f)] private float liftDuration = 0.12f;    // thời gian nhấc/hạ (giây)
    [SerializeField, Range(0f, 1f)] private float lockedDarken = 0.15f;     // hạt đã đúng chỗ tối đi 1 chút

    private SpriteRenderer _renderer;
    private Color _baseColor = Color.white;
    private float _baseScale = 1f;
    private float _liftDistance;    // quãng nhấc theo đơn vị thế giới
    private float _liftT;           // 0 = nằm yên, 1 = nhấc hết cỡ
    private Vector3 _restPosition;  // vị trí lúc nằm yên
    private int _sortingOrder;      // thứ tự vẽ gốc

    public Type CurrentType { get; private set; } = Type.None;
    public bool IsSelected { get; private set; }
    public bool IsLocked { get; private set; }

    private void Awake() => enabled = false;

    // Lấy lười (lazy) để an toàn khi pool tạo hạt ở trạng thái inactive (Awake chưa chạy)
    private SpriteRenderer Renderer
    {
        get
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            return _renderer;
        }
    }

    // Cấu hình hạt khi lấy ra từ pool
    public void Setup(Type type, Color color, Sprite sprite, float cellSize, int sortingOrder)
    {
        CurrentType = type;
        _baseColor = color;

        Renderer.sprite = sprite;
        _sortingOrder = sortingOrder;

        // Tự co giãn để sprite luôn vừa 1 ô, bất kể kích thước ảnh gốc
        float spriteWidth = sprite != null ? sprite.bounds.size.x : 1f;
        _baseScale = cellSize * fillRatio / spriteWidth;

        _liftDistance = cellSize * selectedLift;

        IsSelected = false;
        IsLocked = false;
        _liftT = 0f;
        enabled = false;
        ApplyVisual();
        ApplySortingOrder();
    }

    public void SetSelected(bool selected)
    {
        if (IsSelected == selected) return;
        IsSelected = selected;
        
        if(selected && _liftT <= 0f) _restPosition = transform.position;

        ApplySortingOrder();
        enabled = true;
    }

    public void SetLocked(bool locked)
    {
        if (IsLocked == locked) return;
        IsLocked = locked;
        ApplyVisual();
    }

    // Đẩy hạt lên trên khi đang bay (dùng cho BeadFlyAnimator)
    public void SetSortingOrder(int order)
    {
        _sortingOrder = order;
        ApplySortingOrder();
    }

    public void StopLift()
    {
        _liftT = 0f;
        enabled = false;
        ApplySortingOrder();
    }

    // Gọi khi trả hạt về pool
    public void ResetState()
    {
        CurrentType = Type.None;
        IsSelected = false;
        IsLocked = false;
        _liftT = 0f;
        enabled = false;
    }

    private void ApplyVisual()
    {
        transform.localScale = new Vector3(_baseScale, _baseScale, 1f);

        Renderer.color = IsLocked
            ? Color.Lerp(_baseColor, Color.black, lockedDarken)
            : _baseColor;
    }

    private void ApplySortingOrder()
    {
        bool raised = IsSelected || _liftT > 0f;
        Renderer.sortingOrder = _sortingOrder + (raised ? 1 : 0);   // hạt nhấc nằm trên hạt xung quanh
    }

    private void Update()
    {
        float target = IsSelected ? 1f : 0f;
        _liftT = Mathf.MoveTowards(_liftT, target, Time.deltaTime / liftDuration);

        float eased = 1f - Mathf.Pow(1f - _liftT, 3f);
        transform.position = _restPosition + Vector3.up * (_liftDistance * eased);

        if(Mathf.Approximately(_liftT, target))
        {
            enabled = false;
            ApplySortingOrder();
        }
    }
}