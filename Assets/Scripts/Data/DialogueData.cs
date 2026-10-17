using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    [SerializeField] private string speakerName;

    [SerializeField] private Sprite portrait;

    [TextArea(2, 6)]
    [SerializeField] private string text;

    public string SpeakerName => speakerName;
    public Sprite Portrait => portrait;
    public string Text => text;
}

[CreateAssetMenu(
    fileName = "DialogueData",
    menuName = "Scriptable Objects/DialogueData"
)]
public class DialogueData : ScriptableObject
{
    [SerializeField] private List<DialogueLine> lines = new();

    public IReadOnlyList<DialogueLine> Lines => lines;

    public bool HasDialogue => lines != null && lines.Count > 0;
}