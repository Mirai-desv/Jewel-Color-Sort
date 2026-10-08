public class TrayModel
{
    private Type[] _slots = new Type[0];

    public int SlotCount { get; private set; }

    public void Init(int slotCount)
    {
        SlotCount = slotCount;
        _slots = new Type[slotCount];
    }

    public bool IsValid(int slot) => slot >= 0 && slot < SlotCount;
    public bool IsEmpty(int slot) => _slots[slot] == Type.None;
    public Type GetSlotType(int slot) => _slots[slot];

    // Đặt 1 hạt vào ô (ô phải đang trống, không tự kiểm tra ở đây)
    public void Set(int slot, Type type) => _slots[slot] = type;

    // Dọn trống 1 ô
    public void Clear(int slot) => _slots[slot] = Type.None;

    /// <summary>Đếm số ô đang trống.</summary>
    public int CountEmpty()
    {
        int n = 0;
        for (int i = 0; i < _slots.Length; i++)
            if (_slots[i] == Type.None) n++;
        return n;
    }
}