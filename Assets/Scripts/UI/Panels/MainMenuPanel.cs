using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : BasePanel
{
    [Header("Tham chiếu")]
    [SerializeField] private LevelManager levelManager;

    [Header("Hiển thị")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image levelPreviewImage;

    protected override void OnShow()
    {
        int max = levelManager != null ? Mathf.Max(0, levelManager.LevelCount - 1) : 0;
        int index = Mathf.Clamp(SaveManager.CurrentLevelIndex, 0, max);
        LevelData data = levelManager != null ? levelManager.GetLevel(index) : null;

        if (levelText != null) levelText.SetText("Level {0}", index + 1);

        if(levelPreviewImage != null)
        {
            bool hasImage = data != null && data.HasPreviewImage;
            levelPreviewImage.gameObject.SetActive(hasImage);
            if(hasImage) levelPreviewImage.sprite = data.PreviewImage;
        }
    }
}
