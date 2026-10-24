using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BeadView : MonoBehaviour
{
    [Header("Hiển thị")]
    [SerializeField, Range(0.5f, 1f)] private float fillRatio = 0.92f;      // hạt chiếm bao nhiêu phần của ô
    [SerializeField, Range(1f, 1.5f)] private float selectedScale = 1.12f;  // phóng nhẹ khi được chọn
    [SerializeField, Range(0f, 1f)] private float lockedDarken = 0.15f;     // hạt đã đúng chỗ tối đi 1 chút

    [Header("Lớp bóng Jelly (Gloss / Shine Layer)")]
    [SerializeField] private SpriteRenderer glossRenderer;
    [SerializeField] private Sprite glossSprite;
    [SerializeField, Range(0f, 1f)] private float glossOpacity = 0.75f;

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

    private SpriteRenderer GlossRenderer
    {
        get
        {
            if (glossRenderer == null)
            {
                Transform child = transform.Find("Gloss");
                if (child != null)
                {
                    glossRenderer = child.GetComponent<SpriteRenderer>();
                }
                else if (glossSprite != null)
                {
                    GameObject go = new GameObject("Gloss", typeof(SpriteRenderer));
                    go.transform.SetParent(transform, false);
                    glossRenderer = go.GetComponent<SpriteRenderer>();
                }
            }
            return glossRenderer;
        }
    }

    // Cấu hình hạt khi lấy ra từ pool
    public void Setup(Type type, Color color, Sprite sprite, float cellSize, int sortingOrder, Sprite customGlossSprite = null)
    {
        CurrentType = type;
        _baseColor = color;

        Renderer.sprite = sprite;
        Renderer.sortingOrder = sortingOrder;

        if (customGlossSprite != null) glossSprite = customGlossSprite;

        SpriteRenderer gloss = GlossRenderer;
        if (gloss != null)
        {
            if (glossSprite != null) gloss.sprite = glossSprite;
            gloss.sortingOrder = sortingOrder + 1;
            gloss.color = new Color(1f, 1f, 1f, glossOpacity);
            gloss.enabled = (type != Type.None && glossSprite != null);
        }

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
    public void SetSortingOrder(int order)
    {
        Renderer.sortingOrder = order;
        if (GlossRenderer != null) GlossRenderer.sortingOrder = order + 1;
    }

    // Gọi khi trả hạt về pool
    public void ResetState()
    {
        CurrentType = Type.None;
        IsSelected = false;
        IsLocked = false;
        if (GlossRenderer != null) GlossRenderer.enabled = false;
    }

    private void ApplyVisual()
    {
        float scale = IsSelected ? _baseScale * selectedScale : _baseScale;
        transform.localScale = new Vector3(scale, scale, 1f);

        Renderer.color = IsLocked
            ? Color.Lerp(_baseColor, Color.black, lockedDarken)
            : _baseColor;

        if (GlossRenderer != null && GlossRenderer.sprite != null && CurrentType != Type.None)
        {
            GlossRenderer.enabled = true;
            GlossRenderer.color = new Color(1f, 1f, 1f, glossOpacity);
        }
    }
}