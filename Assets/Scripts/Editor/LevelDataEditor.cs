using UnityEditor;
using UnityEngine;

/// <summary>Thêm nút Validate và thông tin kích thước lưới vào Inspector của LevelData.</summary>
[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var level = (LevelData)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            $"Lưới {level.Width} x {level.Height} | Nền: {level.Background.Count} ô | Hạt: {level.Beads.Count} hạt | Khay: {level.TraySlotCount} ô",
            MessageType.None);

        if (GUILayout.Button("Validate", GUILayout.Height(28)))
            level.Validate();
    }
}
