#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class LevelDataGeneratorWindow : EditorWindow
{
    private LevelData _targetLevel;
    private LevelManager _levelManager;

    [MenuItem("Tools/Jewel Sort/Level Manager Helper")]
    public static void ShowWindow()
    {
        GetWindow<LevelDataGeneratorWindow>("Level Helper");
    }

    private void OnGUI()
    {
        GUILayout.Label("Jewel Color Sort - Level Helper", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        _targetLevel = (LevelData)EditorGUILayout.ObjectField("Target LevelData", _targetLevel, typeof(LevelData), false);
        _levelManager = (LevelManager)EditorGUILayout.ObjectField("Level Manager (Scene)", _levelManager, typeof(LevelManager), true);

        if (_levelManager == null)
        {
            _levelManager = FindObjectOfType<LevelManager>();
        }

        EditorGUILayout.Space();

        if (_targetLevel != null)
        {
            EditorGUILayout.HelpBox($"Level: {_targetLevel.name}\n" +
                                    $"Width: {_targetLevel.Width}, Height: {_targetLevel.Height}\n" +
                                    $"Background Cells: {_targetLevel.Background.Count}\n" +
                                    $"Bead Cells: {_targetLevel.Beads.Count}\n" +
                                    $"Tray Slots: {_targetLevel.TraySlotCount}", MessageType.Info);

            if (GUILayout.Button("Validate Level Data", GUILayout.Height(30)))
            {
                bool valid = _targetLevel.Validate();
                if (valid)
                {
                    EditorUtility.DisplayDialog("Validation", $"Level '{_targetLevel.name}' hợp lệ 100%!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Validation Error", "Level có lỗi, xem chi tiết trong Console!", "OK");
                }
            }

            if (_levelManager != null && GUILayout.Button("Set As First Level in LevelManager", GUILayout.Height(30)))
            {
                Undo.RecordObject(_levelManager, "Set Level");
                var serializedObj = new SerializedObject(_levelManager);
                var levelsProp = serializedObj.FindProperty("levels");
                if (levelsProp.arraySize == 0)
                {
                    levelsProp.InsertArrayElementAtIndex(0);
                }
                levelsProp.GetArrayElementAtIndex(0).objectReferenceValue = _targetLevel;
                serializedObj.ApplyModifiedProperties();
                EditorSceneManager.MarkSceneDirty(_levelManager.gameObject.scene);

                EditorUtility.DisplayDialog("Success", $"Đã gán '{_targetLevel.name}' vào LevelManager!", "OK");
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Chọn một file LevelData trong 'Assets/Data/' để kiểm tra hoặc gán vào Scene.", MessageType.None);
        }
    }
}
#endif
