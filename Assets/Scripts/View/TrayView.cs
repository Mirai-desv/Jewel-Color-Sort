using System.Collections.Generic;
using UnityEngine;

public class TrayView : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] private TraySlotView slotPrefab;
    [SerializeField] private RectTransform slotContainer; // nên gắn HorizontalLayoutGroup
    [SerializeField] private ColorPalette palette;
    [SerializeField] private UnityEngine.UI.Image trayBackground;
    [SerializeField] private Sprite backgroundSprite;

    private readonly List<TraySlotView> _slots = new List<TraySlotView>();
    private Canvas _canvas;
    private bool _layoutPending;

    public int SlotCount => _slots.Count;

    public event System.Action<int> SlotClicked;

    private void OnEnable()
    {
        if(!_layoutPending) return;
        _layoutPending = false;
        ApplyLayout();
        StartCoroutine(RefreshLayoutNextFrame());
    }

    private void RequestLayoutRefresh()
    {
        if(isActiveAndEnabled) StartCoroutine(RefreshLayoutNextFrame());
        else _layoutPending = true; // chạy khi khay được bật
    }

    private void Awake()
    {
        // Đảm bảo khay luôn hiển thị trên ảnh màn chơi và hạt trên bảng (sortingOrder 200 > 10)
        Canvas c = GetComponent<Canvas>();
        if (c == null) c = gameObject.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 200;
        if (GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Tìm khung gỗ header của khay (Level Image) để cùng đưa lên sortingOrder 200
        Transform parent = transform.parent;
        if (parent != null)
        {
            Transform header = parent.Find("Level Image");
            if (header != null)
            {
                Canvas hc = header.GetComponent<Canvas>();
                if (hc == null) hc = header.gameObject.AddComponent<Canvas>();
                hc.overrideSorting = true;
                hc.sortingOrder = 200;
            }
        }

        SetupBackground();
    }

    public RectTransform BackgroundRect => trayBackground != null ? trayBackground.rectTransform : null;

    private int _lastScreenWidth;
    private int _lastScreenHeight;
    private Rect _lastSafeArea;

    private void Update()
    {
        if (_slots.Count == 0) return;
        if (Screen.width != _lastScreenWidth ||
            Screen.height != _lastScreenHeight ||
            Screen.safeArea != _lastSafeArea)
        {
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            _lastSafeArea = Screen.safeArea;
            ApplyLayout();
        }
    }

    // ---------- Dựng / dọn ----------

    public void Build(int slotCount)
    {
        Clear();
        SetupBackground();

        for (int i = 0; i < slotCount; i++)
        {
            TraySlotView slot = Instantiate(slotPrefab, slotContainer);
            slot.Init(i);
            slot.Clicked += HandleSlotClicked;
            _slots.Add(slot);
        }

        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
        _lastSafeArea = Screen.safeArea;

        ApplyLayout();
        RequestLayoutRefresh();
    }

    public void Clear()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] == null) continue;
            _slots[i].Clicked -= HandleSlotClicked;
        }
        _slots.Clear();

        if (slotContainer != null)
        {
            for (int i = slotContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = slotContainer.GetChild(i);
                child.SetParent(null);
                Destroy(child.gameObject);
            }
        }

        if (trayBackground != null) trayBackground.gameObject.SetActive(false);
    }

    private void OnDestroy() => Clear();

    private void SetupBackground()
    {
        if (trayBackground == null)
        {
            Transform existing = transform.Find("TrayBackground");
            if (existing != null)
            {
                trayBackground = existing.GetComponent<UnityEngine.UI.Image>();
            }
            else
            {
                GameObject bgGo = new GameObject("TrayBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
                bgGo.transform.SetParent(transform, false);
                bgGo.transform.SetSiblingIndex(0);
                trayBackground = bgGo.GetComponent<UnityEngine.UI.Image>();
            }
        }

        if (trayBackground != null)
        {
            trayBackground.raycastTarget = false;
            if (backgroundSprite != null)
            {
                trayBackground.sprite = backgroundSprite;
            }
            else if (trayBackground.sprite == null)
            {
                trayBackground.sprite = GetOrCreateBackgroundSprite();
            }
            trayBackground.type = UnityEngine.UI.Image.Type.Sliced;
            trayBackground.color = Color.white;
            trayBackground.transform.SetSiblingIndex(0);
        }
    }

    public void ApplyLayout()
    {
        if (slotContainer == null || _slots.Count == 0) return;

        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
        Canvas rootCanvas = _canvas != null ? _canvas.rootCanvas : null;
        Camera uiCamera = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? rootCanvas.worldCamera
            : null;

        Rect safe = Screen.safeArea;
        if (safe.width <= 0 || safe.height <= 0)
            safe = new Rect(0, 0, Screen.width, Screen.height);

        RectTransform canvasRt = rootCanvas != null ? rootCanvas.GetComponent<RectTransform>() : null;
        Transform parentTransform = slotContainer.parent != null ? slotContainer.parent : transform;

        Vector2 screenBottomCenter = new Vector2(safe.x + safe.width * 0.5f, safe.y);
        Vector2 screenLeft = new Vector2(safe.x, safe.y + safe.height * 0.5f);
        Vector2 screenRight = new Vector2(safe.x + safe.width, safe.y + safe.height * 0.5f);
        Vector2 screenTop = new Vector2(safe.x + safe.width * 0.5f, safe.y + safe.height);

        Vector3 worldBottom, worldLeft, worldRight, worldTop;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRt, screenBottomCenter, uiCamera, out worldBottom);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRt, screenLeft, uiCamera, out worldLeft);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRt, screenRight, uiCamera, out worldRight);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRt, screenTop, uiCamera, out worldTop);

        Vector3 localBottom = parentTransform.InverseTransformPoint(worldBottom);
        Vector3 localLeft = parentTransform.InverseTransformPoint(worldLeft);
        Vector3 localRight = parentTransform.InverseTransformPoint(worldRight);
        Vector3 localTop = parentTransform.InverseTransformPoint(worldTop);

        float safeWidth = Mathf.Abs(localRight.x - localLeft.x);
        float safeHeight = Mathf.Abs(localTop.y - localBottom.y);
        if (safeWidth <= 10f) safeWidth = 1080f;
        if (safeHeight <= 10f) safeHeight = 1920f;

        bool isLandscape = safeWidth > safeHeight;
        int count = _slots.Count;

        // 1. Xác định số cột tối ưu dựa trên số lượng ô
        int cols;
        if (count <= 7)
            cols = count;
        else if (count <= 14)
            cols = Mathf.CeilToInt(count / 2f);
        else if (count <= 26)
            cols = Mathf.CeilToInt(count / 3f);
        else
            cols = isLandscape ? Mathf.CeilToInt(count / 3f) : Mathf.CeilToInt(count / 4f);

        cols = Mathf.Clamp(cols, 1, count);
        int rows = Mathf.CeilToInt((float)count / cols);

        // 2. Tính bề rộng khả dụng cho khay (thụt vào một chút để có lề 2 bên)
        float maxTrayWidth = isLandscape
            ? Mathf.Min(safeWidth * 0.72f, 1100f)
            : Mathf.Min(safeWidth * 0.94f, 1080f);

        float paddingX = 35f;
        float topPadding = 50f;
        float bottomPadding = 40f;
        float bottomMargin = 20f;

        float availableSlotsWidth = maxTrayWidth - paddingX * 2f;
        float spacingRatio = 0.25f;

        // Bề rộng mỗi ô
        float slotSize = availableSlotsWidth / (cols + (cols - 1) * spacingRatio);
        slotSize = Mathf.Clamp(slotSize, 36f, 90f);
        float spacing = Mathf.Round(slotSize * spacingRatio);

        // 3. Giới hạn chiều cao khay không chiếm quá nhiều màn hình (tối đa ~24% dọc, 30% ngang)
        float maxTrayHeight = safeHeight * (isLandscape ? 0.30f : 0.24f);
        float maxSlotsHeight = maxTrayHeight - topPadding - bottomPadding;
        float neededSlotsHeight = rows * slotSize + (rows - 1) * spacing;

        if (neededSlotsHeight > maxSlotsHeight && neededSlotsHeight > 0f)
        {
            float scaleH = maxSlotsHeight / neededSlotsHeight;
            slotSize = Mathf.Max(32f, slotSize * scaleH);
            spacing = Mathf.Round(slotSize * spacingRatio);
        }

        float gridWidth = cols * slotSize + (cols - 1) * spacing;
        float gridHeight = rows * slotSize + (rows - 1) * spacing;

        // 4. Cập nhật GridLayoutGroup
        UnityEngine.UI.GridLayoutGroup grid = slotContainer.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if (grid != null)
        {
            grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = cols;
            grid.cellSize = new Vector2(slotSize, slotSize);
            grid.spacing = new Vector2(spacing, spacing);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.padding = new RectOffset(0, 0, 0, 0);
        }

        // 5. Cập nhật slotContainer và neo chính xác ở đáy màn hình (safe area)
        slotContainer.anchorMin = new Vector2(0.5f, 0.5f);
        slotContainer.anchorMax = new Vector2(0.5f, 0.5f);
        slotContainer.pivot = new Vector2(0.5f, 0f);
        slotContainer.sizeDelta = new Vector2(gridWidth, gridHeight);

        float slotZ = slotContainer.localPosition.z;
        float slotContainerY = localBottom.y + bottomMargin + bottomPadding;
        slotContainer.localPosition = new Vector3(localBottom.x, slotContainerY, slotZ);

        // 6. Cập nhật icon bên trong từng ô
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] != null) _slots[i].SetSize(slotSize);
        }

        // 7. Cập nhật TrayBackground ôm trọn khay với padding đầy đủ
        if (trayBackground != null)
        {
            RectTransform bgRt = trayBackground.rectTransform;
            bgRt.anchorMin = new Vector2(0.5f, 0.5f);
            bgRt.anchorMax = new Vector2(0.5f, 0.5f);
            bgRt.pivot = new Vector2(0.5f, 0.5f);

            float bgWidth = gridWidth + paddingX * 2f;
            float bgHeight = gridHeight + topPadding + bottomPadding;
            float bgCenterY = slotContainerY + gridHeight * 0.5f + (topPadding - bottomPadding) * 0.5f;
            float bgZ = bgRt.localPosition.z;

            bgRt.sizeDelta = new Vector2(bgWidth, bgHeight);
            bgRt.localPosition = new Vector3(localBottom.x, bgCenterY, bgZ);
            trayBackground.gameObject.SetActive(true);
        }
    }

    private System.Collections.IEnumerator RefreshLayoutNextFrame()
    {
        yield return null;
        ApplyLayout();
    }

    private static Sprite _cachedProceduralSprite;

    private static Sprite GetOrCreateBackgroundSprite()
    {
        if (_cachedProceduralSprite != null) return _cachedProceduralSprite;

        int size = 128;
        int radius = 32;
        int pad = 4;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "ProceduralTrayBg",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color32 fill = new Color32(250, 243, 233, 248);       // Warm cream
        Color32 border = new Color32(185, 155, 125, 255);     // Wooden rim
        Color32 innerBorder = new Color32(238, 224, 205, 255);
        Color32 shadow = new Color32(120, 80, 50, 70);        // Drop shadow
        Color32 clear = new Color32(0, 0, 0, 0);

        Color32[] pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(0, Mathf.Abs(x - (size - 1) * 0.5f) - ((size - 1) * 0.5f - radius - pad));
                float dy = Mathf.Max(0, Mathf.Abs(y - (size - 1) * 0.5f) - ((size - 1) * 0.5f - radius - pad));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > radius)
                {
                    float sdy = Mathf.Max(0, Mathf.Abs(y - 3 - (size - 1) * 0.5f) - ((size - 1) * 0.5f - radius - pad));
                    float sdist = Mathf.Sqrt(dx * dx + sdy * sdy);
                    if (sdist <= radius + 4f)
                    {
                        float alpha = Mathf.Clamp01((radius + 4f - sdist) / 4f) * 0.28f;
                        pixels[y * size + x] = new Color32(shadow.r, shadow.g, shadow.b, (byte)(alpha * 255));
                    }
                    else
                    {
                        pixels[y * size + x] = clear;
                    }
                }
                else if (dist >= radius - 3f)
                {
                    pixels[y * size + x] = border;
                }
                else if (dist >= radius - 5f)
                {
                    pixels[y * size + x] = innerBorder;
                }
                else
                {
                    pixels[y * size + x] = fill;
                }
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, true);

        _cachedProceduralSprite = Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(44, 44, 44, 44)
        );

        return _cachedProceduralSprite;
    }


    // Hiển thị hạt trong 1 ô. type = None thì ô trống
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

    private bool IsValid(int index) => index >= 0 && index < _slots.Count;

    private void HandleSlotClicked(int index) => SlotClicked?.Invoke(index);
}
