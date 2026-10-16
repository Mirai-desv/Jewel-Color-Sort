public enum EndState { None, Win, Lose }

/// <summary>
/// Kiểm tra thắng / thua (thuần C#).
/// - Thắng: mọi ô nền đều có hạt đúng màu.
/// - Thua: không còn nước đi hợp lệ nào.
///
/// Nước đi hợp lệ tồn tại khi:
///  (a) khay còn ô trống và trên lưới còn hạt chưa khoá (đưa hạt vào khay), hoặc
///  (b) có hạt (trên lưới chưa khoá, hoặc trong khay) mà lưới đang có ô rỗng với màu nền trùng màu hạt đó.
/// </summary>
public class WinLoseChecker
{
    // Đánh dấu "màu nền X đang có ô rỗng", tra cứu theo giá trị enum
    private readonly bool[] _emptyBackground = new bool[MaxTypeValue() + 1];

    public EndState Evaluate(GridModel grid, TrayModel tray)
    {
        if (grid.IsSolved) return EndState.Win;
        return HasAnyMove(grid, tray) ? EndState.None : EndState.Lose;
    }

    public bool HasAnyMove(GridModel grid, TrayModel tray)
    {
        System.Array.Clear(_emptyBackground, 0, _emptyBackground.Length);

        int size = grid.Size;
        for (int i = 0; i < size; i++)
        {
            if (grid.IsEmpty(i)) _emptyBackground[(int)grid.GetBackground(i)] = true;
        }

        bool trayHasRoom = tray.CountEmpty() > 0;

        for (int i = 0; i < size; i++)
        {
            if (!grid.HasBead(i) || grid.IsLocked(i)) continue;

            if (trayHasRoom) return true;                         // (a)
            if (_emptyBackground[(int)grid.GetBead(i)]) return true; // (b) lưới -> lưới
        }

        for (int s = 0; s < tray.SlotCount; s++)
        {
            if (tray.IsEmpty(s)) continue;
            if (_emptyBackground[(int)tray.GetSlotType(s)]) return true; // (b) khay -> lưới
        }

        return false;
    }

    private static int MaxTypeValue()
    {
        int max = 0;
        foreach (object value in System.Enum.GetValues(typeof(Type)))
            max = System.Math.Max(max, System.Convert.ToInt32(value));
        return max;
    }
}
