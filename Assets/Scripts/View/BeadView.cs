using UnityEngine;

/// <summary>
/// Đại diện cho 1 hạt trên màn hình. Chỉ lo hiển thị, không chứa luật chơi.
/// Đổi trạng thái bằng scale và vertex color (không dùng MaterialPropertyBlock)
/// nên vẫn giữ được batching.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BeadView : MonoBehaviour
{
    [Header("Hiển thị")]
    [SerializeField, Range(0.5f, 1f)] private float fillRatio = 0.92f;      // hạt chiếm bao nhiêu phần của ô
    [SerializeField, Range(1f, 1.5f)] private float selectedScale = 1.12f;  // phóng nhẹ khi được chọn
    [SerializeField, Range(0f, 1f)] private float lockedDarken = 0.15f;     // hạt đã đúng chỗ tối đi 1 chút

    private SpriteRenderer _renderer;
    private Color _baseColor = Color.white;
    private float _baseScale = 1f;

    public Type CurrentType { get; private set; } = Type.None;
    public bool IsSelected { get; private set; }
    public bool IsLocked { get; private set; }

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
        Renderer.sortingOrder = sortingOrder;

        // Tự co giãn để sprite luôn vừa 1 ô, bất kể kích thước ảnh gốc
        float spriteWidth = sprite != null ? sprite.bounds.size.x : 1f;
        _baseScale = cellSize * fillRatio / spriteWidth;

        IsSelected = false;
        IsLocked = false;
        ApplyVisual();
    }

    public void SetSelected(bool selected)
    {
        if (IsSelected == selected) return;
        IsSelected = selected;
        ApplyVisual();
    }

    public void SetLocked(bool locked)
    {
        if (IsLocked == locked) return;
        IsLocked = locked;
        ApplyVisual();
    }

    // Đẩy hạt lên trên khi đang bay (dùng cho BeadFlyAnimator)
    public void SetSortingOrder(int order) => Renderer.sortingOrder = order;

    // Gọi khi trả hạt về pool
    public void ResetState()
    {
        CurrentType = Type.None;
        IsSelected = false;
        IsLocked = false;
    }

    private void ApplyVisual()
    {
        float scale = IsSelected ? _baseScale * selectedScale : _baseScale;
        transform.localScale = new Vector3(scale, scale, 1f);

        Renderer.color = IsLocked
            ? Color.Lerp(_baseColor, Color.black, lockedDarken)
            : _baseColor;
    }
}