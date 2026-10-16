using UnityEngine;

/// <summary>
/// Nhận cú chạm (Tapped) từ GestureDetector, đổi sang ô (x, y) trên lưới
/// rồi phát sự kiện CellClicked. Không biết luật chơi.
/// </summary>
public class InputHandler : MonoBehaviour
{
    [SerializeField] private GridView gridView;
    [SerializeField] private GestureDetector gesture; // để trống thì tự tìm trong scene
    [SerializeField] private Camera worldCamera;      // để trống thì dùng Camera.main

    public event System.Action<int, int> CellClicked;

    private void Awake()
    {
        if (worldCamera == null) worldCamera = Camera.main;
        if (gesture == null) gesture = FindFirstObjectByType<GestureDetector>();
    }

    private void OnEnable()
    {
        if (gesture != null) gesture.Tapped += OnTapped;
    }

    private void OnDisable()
    {
        if (gesture != null) gesture.Tapped -= OnTapped;
    }

    private void OnTapped(Vector2 screenPos)
    {
        if (worldCamera == null) return;

        Vector3 world = worldCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, -worldCamera.transform.position.z));

        if (gridView.TryWorldToCell(world, out int x, out int y))
            CellClicked?.Invoke(x, y);
    }
}
