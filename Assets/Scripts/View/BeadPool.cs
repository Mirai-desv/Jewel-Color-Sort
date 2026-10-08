using UnityEngine;
using System.Collections.Generic;

public class BeadPool : MonoBehaviour
{
    [SerializeField] private BeadView beadPrefab;
    [Tooltip("Số hạt tạo sẵn. Có gắng đặt cao lên")]
    [SerializeField] private int prewarmCount = 256;

    private readonly Stack<BeadView> _pool = new Stack<BeadView>();
    private void Awake() => Prewarm(prewarmCount);

    // Tạo sẵn cho đến khi kho có đủ số lượng
    public void Prewarm(int count)
    {
        while(_pool.Count < count)
        {
            BeadView bead = Create();
            bead.gameObject.SetActive(false);
            _pool.Push(bead);
        }
    }

    // Lấy 1 hạt ra dùng. Hết hạt trong kho thì tự tạo thêm
    public BeadView Get(Transform parent)
    {
        BeadView bead = _pool.Count > 0 ? _pool.Pop() : Create();
        bead.transform.SetParent(parent != null ? parent : transform, false);
        bead.gameObject.SetActive(true);
        return bead;
    }

    // Trả hạt về kho
    public void Release(BeadView bead)
    {
        if (bead == null) return;
        if (!bead.gameObject.activeSelf) return; // đã nằm trong kho rồi, tránh trả 2 lần
 
        bead.ResetState();
        bead.gameObject.SetActive(false);
        bead.transform.SetParent(transform, false);
        _pool.Push(bead);
    }
 
    private BeadView Create() => Instantiate(beadPrefab, transform);
}
