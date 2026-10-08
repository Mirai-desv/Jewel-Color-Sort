using System.Collections.Generic;
using UnityEngine;

public class BackgroundView : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float washOut = 0.45f;
    [SerializeField] private int sortingOrder = 0;

    private SpriteRenderer _renderer;
    private Texture2D _texture;
    private Sprite _sprite;

    public void Build(IReadOnlyList<CellData> cells, int width, int height, ColorPalette palette, float cellSize)
    {
        Clear();
        _renderer = GetComponent<SpriteRenderer>();

        // Mặc định trong suốt (0, 0, 0, 0)
        var pixels = new Color32[width * height];
        for (int i = 0; i < cells.Count; i++)
        {
            CellData c = cells[i];
            if (c.type == Type.None) continue;
            if (c.x < 0 || c.x >= width || c.y < 0 || c.y >= height) continue;

            Color color = Color.Lerp(palette.GetColor(c.type), Color.white, washOut);
            pixels[c.y * width + c.x] = color;
        }

        _texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            name = "BackgroundTexture",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        _texture.SetPixels32(pixels);
        _texture.Apply(false, true); // Không cần mipmap, giải phóng bản sao CPU

        // Pivot (0, 0) = góc dưới trái; pixelsPerUnit = 1/cellSize để mỗi pixel đúng bằng 1 ô
        _sprite = Sprite.Create(_texture, new Rect(0, 0, width, height), Vector2.zero, 1f / cellSize, 0, SpriteMeshType.FullRect);

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