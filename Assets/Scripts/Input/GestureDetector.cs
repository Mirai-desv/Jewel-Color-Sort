using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class GestureDetector : MonoBehaviour
{
    public event System.Action<Vector2> Tapped;
    public event System.Action<Vector2> Dragged;
    public event System.Action<float, Vector2> Pinched; // (hệ số zoom, vị trí giữa 2 ngón trên màn hình)

    private bool _active;
    private bool _blockedByUI;
    private bool _dragging;
    private bool _consumed; // đã pinch trong lần chạm này
    private Vector2 _startPos;
    private Vector2 _lastPos;
    private float _lastPinchDistance;
    private Vector2 _lastPinchMid;

    private void Update()
    {
        int count = ReadPointers(out Vector2 p0, out Vector2 p1, out int touchId);

        if (count == 0)
        {
            if (_active) EndPress();
            else CheckInstantMouseClick();
            return;
        }

        if (!_active) BeginPress(p0, touchId);
        if (_blockedByUI) return;

        if (count >= 2)
        {
            HandlePinch(p0, p1);
            return;
        }

        _lastPinchDistance = 0f;
        if (_consumed)
        {
            _lastPos = p0;
            return;
        }

        HandleSinglePointer(p0);
    }

    private void BeginPress(Vector2 pos, int touchId)
    {
        _active = true;
        _dragging = false;
        _consumed = false;
        _startPos = pos;
        _lastPos = pos;
        _lastPinchDistance = 0f;
        _blockedByUI = IsPointerOverUI(touchId);
    }

    private void HandleSinglePointer(Vector2 pos)
    {
        float threshold = Constants.ClickMaxMovePixels;

        if (!_dragging)
        {
            if ((pos - _startPos).sqrMagnitude > threshold * threshold)
            {
                _dragging = true;
                Dragged?.Invoke(pos - _startPos); // bù phần đã đi để không bị giật
            }
        }
        else
        {
            Dragged?.Invoke(pos - _lastPos);
        }

        _lastPos = pos;
    }

    private void HandlePinch(Vector2 p0, Vector2 p1)
    {
        float distance = Vector2.Distance(p0, p1);
        Vector2 mid = (p0 + p1) * 0.5f;

        if (_lastPinchDistance > 1f && distance > 1f)
        {
            Dragged?.Invoke(mid - _lastPinchMid);                  // 2 ngón trượt đi: kéo ảnh
            Pinched?.Invoke(distance / _lastPinchDistance, mid);   // 2 ngón xa/gần nhau: zoom quanh điểm giữa
        }

        _lastPinchDistance = distance;
        _lastPinchMid = mid;
        _consumed = true;
        _dragging = false;
    }

    private void EndPress()
    {
        bool isTap = !_blockedByUI && !_dragging && !_consumed;

        _active = false;
        _dragging = false;
        _consumed = false;
        _lastPinchDistance = 0f;

        if (isTap) Tapped?.Invoke(_lastPos);
    }

    // Trường hợp khung hình giật: nhấn và nhả chuột trong cùng 1 khung hình
    private void CheckInstantMouseClick()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;
        if (!mouse.leftButton.wasPressedThisFrame || !mouse.leftButton.wasReleasedThisFrame) return;
        if (IsPointerOverUI(-1)) return;

        Tapped?.Invoke(mouse.position.ReadValue());
    }

    // Trả về số con trỏ đang nhấn (cảm ứng ưu tiên, không có thì dùng chuột trái).
    // p0, p1 là vị trí 2 con trỏ đầu tiên, touchId = -1 nếu là chuột.
    private static int ReadPointers(out Vector2 p0, out Vector2 p1, out int touchId)
    {
        p0 = default;
        p1 = default;
        touchId = -1;

        Touchscreen touch = Touchscreen.current;
        if (touch != null)
        {
            int n = 0;
            var touches = touch.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                TouchControl t = touches[i];
                if (!t.press.isPressed) continue;

                Vector2 pos = t.position.ReadValue();
                if (n == 0)
                {
                    p0 = pos;
                    touchId = t.touchId.ReadValue();
                }
                else if (n == 1)
                {
                    p1 = pos;
                }
                n++;
            }
            if (n > 0) return n;
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed)
        {
            p0 = mouse.position.ReadValue();
            return 1;
        }

        return 0;
    }

    private static bool IsPointerOverUI(int touchId)
    {
        EventSystem es = EventSystem.current;
        if (es == null) return false;

        return touchId >= 0 ? es.IsPointerOverGameObject(touchId) : es.IsPointerOverGameObject();
    }
}