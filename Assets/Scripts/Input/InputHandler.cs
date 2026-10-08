using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private GridView gridView;
    [SerializeField] private Camera worldCamera; // để trống thì dùng Camera.main

    /// <summary>Phát ra toạ độ ô (x, y) mà người chơi vừa click.</summary>
    public event System.Action<int, int> CellClicked;

    private bool _pressing;
    private bool _blockedByUI;
    private Vector2 _startPos;

    private void Awake()
    {
        if (worldCamera == null) worldCamera = Camera.main;
    }

    private void Update()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null) return;

        bool isPressed = pointer.press.isPressed;
        Vector2 pos = pointer.position.ReadValue();

        if (isPressed && !_pressing)
        {
            // Vừa nhấn xuống trong khung hình này
            _pressing = true;
            _startPos = pos;
            _blockedByUI = IsPointerOverUI(pointer);
        }
        else if (!isPressed && _pressing)
        {
            // Vừa nhả ra trong khung hình này
            _pressing = false;
            TryClick(pos);
        }
    }

    private void TryClick(Vector2 releasePos)
    {
        Debug.Log($"[InputHandler] Nhả tại {releasePos}, blockedByUI={_blockedByUI}"); // TẠM THỜI
        if (_blockedByUI) return;

        float maxMove = Constants.ClickMaxMovePixels;
        if ((releasePos - _startPos).sqrMagnitude > maxMove * maxMove) return; // là kéo, không phải click

        if (worldCamera == null) return;

        Vector3 world = worldCamera.ScreenToWorldPoint(
            new Vector3(releasePos.x, releasePos.y, -worldCamera.transform.position.z));

        bool inBounds = gridView.TryWorldToCell(world, out int x, out int y);
        Debug.Log($"[InputHandler] world={world}, cell=({x},{y}), inBounds={inBounds}"); // TẠM THỜI

        if (inBounds) CellClicked?.Invoke(x, y);
        /*
        if (gridView.TryWorldToCell(world, out int x, out int y))
            CellClicked?.Invoke(x, y);
        */
    }

    private static bool IsPointerOverUI(Pointer pointer)
    {
        EventSystem es = EventSystem.current;
        if (es == null) return false;

        // Gọi trong Update() nên an toàn, không bị cảnh báo "query state from last frame"
        if (pointer is Touchscreen touch)
            return es.IsPointerOverGameObject(touch.primaryTouch.touchId.ReadValue());

        return es.IsPointerOverGameObject();
    }
}