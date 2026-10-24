using TMPro;
using UnityEngine;

public class MainMenuPanel : BasePanel
{
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private DebugLevelSelector debugLevelSelector;

    protected override void Awake()
    {
        base.Awake();
        if (debugLevelSelector == null) debugLevelSelector = GetComponent<DebugLevelSelector>();
    }

    protected override void OnShow()
    {
        if (levelText != null) levelText.SetText("Level {0}", SaveManager.CurrentLevelIndex + 1);
        if (debugLevelSelector != null) debugLevelSelector.ApplyVisibility();
    }
}
