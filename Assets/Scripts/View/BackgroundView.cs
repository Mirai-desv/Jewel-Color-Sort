using System.Collections.Generic;
using UnityEngine;

public class BackgroundView : MonoBehaviour
{
    private const int CellResolution = 16;
    [SerializeField, Range(0f, 1f)] private float washOut = 0.45f;
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
            float effectiveWash = Mathf.Max(washOut, 0.45f);
            if (rawColor.grayscale < 0.25f)
            {
                effectiveWash = 0.55f; // Tăng độ sáng nền cho các ô màu tối/đen
            }

            Color baseColor = Color.Lerp(rawColor, Color.white, effectiveWash);

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

                    if (dist < 3.5f)
                    {
                        // Lỗ cắm hạt (concave socket hole)
                        float shade = 1f - (dy * 0.04f);
                        r = (r * 0.42f + 0.08f) * shade;
                        g = (g * 0.42f + 0.08f) * shade;
                        b = (b * 0.42f + 0.10f) * shade;
                    }
                    else if (dist <= 5.5f)
                    {
                        // Viền mép lõm (socket bevel ring)
                        r *= 0.82f;
                        g *= 0.82f;
                        b *= 0.82f;
                    }
                    else if (dist >= 7.0f)
                    {
                        // Đường viền ngăn cách giữa các ô
                        r *= 0.92f;
                        g *= 0.92f;
                        b *= 0.92f;
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