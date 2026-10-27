using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "BeadSort/Color Palette")]
public class ColorPalette : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public Type type;
        public Color color;
    }

    [SerializeField] private Sprite beadSprite;
    [SerializeField] private Sprite glossSprite;
    [SerializeField] private List<Entry> entries = new List<Entry>();

    private Color[] _lookup; // tra cứu nhanh theo (int)Type

    public Sprite BeadSprite => beadSprite;
    public Sprite GlossSprite => glossSprite;

    // Trả về màu của loại
    public Color GetColor(Type type)
    {
        if (_lookup == null) BuildLookup();
        int i = (int)type;
        return (i >= 0 && i < _lookup.Length) ? _lookup[i] : Color.magenta;
    }

    // Đầu tiên thì tô bằng màu magenta, sau đó mới tô bằng màu thật
    // Nếu mà ko có màu đúng thì thay bằng màu magenta để dễ nhận ra và điều chỉnh (màu cam cũng dễ nhận biết mà nhỉ)
    private void BuildLookup()
    {
        int size = 1;
        for (int i = 0; i < entries.Count; i++)
            size = Mathf.Max(size, (int)entries[i].type + 1);

        _lookup = new Color[size];
        for (int i = 0; i < size; i++) _lookup[i] = Color.magenta;

        for (int i = 0; i < entries.Count; i++)
        {
            Color c = entries[i].color;
            c.a = 1f; // a ở đây là Alpha
            _lookup[(int)entries[i].type] = c;
        }
    }

    private void OnEnable() => _lookup = null;
    private void OnValidate() => _lookup = null;
}