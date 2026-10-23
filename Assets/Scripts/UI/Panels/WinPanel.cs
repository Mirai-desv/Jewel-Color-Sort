using TMPro;
using UnityEngine;

public class WinPanel : BasePanel
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text nextButtonLabel;

    protected override void Awake()
    {
        base.Awake();
        Canvas c = GetComponent<Canvas>();
        if (c == null) c = gameObject.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 600;
        if (GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
    }

    /// <summary>GameManager gọi trước khi hiện panel.</summary>
    public void Setup(int levelNumber, int reward, bool hasNext)
    {
        if (titleText != null) titleText.SetText("Level {0} Complete!", levelNumber);
        if (rewardText != null) rewardText.SetText("+{0}", reward);
        if (nextButtonLabel != null) nextButtonLabel.SetText(hasNext ? "Next Level" : "Play Again");
    }
}
