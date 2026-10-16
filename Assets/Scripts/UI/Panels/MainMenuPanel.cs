using TMPro;
using UnityEngine;

/// <summary>Màn hình chính: hiển thị level sẽ chơi tiếp. Các nút (Start, Setting, Exit) là component riêng.</summary>
public class MainMenuPanel : BasePanel
{
    [SerializeField] private TMP_Text levelText;

    protected override void OnShow()
    {
        if (levelText != null) levelText.SetText("Level {0}", SaveManager.CurrentLevelIndex + 1);
    }
}
