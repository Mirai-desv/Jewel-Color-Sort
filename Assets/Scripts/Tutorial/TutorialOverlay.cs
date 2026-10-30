using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Lớp phủ hướng dẫn: nền tối có "lỗ sáng", vòng tròn/bàn tay chỉ, hộp thoại và nút Bỏ qua.
/// Tự dựng toàn bộ UI lúc chạy nên không cần làm prefab.
///
/// Cách đặt: tạo 1 GameObject trống ở GỐC scene (không nằm dưới Canvas nào) rồi gắn script này.
/// Mọi thành phần đều tắt Raycast Target (trừ nút Bỏ qua), nên click vẫn đi xuyên xuống game;
/// việc chặn click sai do TutorialManager lo (qua LevelSession.CellClickFilter / SlotClickFilter).
/// Script này không biết luật game, chỉ nhận toạ độ MÀN HÌNH (pixel) từ TutorialManager.
[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
public class TutorialOverlay : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private int sortingOrder = 500;
    [SerializeField] private Vector2 referenceResolution = new Vector2(1080f, 1920f);

    [Header("Lớp tối")]
    [SerializeField] private Color dimColor = new Color(0f, 0f, 0f, 0.72f);
    [Tooltip("Nới vùng sáng ra mỗi phía bấy nhiêu (đơn vị canvas)")]
    [SerializeField, Min(0f)] private float holePadding = 12f;

    [Header("Vòng tròn / bàn tay chỉ")]
    [Tooltip("Để trống thì dùng vòng tròn nhấp nháy mặc định")]
    [SerializeField] private Sprite pointerSprite;
    [Tooltip("Điểm của hình sẽ đặt đúng vào ô cần chạm (đầu ngón tay thì để khoảng 0.3, 0.9)")]
    [SerializeField] private Vector2 pointerPivot = new Vector2(0.5f, 0.5f);
    [SerializeField, Min(16f)] private float pointerSize = 150f;
    [SerializeField] private Color pointerColor = Color.white;

    [Header("Hộp thoại")]
    [Tooltip("Để trống thì dùng hộp bo góc mặc định")]
    [SerializeField] private Sprite boxSprite;
    [SerializeField] private Color boxColor = new Color(1f, 0.97f, 0.9f, 1f);
    [SerializeField] private Color textColor = new Color(0.25f, 0.15f, 0.1f, 1f);
    [Tooltip("Nên gán font có tiếng Việt (vd DVN-Fredoka-Bold SDF). Để trống thì dùng font TMP mặc định, có thể mất dấu.")]
    [SerializeField] private TMP_FontAsset font;
    [SerializeField, Range(0.3f, 1f)] private float boxWidthFraction = 0.82f;
    [SerializeField, Min(80f)] private float boxHeight = 230f;
    [Tooltip("Tâm hộp cách mép trên/dưới màn hình bao nhiêu phần chiều cao")]
    [SerializeField, Range(0.05f, 0.45f)] private float boxMargin = 0.2f;

    [Header("Nút bỏ qua")]
    [SerializeField] private string skipLabel = "Bỏ qua";

    public event System.Action SkipClicked;

    private bool _built;
    private RectTransform _root;
    private Image _dimTop, _dimBottom, _dimLeft, _dimRight;
    private Image _pointer;
    private RectTransform _pointerRt;
    private RectTransform _boxRt;
    private TMP_Text _text;
    private RectTransform _skipRt;
    private string _shownMessage;

    private static Sprite _ringSprite;
    private static Sprite _roundedSprite;

    // ---------- API cho TutorialManager ----------

    public void Show()
    {
        EnsureBuilt();
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);

    public void SetSkipVisible(bool visible)
    {
        EnsureBuilt();
        _skipRt.gameObject.SetActive(visible);
    }

    /// <summary>
    /// Vẽ lại lớp phủ. Gọi mỗi khung hình để bám theo layout/độ phân giải.
    /// holeScreen và pointerScreen là toạ độ màn hình (pixel). showHint = false thì chỉ còn nền tối.
    /// </summary>
    public void Present(Rect holeScreen, Vector2 pointerScreen, string message, bool showHint)
    {
        EnsureBuilt();

        float w = _root.rect.width;
        float h = _root.rect.height;
        if (w < 1f || h < 1f) return; // canvas chưa kịp tính kích thước, khung sau sẽ thử lại

        Vector2 a = ToCanvas(holeScreen.min);
        Vector2 b = ToCanvas(holeScreen.max);
        float xMin = Mathf.Clamp(Mathf.Min(a.x, b.x) - holePadding, 0f, w);
        float xMax = Mathf.Clamp(Mathf.Max(a.x, b.x) + holePadding, 0f, w);
        float yMin = Mathf.Clamp(Mathf.Min(a.y, b.y) - holePadding, 0f, h);
        float yMax = Mathf.Clamp(Mathf.Max(a.y, b.y) + holePadding, 0f, h);

        PlaceRect(_dimTop, 0f, yMax, w, h - yMax);
        PlaceRect(_dimBottom, 0f, 0f, w, yMin);
        PlaceRect(_dimLeft, 0f, yMin, xMin, yMax - yMin);
        PlaceRect(_dimRight, xMax, yMin, w - xMax, yMax - yMin);

        _pointer.enabled = showHint;
        if (showHint) _pointerRt.anchoredPosition = ToCanvas(pointerScreen);

        bool showBox = showHint && !string.IsNullOrEmpty(message);
        _boxRt.gameObject.SetActive(showBox);
        if (showBox)
        {
            if (message != _shownMessage)
            {
                _shownMessage = message;
                _text.text = message;
            }

            // Hộp thoại nằm ở nửa màn hình đối diện vùng sáng để không che nó
            float holeCenterY = (yMin + yMax) * 0.5f;
            float boxY = holeCenterY < h * 0.5f ? h * (1f - boxMargin) : h * boxMargin;
            _boxRt.anchoredPosition = new Vector2(w * 0.5f, boxY);
            _boxRt.sizeDelta = new Vector2(w * boxWidthFraction, boxHeight);
        }

        // Nút Bỏ qua nằm góc trên-phải của vùng an toàn (tránh tai thỏ)
        Rect safe = Screen.safeArea;
        Vector2 corner = ToCanvas(new Vector2(safe.xMax, safe.yMax));
        _skipRt.anchoredPosition = corner + new Vector2(-24f, -24f);
    }

    // ---------- Hoạt ảnh ----------

    private void Update()
    {
        if (!_built || !_pointer.enabled) return;

        float t = Mathf.PingPong(Time.unscaledTime * 1.4f, 1f);
        float s = Mathf.Lerp(0.8f, 1.1f, t);
        _pointerRt.localScale = new Vector3(s, s, 1f);
    }

    // ---------- Dựng UI ----------

    private void EnsureBuilt()
    {
        if (_built) return;
        _built = true;
        _root = (RectTransform)transform;

        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        _dimTop = CreateImage("DimTop", dimColor);
        _dimBottom = CreateImage("DimBottom", dimColor);
        _dimLeft = CreateImage("DimLeft", dimColor);
        _dimRight = CreateImage("DimRight", dimColor);

        _pointer = CreateImage("Pointer", pointerColor);
        _pointer.sprite = pointerSprite != null ? pointerSprite : GetRingSprite();
        _pointer.preserveAspect = true;
        _pointerRt = _pointer.rectTransform;
        _pointerRt.pivot = pointerPivot;
        _pointerRt.sizeDelta = new Vector2(pointerSize, pointerSize);

        Image box = CreateImage("MessageBox", boxColor);
        box.sprite = boxSprite != null ? boxSprite : GetRoundedSprite();
        box.type = Image.Type.Sliced;
        _boxRt = box.rectTransform;
        _boxRt.pivot = new Vector2(0.5f, 0.5f);
        _text = CreateText("Text", _boxRt, textColor, 28f, 54f);
        Stretch(_text.rectTransform, 32f, 20f);

        Image skipImage = CreateImage("Skip", new Color(0f, 0f, 0f, 0.55f));
        skipImage.sprite = GetRoundedSprite();
        skipImage.type = Image.Type.Sliced;
        skipImage.raycastTarget = true; // nút duy nhất nhận chạm
        _skipRt = skipImage.rectTransform;
        _skipRt.pivot = new Vector2(1f, 1f);
        _skipRt.sizeDelta = new Vector2(220f, 84f);
        TMP_Text skipText = CreateText("Label", _skipRt, Color.white, 24f, 40f);
        skipText.text = skipLabel;
        Stretch(skipText.rectTransform, 12f, 8f);

        Button skip = skipImage.gameObject.AddComponent<Button>();
        skip.targetGraphic = skipImage;
        skip.onClick.AddListener(() => SkipClicked?.Invoke());
    }

    private Image CreateImage(string objectName, Color color)
    {
        var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(_root, false);

        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        // Neo ở góc dưới-trái: anchoredPosition chính là khoảng cách tính từ góc đó
        RectTransform rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = Vector2.zero;
        return image;
    }

    private TMP_Text CreateText(string objectName, Transform parent, Color color, float minSize, float maxSize)
    {
        var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = minSize;
        tmp.fontSizeMax = maxSize;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void Stretch(RectTransform rt, float padX, float padY)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padX, padY);
        rt.offsetMax = new Vector2(-padX, -padY);
    }

    // ---------- Toạ độ ----------

    // Toạ độ màn hình (pixel) -> toạ độ canvas tính từ góc dưới-trái
    private Vector2 ToCanvas(Vector2 screenPoint)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screenPoint, null, out Vector2 local);
        return local - _root.rect.min;
    }

    private static void PlaceRect(Image image, float x, float y, float width, float height)
    {
        RectTransform rt = image.rectTransform;
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(Mathf.Max(0f, width), Mathf.Max(0f, height));
    }

    // ---------- Hình mặc định (vẽ bằng code, không cần file ảnh) ----------

    private static Sprite GetRingSprite()
    {
        if (_ringSprite != null) return _ringSprite;

        const int size = 128;
        var pixels = new Color32[size * size];
        float c = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                float ring = Mathf.Clamp01(1.5f - Mathf.Abs(d - 52f) + 4f);   // vành ngoài dày ~10px
                float dot = Mathf.Clamp01(12f - d);                            // chấm giữa
                byte a = (byte)(Mathf.Clamp01(Mathf.Max(ring, dot)) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
        }

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "TutorialRing",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        tex.SetPixels32(pixels);
        tex.Apply(false, true);

        _ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return _ringSprite;
    }

    private static Sprite GetRoundedSprite()
    {
        if (_roundedSprite != null) return _roundedSprite;

        const int size = 64;
        const int radius = 24;
        var pixels = new Color32[size * size];
        float half = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - half) - (half - radius);
                float dy = Mathf.Abs(y + 0.5f - half) - (half - radius);
                float ox = Mathf.Max(dx, 0f);
                float oy = Mathf.Max(dy, 0f);
                float dist = Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(dx, dy), 0f) - radius;
                byte a = (byte)(Mathf.Clamp01(0.5f - dist) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
        }

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "TutorialRounded",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        tex.SetPixels32(pixels);
        tex.Apply(false, true);

        _roundedSprite = Sprite.Create(
            tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return _roundedSprite;
    }
}