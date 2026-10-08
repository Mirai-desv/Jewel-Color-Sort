using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TraySlotView : MonoBehaviour, IPointerClickHandler
{
    [Header("Tham chiếu UI")]
    [SerializeField] private Image backgroundImage;   // nền ô, cũng là vùng nhận click
    [SerializeField] private Image beadIcon;          // icon hạt, đổi màu theo nhóm
    [SerializeField] private TMP_Text countText;      // số lượng hạt trong ô
    [SerializeField] private GameObject selectedMarker; // viền/hiệu ứng khi đang chọn (tuỳ chọn)
    [SerializeField] private GameObject lockIcon;       // icon khoá (tuỳ chọn)

    public int Index { get; private set; }
    public bool IsSelected { get; private set; }
    public bool IsLocked { get; private set; }

    // Phát ra Index của ô khi người chơi bấm vào
    public event System.Action<int> Clicked;

    public void Init(int index)
    {
        Index = index;
        SetEmpty();
        SetSelected(false);
        SetLocked(false);
    }

    // Hiển thị 1 nhóm hạt cùng màu
    public void SetContent(Sprite sprite, Color color, int count)
    {
        if (count <= 0)
        {
            SetEmpty();
            return;
        }

        beadIcon.enabled = true;
        beadIcon.sprite = sprite;
        beadIcon.color = color;

        countText.gameObject.SetActive(true);
        countText.SetText("{0}", count); // không cấp phát chuỗi mới
    }

    public void SetEmpty()
    {
        beadIcon.enabled = false;
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        if (selectedMarker != null) selectedMarker.SetActive(selected);
    }

    public void SetLocked(bool locked)
    {
        IsLocked = locked;
        if (lockIcon != null) lockIcon.SetActive(locked);
    }

    // Ô khoá vẫn báo click, để tầng trên quyết định (vd hiện gợi ý mở khoá)
    public void OnPointerClick(PointerEventData eventData) => Clicked?.Invoke(Index);
}