using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hiển thị cả khay chứa (nằm trong Canvas, đáy màn hình). Mỗi ô khay chứa đúng 1 hạt.
/// Không giữ dữ liệu game: Logic (TrayModel) đổi gì thì gọi SetSlot để cập nhật hiển thị.
/// </summary>
public class TrayView : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] private TraySlotView slotPrefab;
    [SerializeField] private RectTransform slotContainer; // nên gắn HorizontalLayoutGroup
    [SerializeField] private ColorPalette palette;

    private readonly List<TraySlotView> _slots = new List<TraySlotView>();
    private Canvas _canvas;

    public int SlotCount => _slots.Count;

    /// <summary>Phát ra chỉ số ô khay mà người chơi vừa bấm.</summary>
    public event System.Action<int> SlotClicked;

    // ---------- Dựng / dọn ----------

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

    // ---------- Cập nhật khi Logic báo thay đổi ----------

    /// <summary>Hiển thị hạt trong 1 ô. type = None thì ô trống.</summary>
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

    // ---------- Truy vấn (dùng cho animation) ----------

    public RectTransform GetSlotRect(int index) =>
        IsValid(index) ? (RectTransform)_slots[index].transform : null;

    /// <summary>
    /// Vị trí THẾ GIỚI (z = 0) tương ứng với tâm ô khay trên màn hình,
    /// để hạt (SpriteRenderer, world space) bay tới / bay ra từ đúng ô khay (UI).
    /// </summary>
    public Vector3 GetSlotWorldPosition(int index, Camera worldCamera)
    {
        RectTransform rect = GetSlotRect(index);
        if (rect == null || worldCamera == null) return Vector3.zero;

        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();

        Camera uiCamera = null;
        if (_canvas != null && _canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = _canvas.rootCanvas.worldCamera;

        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, rect.position);
        return worldCamera.ScreenToWorldPoint(
            new Vector3(screen.x, screen.y, -worldCamera.transform.position.z));
    }

    // ---------- Nội bộ ----------

    private bool IsValid(int index) => index >= 0 && index < _slots.Count;

    private void HandleSlotClicked(int index) => SlotClicked?.Invoke(index);
}
