using System.Collections.Generic;
using UnityEngine;

public class BackgroundView : MonoBehaviour
{
    private const int CellResolution = 16;
    [SerializeField, Range(0f, 1f)] private float washOut = 0f;
    [SerializeField] private int sortingOrder = 0;

    private SpriteRenderer _renderer;
    private Texture2D _texture;
    private Sprite _sprite;

    private static readonly float[] SocketDistances = PrecomputeDistances();

    private static float[] PrecomputeDistances()
    {
        var d = new float[CellResolution * CellResolution];
        float center = (CellResolution - 1) * 0.5f;
        for (int y = 0; y < CellResolution; y++)
        {
            for (int x = 0; x < CellResolution; x++)
            {
                float dx = x - center;
                float dy = y - center;
                d[y * CellResolution + x] = Mathf.Sqrt(dx * dx + dy * dy);
            }
        }
        return d;
    }

    public void Build(IReadOnlyList<CellData> cells, int width, int height, ColorPalette palette, float cellSize)
    {
        Clear();
        _renderer = GetComponent<SpriteRenderer>();

        int texW = width * CellResolution;
        int texH = height * CellResolution;
        var pixels = new Color32[texW * texH];

        for (int i = 0; i < cells.Count; i++)
        {
            CellData c = cells[i];
            if (c.type == Type.None) continue;
            if (c.x < 0 || c.x >= width || c.y < 0 || c.y >= height) continue;

            Color rawColor = palette.GetColor(c.type);
            // Giữ nguyên 100% màu sắc gốc (không ép washOut pha trắng làm biến đổi sắc độ/tông màu)
            Color baseColor = (washOut > 0f) ? Color.Lerp(rawColor, Color.white, washOut) : rawColor;
            float gray = baseColor.grayscale;
            bool isDark = gray < 0.22f;

            int startX = c.x * CellResolution;
            int startY = c.y * CellResolution;

            for (int py = 0; py < CellResolution; py++)
            {
                int rowIdx = (startY + py) * texW;
                float dy = py - 7.5f;

                for (int px = 0; px < CellResolution; px++)
                {
                    float dist = SocketDistances[py * CellResolution + px];

                    float r = baseColor.r;
                    float g = baseColor.g;
                    float b = baseColor.b;

                    if (dist < 3.8f)
                    {
                        // Lỗ cắm hạt (concave socket hole): đổ bóng nhẹ tạo chiều sâu 3D tươi sáng
                        float shade = 1f - (dy * 0.03f);
                        float factor = 0.88f * shade;
                        r *= factor;
                        g *= factor;
                        b *= factor;
                    }
                    else if (dist <= 5.4f)
                    {
                        // Viền mép lõm (socket bevel ring)
                        if (isDark)
                        {
                            // Đối với các ô màu tối / đen: làm sáng nhẹ viền khuyên để người chơi thấy rõ lỗ cắm hạt
                            r = Mathf.Min(1f, r * 1.2f + 0.22f);
                            g = Mathf.Min(1f, g * 1.2f + 0.22f);
                            b = Mathf.Min(1f, b * 1.2f + 0.22f);
                        }
                        else
                        {
                            // Hiệu ứng viền nổi 3D
                            float bevel = 1f - (dy * 0.05f);
                            r = Mathf.Clamp01(r * bevel);
                            g = Mathf.Clamp01(g * bevel);
                            b = Mathf.Clamp01(b * bevel);
                        }
                    }
                    else if (dist >= 7.0f)
                    {
                        // Đường viền ngăn cách giữa các ô trên lưới
                        r *= 0.94f;
                        g *= 0.94f;
                        b *= 0.94f;
                    }

                    pixels[rowIdx + (startX + px)] = new Color(
                        Mathf.Clamp01(r),
                        Mathf.Clamp01(g),
                        Mathf.Clamp01(b),
                        1f
                    );
                }
            }
        }

        _texture = new Texture2D(texW, texH, TextureFormat.RGBA32, false)
        {
            name = "BackgroundTexture",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        _texture.SetPixels32(pixels);
        _texture.Apply(false, true);

        // pixelsPerUnit = CellResolution / cellSize
        _sprite = Sprite.Create(_texture, new Rect(0, 0, texW, texH), Vector2.zero, CellResolution / cellSize, 0, SpriteMeshType.FullRect);

        _renderer.sprite = _sprite;
        _renderer.sortingOrder = sortingOrder;
    }

    public void Clear()
    {
        if (_renderer != null) _renderer.sprite = null;
        if (_sprite != null) Destroy(_sprite);
        if (_texture != null) Destroy(_texture);
        _sprite = null;
        _texture = null;
    }

    private void OnDestroy() => Clear();
}