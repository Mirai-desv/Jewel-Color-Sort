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
    [SerializeField] private List<Entry> entries = new List<Entry>();

    private Color[] _lookup; // tra cứu nhanh theo (int)Type

    public Sprite BeadSprite => beadSprite;

    // Trả về màu của loại; thiếu khai báo thì trả magenta để dễ nhận ra
    public Color GetColor(Type type)
    {
        if (_lookup == null) BuildLookup();
        int i = (int)type;
        return (i >= 0 && i < _lookup.Length) ? _lookup[i] : Color.magenta;
    }

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
            c.a = 1f; // tránh quên chỉnh Alpha trong Inspector làm hạt tàng hình
            _lookup[(int)entries[i].type] = c;
        }
    }

    private void OnEnable() => _lookup = null;
    private void OnValidate() => _lookup = null;
}