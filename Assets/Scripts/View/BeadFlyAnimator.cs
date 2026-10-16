using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bộ cập nhật TẬP TRUNG cho mọi hạt đang bay (không dùng coroutine/tween riêng từng hạt).
/// Logic đã cập nhật xong trước; animator chỉ lo phần nhìn: hạt trượt từ vị trí đầu tới vị trí đích.
/// - FlyOnGrid:   lưới -> lưới (hạt ở lại lưới sau khi tới nơi)
/// - FlyToTray:   lưới -> khay (tới nơi thì trả hạt về pool, rồi báo ArrivedAtTray để TrayView hiện icon)
/// - FlyFromTray: khay -> lưới (hạt xuất phát nhỏ rồi phóng to dần)
/// </summary>
public class BeadFlyAnimator : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private BeadPool pool;

    /// <summary>Phát ra khi 1 hạt bay vào ô khay xong (tham số: index ô khay).</summary>
    public event System.Action<int> ArrivedAtTray;

    /// <summary>Còn hạt đang bay (hoặc đang chờ tới lượt xuất phát).</summary>
    public bool IsBusy => _flights.Count > 0;

    private struct Flight
    {
        public BeadView Bead;
        public Vector3 From;
        public Vector3 To;
        public Vector3 BaseScale;
        public float ScaleFrom;
        public float ScaleTo;
        public float Delay;
        public float Elapsed;
        public int RestOrder;
        public int TraySlot; // >= 0: bay vào khay rồi trả về pool; -1: ở lại lưới
    }

    private readonly List<Flight> _flights = new List<Flight>(256);

    private float Duration => config != null ? config.FlyDuration : 0.3f;
    private float TrayScale => config != null ? config.TrayFlyScale : 0.7f;
    private int FlyOrder => config != null ? config.FlySortingOrder : 100;

    public void FlyOnGrid(BeadView bead, Vector3 to, float delay, int restOrder) =>
        Add(bead, to, delay, 1f, 1f, restOrder, -1);

    public void FlyFromTray(BeadView bead, Vector3 to, float delay, int restOrder) =>
        Add(bead, to, delay, TrayScale, 1f, restOrder, -1);

    public void FlyToTray(BeadView bead, Vector3 to, float delay, int slot) =>
        Add(bead, to, delay, 1f, TrayScale, 0, slot);

    /// <summary>Huỷ mọi chuyến bay (khi nạp level mới / restart), trả hạt về pool.</summary>
    public void CancelAll()
    {
        for (int i = 0; i < _flights.Count; i++)
        {
            if (_flights[i].Bead != null) pool.Release(_flights[i].Bead);
        }
        _flights.Clear();
    }

    private void Add(BeadView bead, Vector3 to, float delay, float scaleFrom, float scaleTo, int restOrder, int slot)
    {
        if (bead == null) return;

        Transform t = bead.transform;
        var flight = new Flight
        {
            Bead = bead,
            From = t.position,
            To = to,
            BaseScale = t.localScale,
            ScaleFrom = scaleFrom,
            ScaleTo = scaleTo,
            Delay = Mathf.Max(0f, delay),
            Elapsed = 0f,
            RestOrder = restOrder,
            TraySlot = slot
        };

        bead.SetSortingOrder(FlyOrder);          // hạt đang bay nằm trên cùng
        t.localScale = flight.BaseScale * scaleFrom;
        _flights.Add(flight);
    }

    private void Update()
    {
        if (_flights.Count == 0) return;

        float dt = Time.deltaTime;
        float duration = Duration;

        // Duyệt ngược để xoá phần tử đang duyệt bằng cách đảo với phần tử cuối
        for (int i = _flights.Count - 1; i >= 0; i--)
        {
            Flight f = _flights[i];

            if (f.Bead == null)
            {
                RemoveAt(i);
                continue;
            }

            if (f.Delay > 0f)
            {
                f.Delay -= dt;
                _flights[i] = f;
                continue;
            }

            f.Elapsed += dt;
            float t = Mathf.Clamp01(f.Elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic

            Transform tr = f.Bead.transform;
            tr.position = Vector3.LerpUnclamped(f.From, f.To, eased);
            tr.localScale = f.BaseScale * Mathf.Lerp(f.ScaleFrom, f.ScaleTo, eased);

            if (t >= 1f)
            {
                Finish(f);
                RemoveAt(i);
            }
            else
            {
                _flights[i] = f;
            }
        }
    }

    private void Finish(Flight f)
    {
        if (f.TraySlot >= 0)
        {
            pool.Release(f.Bead);
            ArrivedAtTray?.Invoke(f.TraySlot);
        }
        else
        {
            Transform tr = f.Bead.transform;
            tr.position = f.To;
            tr.localScale = f.BaseScale;
            f.Bead.SetSortingOrder(f.RestOrder);
        }
    }

    private void RemoveAt(int index)
    {
        int last = _flights.Count - 1;
        _flights[index] = _flights[last];
        _flights.RemoveAt(last);
    }
}
