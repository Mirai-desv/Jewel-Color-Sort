using TMPro;
using UnityEngine;

public class MainMenuPanel : BasePanel
{
    [SerializeField] private TMP_Text levelText;

    protected override void OnShow()
    {
        if (levelText != null) levelText.SetText("Level {0}", SaveManager.CurrentLevelIndex + 1);
    }
}
