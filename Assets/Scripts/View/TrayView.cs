using System.Collections.Generic;
using UnityEngine;

public class TrayView : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] private TraySlotView slotPrefab;
    [SerializeField] private RectTransform slotContainer; // nên gắn HorizontalLayoutGroup để tự xếp hàng
    [SerializeField] private ColorPalette palette;

    private readonly List<TraySlotView> _slots = new List<TraySlotView>();

    public int SlotCount => _slots.Count;

    // Phát ra chỉ số ô khay mà người chơi vừa bấm
    public event System.Action<int> SlotClicked;


    public void Build(int slotCount)
    {
        Clear();

        for (int i = 0; i < slotCount; i++)
        {
            TraySlotView slot = Instantiate(slotPrefab, slotContainer);
            slot.Init(i);
            slot.Clicked += HandleSlotClicked;
            _slots.Add(slot);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] == null) continue;
            _slots[i].Clicked -= HandleSlotClicked;
            Destroy(_slots[i].gameObject);
        }
        _slots.Clear();
    }

    private void OnDestroy() => Clear();

    // Hiển thị nhóm hạt (màu + số lượng) trong 1 ô. type = None hoặc count &lt;= 0 thì ô trống
    public void SetSlot(int index, Type type)
    {
        if (!IsValid(index)) return;

        if (type == Type.None)
            _slots[index].SetEmpty();
        else
            _slots[index].SetContent(palette.BeadSprite, palette.GetColor(type), 1);
    }

    public void SetSelected(int index, bool selected)
    {
        if (IsValid(index)) _slots[index].SetSelected(selected);
    }

    public void ClearSelection()
    {
        for (int i = 0; i < _slots.Count; i++) _slots[i].SetSelected(false);
    }

    public void SetLocked(int index, bool locked)
    {
        if (IsValid(index)) _slots[index].SetLocked(locked);
    }

    // ---------- Truy vấn (dùng cho BeadFlyAnimator sau này) ----------

    // RectTransform của 1 ô khay, để biết hạt cần bay tới đâu
    public RectTransform GetSlotRect(int index) => IsValid(index) ? (RectTransform)_slots[index].transform : null;


    private bool IsValid(int index) => index >= 0 && index < _slots.Count;

    private void HandleSlotClicked(int index) => SlotClicked?.Invoke(index);
}