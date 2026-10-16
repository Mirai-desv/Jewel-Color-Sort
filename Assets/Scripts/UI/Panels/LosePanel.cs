using TMPro;
using UnityEngine;

/// <summary>Bảng thua (hết nước đi). Nên đặt kèm UndoButton và RestartButton.</summary>
public class LosePanel : BasePanel
{
    [SerializeField] private TMP_Text messageText;

    protected override void OnShow()
    {
        if (messageText != null) messageText.SetText("No moves left!");
    }
}
