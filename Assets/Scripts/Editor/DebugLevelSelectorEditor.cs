#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(DebugLevelSelector))]
public class DebugLevelSelectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DebugLevelSelector selector = (DebugLevelSelector)target;

        serializedObject.Update();

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Jewel Color Sort - Developer Tools", EditorStyles.boldLabel);

        // Nút toggle to, nổi bật, dễ bấm
        Color defaultBg = GUI.backgroundColor;
        bool isShowing = selector.ShowDebugButton;

        if (isShowing)
        {
            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.4f); // Xanh lá nổi bật khi đang hiện
            if (GUILayout.Button("👁 NÚT DEBUG: ĐANG HIỆN (Nhấp vào đây để ẨN)", GUILayout.Height(42)))
            {
                Undo.RecordObject(selector, "Hide Debug Level Button");
                selector.ShowDebugButton = false;
                EditorUtility.SetDirty(selector);
                if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(selector.gameObject.scene);
            }
        }
        else
        {
            GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f); // Xám khi đang ẩn
            if (GUILayout.Button("🙈 NÚT DEBUG: ĐANG ẨN (Nhấp vào đây để HIỆN)", GUILayout.Height(42)))
            {
                Undo.RecordObject(selector, "Show Debug Level Button");
                selector.ShowDebugButton = true;
                EditorUtility.SetDirty(selector);
                if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(selector.gameObject.scene);
            }
        }

        GUI.backgroundColor = defaultBg;
        EditorGUILayout.Space(8);

        EditorGUILayout.HelpBox(
            "💡 Nút này giúp bạn vào thẳng bất kỳ level nào (1-15) để kiểm tra màn chơi.\n" +
            "• Bạn có thể ẩn nút này đi bất kỳ lúc nào bằng nút gạt ở trên hoặc bỏ tích 'Show Debug Button'.\n" +
            "• Khi ẩn đi, nút sẽ hoàn toàn không xuất hiện trên màn hình chính.",
            MessageType.Info);

        EditorGUILayout.Space(8);
        DrawDefaultInspector();

        if (Application.isPlaying)
        {
            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Thao Tác Nhanh Khi Đang Chơi", EditorStyles.boldLabel);
            if (GUILayout.Button("🚀 Mở Bảng Chọn Level Ngay Bây Giờ", GUILayout.Height(36)))
            {
                selector.OpenLevelSelector();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
