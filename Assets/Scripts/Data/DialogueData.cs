using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    [SerializeField] private string speakerName;
    [SerializeField] private Sprite portrait;
    [Tooltip("Ảnh nền (tuỳ chọn). Để trống thì giữ nguyên nền của câu trước.")]
    [SerializeField] private Sprite background;
    [SerializeField] private bool darkenPortrait; 

    [TextArea(2, 6)]
    [SerializeField] private string text;

    public string SpeakerName => speakerName;
    public Sprite Portrait => portrait;
    public bool DarkenPortrait => darkenPortrait;  
    public Sprite Background => background;
    public string Text => text;
}

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private List<DialogueLine> lines = new();

    public IReadOnlyList<DialogueLine> Lines => lines;
    public bool HasDialogue => lines != null && lines.Count > 0;
}