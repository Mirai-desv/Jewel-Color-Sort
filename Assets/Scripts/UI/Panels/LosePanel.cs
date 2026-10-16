using TMPro;
using UnityEngine;

// bug, kệ nó
public class LosePanel : BasePanel
{
    [SerializeField] private TMP_Text messageText;

    protected override void OnShow()
    {
        if (messageText != null) messageText.SetText("No moves left!");
    }
}
