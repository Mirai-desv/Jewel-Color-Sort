using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugLevelSelector : MonoBehaviour
{
    [Header("Hiển thị")]
    [Tooltip("Bật/tắt nút chọn level ở homescreen. Có thể bỏ chọn trong Editor bất cứ lúc nào để ẩn.")]
    [SerializeField] private bool showDebugButton = true;

    [Header("Tham chiếu")]
    [SerializeField] private GameObject debugButtonRoot;
    [SerializeField] private LevelManager levelManager;

    private GameObject _popupRoot;
    private Transform _gridContainer;
    private TMP_FontAsset _fontAsset;

    public bool ShowDebugButton
    {
        get => showDebugButton;
        set
        {
            showDebugButton = value;
            ApplyVisibility();
        }
    }

    private void OnValidate()
    {
        ApplyVisibility();
    }

    private void Awake()
    {
        if (levelManager == null) levelManager = FindAnyObjectByType<LevelManager>();
        FindFontAsset();
        EnsureDebugButton();
        ApplyVisibility();
    }

    private void Start()
    {
        EnsureDebugButton();
        ApplyVisibility();
    }

    public void ApplyVisibility()
    {
        if (debugButtonRoot != null)
        {
            debugButtonRoot.SetActive(showDebugButton);
        }
    }

    private void FindFontAsset()
    {
        if (_fontAsset != null) return;
        TMP_Text anyText = FindAnyObjectByType<TMP_Text>();
        if (anyText != null) _fontAsset = anyText.font;
    }

    public void EnsureDebugButton()
    {
        if (debugButtonRoot != null)
        {
            Button btn = debugButtonRoot.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(OpenLevelSelector);
                btn.onClick.AddListener(OpenLevelSelector);
            }
            return;
        }

        // Tìm trong con của object này
        Transform existing = transform.Find("[DEBUG] LevelSelectButton");
        if (existing != null)
        {
            debugButtonRoot = existing.gameObject;
            Button btn = debugButtonRoot.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(OpenLevelSelector);
                btn.onClick.AddListener(OpenLevelSelector);
            }
            return;
        }

        // Tạo nút debug nếu chưa có
        BuildDebugButton();
    }

    private void BuildDebugButton()
    {
        GameObject btnGo = new GameObject("[DEBUG] LevelSelectButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(transform, false);
        debugButtonRoot = btnGo;

        RectTransform rt = btnGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(520f, 95f);
        rt.anchoredPosition = new Vector2(0f, -295f); // Ngay phía dưới nút ExitButton (-150 - 140)

        Image img = btnGo.GetComponent<Image>();
        img.color = new Color(0.16f, 0.18f, 0.24f, 0.95f); // Nền xám đen phong cách dev tool

        Outline outline = btnGo.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.72f, 0.15f, 0.95f); // Viền vàng/cam đậm chất debug
        outline.effectDistance = new Vector2(3f, -3f);

        Button btn = btnGo.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        cb.pressedColor = new Color(0.75f, 0.75f, 0.75f);
        btn.colors = cb;
        btn.onClick.AddListener(OpenLevelSelector);

        // Text bên trong nút
        GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(btnGo.transform, false);

        RectTransform trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        if (_fontAsset != null) tmp.font = _fontAsset;
        tmp.text = "🛠 CHỌN LEVEL (DEBUG)";
        tmp.fontSize = 32f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(1f, 0.88f, 0.45f); // Chữ vàng sáng
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        ApplyVisibility();
    }

    [ContextMenu("Open Level Selector")]
    public void OpenLevelSelector()
    {
        AudioManager.PlaySfx(Sfx.Click);
        if (_popupRoot == null)
        {
            BuildLevelSelectPopup();
        }

        PopulateLevels();
        _popupRoot.SetActive(true);
    }

    public void ClosePopup()
    {
        AudioManager.PlaySfx(Sfx.Click);
        if (_popupRoot != null) _popupRoot.SetActive(false);
    }

    private void BuildLevelSelectPopup()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        Transform rootParent = canvas != null ? canvas.transform : transform.root;

        _popupRoot = new GameObject("[DEBUG] LevelSelectPopup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        _popupRoot.transform.SetParent(rootParent, false);

        // Đảm bảo popup luôn vẽ đè lên tất cả panel khác
        Canvas c = _popupRoot.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 500;
        _popupRoot.AddComponent<GraphicRaycaster>();

        RectTransform prt = _popupRoot.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;

        // Nền đen bán trong suốt bao phủ toàn màn hình
        Image pimg = _popupRoot.GetComponent<Image>();
        pimg.color = new Color(0f, 0f, 0f, 0.82f);
        Button backdropBtn = _popupRoot.AddComponent<Button>();
        backdropBtn.transition = Selectable.Transition.None;
        backdropBtn.onClick.AddListener(ClosePopup);

        // Khung cửa sổ chính giữa
        GameObject window = new GameObject("Window", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        window.transform.SetParent(_popupRoot.transform, false);

        RectTransform wrt = window.GetComponent<RectTransform>();
        wrt.anchorMin = new Vector2(0.5f, 0.5f);
        wrt.anchorMax = new Vector2(0.5f, 0.5f);
        wrt.pivot = new Vector2(0.5f, 0.5f);
        wrt.sizeDelta = new Vector2(880f, 1080f);

        Image wimg = window.GetComponent<Image>();
        wimg.color = new Color(0.12f, 0.13f, 0.18f, 0.98f); // Nền xám xanh đen sang trọng

        Outline woutline = window.AddComponent<Outline>();
        woutline.effectColor = new Color(0.35f, 0.40f, 0.55f, 0.9f);
        woutline.effectDistance = new Vector2(3f, -3f);

        // Tiêu đề
        GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(window.transform, false);
        RectTransform trt = titleGo.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 1f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0f, -30f);
        trt.sizeDelta = new Vector2(-60f, 50f);

        TextMeshProUGUI titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
        if (_fontAsset != null) titleTmp.font = _fontAsset;
        titleTmp.text = "🛠 CHỌN LEVEL ĐỂ TEST";
        titleTmp.fontSize = 38f;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = new Color(1f, 0.85f, 0.35f);
        titleTmp.alignment = TextAlignmentOptions.Center;

        // Subtitle
        GameObject subGo = new GameObject("Subtitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        subGo.transform.SetParent(window.transform, false);
        RectTransform srt = subGo.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0f, 1f);
        srt.anchorMax = new Vector2(1f, 1f);
        srt.pivot = new Vector2(0.5f, 1f);
        srt.anchoredPosition = new Vector2(0f, -85f);
        srt.sizeDelta = new Vector2(-60f, 35f);

        TextMeshProUGUI subTmp = subGo.GetComponent<TextMeshProUGUI>();
        if (_fontAsset != null) subTmp.font = _fontAsset;
        subTmp.text = "Bấm vào bất kỳ level nào để vào chơi và kiểm tra màn";
        subTmp.fontSize = 22f;
        subTmp.color = new Color(0.75f, 0.80f, 0.90f);
        subTmp.alignment = TextAlignmentOptions.Center;

        // Nút Đóng (X) góc phải trên
        GameObject closeBtnGo = new GameObject("CloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeBtnGo.transform.SetParent(window.transform, false);
        RectTransform crt = closeBtnGo.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(1f, 1f);
        crt.anchorMax = new Vector2(1f, 1f);
        crt.pivot = new Vector2(1f, 1f);
        crt.anchoredPosition = new Vector2(-25f, -25f);
        crt.sizeDelta = new Vector2(65f, 65f);

        Image cimg = closeBtnGo.GetComponent<Image>();
        cimg.color = new Color(0.85f, 0.25f, 0.25f);
        Button cbtn = closeBtnGo.GetComponent<Button>();
        cbtn.onClick.AddListener(ClosePopup);

        GameObject ctext = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        ctext.transform.SetParent(closeBtnGo.transform, false);
        RectTransform ctrt = ctext.GetComponent<RectTransform>();
        ctrt.anchorMin = Vector2.zero;
        ctrt.anchorMax = Vector2.one;
        ctrt.offsetMin = Vector2.zero;
        ctrt.offsetMax = Vector2.zero;
        TextMeshProUGUI ctmp = ctext.GetComponent<TextMeshProUGUI>();
        if (_fontAsset != null) ctmp.font = _fontAsset;
        ctmp.text = "✕";
        ctmp.fontSize = 34f;
        ctmp.fontStyle = FontStyles.Bold;
        ctmp.color = Color.white;
        ctmp.alignment = TextAlignmentOptions.Center;

        // Vùng ScrollView chứa danh sách các level
        GameObject scrollGo = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollGo.transform.SetParent(window.transform, false);
        RectTransform scrt = scrollGo.GetComponent<RectTransform>();
        scrt.anchorMin = new Vector2(0f, 0f);
        scrt.anchorMax = new Vector2(1f, 1f);
        scrt.pivot = new Vector2(0.5f, 0.5f);
        scrt.offsetMin = new Vector2(35f, 110f); // chừa chỗ cho footer phía dưới
        scrt.offsetMax = new Vector2(-35f, -135f); // chừa chỗ cho header phía trên

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(RectMask2D));
        viewport.transform.SetParent(scrollGo.transform, false);
        RectTransform vprt = viewport.GetComponent<RectTransform>();
        vprt.anchorMin = Vector2.zero;
        vprt.anchorMax = Vector2.one;
        vprt.offsetMin = Vector2.zero;
        vprt.offsetMax = Vector2.zero;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        RectTransform conrt = content.GetComponent<RectTransform>();
        conrt.anchorMin = new Vector2(0f, 1f);
        conrt.anchorMax = new Vector2(1f, 1f);
        conrt.pivot = new Vector2(0.5f, 1f);
        conrt.anchoredPosition = Vector2.zero;
        conrt.sizeDelta = new Vector2(0f, 600f);

        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(250f, 100f);
        grid.spacing = new Vector2(20f, 18f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect sr = scrollGo.GetComponent<ScrollRect>();
        sr.content = conrt;
        sr.viewport = vprt;
        sr.horizontal = false;
        sr.vertical = true;
        sr.scrollSensitivity = 25f;

        _gridContainer = content.transform;

        // Footer với nút Reset Level 1 và nút Đóng
        GameObject footerGo = new GameObject("Footer", typeof(RectTransform));
        footerGo.transform.SetParent(window.transform, false);
        RectTransform frt = footerGo.GetComponent<RectTransform>();
        frt.anchorMin = new Vector2(0f, 0f);
        frt.anchorMax = new Vector2(1f, 0f);
        frt.pivot = new Vector2(0.5f, 0f);
        frt.anchoredPosition = new Vector2(0f, 25f);
        frt.sizeDelta = new Vector2(-70f, 65f);

        // Nút Reset Save về Lv 1
        GameObject resetBtnGo = new GameObject("ResetSaveBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        resetBtnGo.transform.SetParent(footerGo.transform, false);
        RectTransform rbrt = resetBtnGo.GetComponent<RectTransform>();
        rbrt.anchorMin = new Vector2(0f, 0.5f);
        rbrt.anchorMax = new Vector2(0.48f, 0.5f);
        rbrt.sizeDelta = new Vector2(0f, 60f);
        Image rimg = resetBtnGo.GetComponent<Image>();
        rimg.color = new Color(0.24f, 0.28f, 0.38f);
        Button rbtn = resetBtnGo.GetComponent<Button>();
        rbtn.onClick.AddListener(() =>
        {
            AudioManager.PlaySfx(Sfx.Click);
            SaveManager.CurrentLevelIndex = 0;
            SaveManager.Save();
            PopulateLevels();
        });

        GameObject rtext = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        rtext.transform.SetParent(resetBtnGo.transform, false);
        RectTransform rtrt = rtext.GetComponent<RectTransform>();
        rtrt.anchorMin = Vector2.zero;
        rtrt.anchorMax = Vector2.one;
        rtrt.offsetMin = Vector2.zero;
        rtrt.offsetMax = Vector2.zero;
        TextMeshProUGUI rtmp = rtext.GetComponent<TextMeshProUGUI>();
        if (_fontAsset != null) rtmp.font = _fontAsset;
        rtmp.text = "🔄 Đặt Lại Tiến Trình (Lv 1)";
        rtmp.fontSize = 22f;
        rtmp.fontStyle = FontStyles.Bold;
        rtmp.color = new Color(0.9f, 0.95f, 1f);
        rtmp.alignment = TextAlignmentOptions.Center;

        // Nút Đóng Popup
        GameObject closeFooterBtn = new GameObject("CloseFooterBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeFooterBtn.transform.SetParent(footerGo.transform, false);
        RectTransform cfbrt = closeFooterBtn.GetComponent<RectTransform>();
        cfbrt.anchorMin = new Vector2(0.52f, 0.5f);
        cfbrt.anchorMax = new Vector2(1f, 0.5f);
        cfbrt.sizeDelta = new Vector2(0f, 60f);
        Image cfimg = closeFooterBtn.GetComponent<Image>();
        cfimg.color = new Color(0.35f, 0.22f, 0.24f);
        Button cfbtn = closeFooterBtn.GetComponent<Button>();
        cfbtn.onClick.AddListener(ClosePopup);

        GameObject cftext = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        cftext.transform.SetParent(closeFooterBtn.transform, false);
        RectTransform cftrt = cftext.GetComponent<RectTransform>();
        cftrt.anchorMin = Vector2.zero;
        cftrt.anchorMax = Vector2.one;
        cftrt.offsetMin = Vector2.zero;
        cftrt.offsetMax = Vector2.zero;
        TextMeshProUGUI cftmp = cftext.GetComponent<TextMeshProUGUI>();
        if (_fontAsset != null) cftmp.font = _fontAsset;
        cftmp.text = "Đóng";
        cftmp.fontSize = 24f;
        cftmp.fontStyle = FontStyles.Bold;
        cftmp.color = Color.white;
        cftmp.alignment = TextAlignmentOptions.Center;
    }

    private void PopulateLevels()
    {
        if (_gridContainer == null) return;

        // Xoá các nút cũ
        for (int i = _gridContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(_gridContainer.GetChild(i).gameObject);
        }

        if (levelManager == null) levelManager = FindAnyObjectByType<LevelManager>();
        int count = levelManager != null ? levelManager.LevelCount : 15;
        if (count <= 0) count = 15;

        int currentPlaying = SaveManager.CurrentLevelIndex;

        for (int i = 0; i < count; i++)
        {
            int levelIdx = i;
            bool isCurrent = levelIdx == currentPlaying;

            GameObject lbtnGo = new GameObject($"Level_{levelIdx + 1:00}_Btn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            lbtnGo.transform.SetParent(_gridContainer, false);

            Image img = lbtnGo.GetComponent<Image>();
            img.color = isCurrent
                ? new Color(0.35f, 0.28f, 0.12f, 0.98f) // Đang chơi: viền vàng ấm
                : new Color(0.18f, 0.21f, 0.29f, 0.95f); // Bình thường: xám xanh

            Outline outline = lbtnGo.AddComponent<Outline>();
            outline.effectColor = isCurrent
                ? new Color(1f, 0.80f, 0.20f, 1f) // Nổi bật viền vàng
                : new Color(0.28f, 0.33f, 0.44f, 0.8f);
            outline.effectDistance = isCurrent ? new Vector2(3f, -3f) : new Vector2(2f, -2f);

            Button btn = lbtnGo.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(1.25f, 1.25f, 1.25f);
            cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            btn.colors = cb;

            btn.onClick.AddListener(() =>
            {
                AudioManager.PlaySfx(Sfx.Click);
                ClosePopup();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadSpecificLevel(levelIdx);
                }
                else if (levelManager != null)
                {
                    SaveManager.CurrentLevelIndex = levelIdx;
                    levelManager.LoadLevel(levelIdx);
                }
            });

            // Text hiển thị
            GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(lbtnGo.transform, false);
            RectTransform trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(8f, 6f);
            trt.offsetMax = new Vector2(-8f, -6f);

            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            if (_fontAsset != null) tmp.font = _fontAsset;

            string levelName = $"Level {levelIdx + 1}";
            string subText = "";
            if (levelManager != null && levelIdx < levelManager.LevelCount)
            {
                LevelData data = levelManager.GetLevel(levelIdx);
                if (data != null)
                {
                    subText = $"{data.Background.Count} ô";
                }
            }

            if (isCurrent)
            {
                tmp.text = $"<b>{levelName}</b>\n<size=18><color=#FFD54F>★ ĐANG CHƠI</color></size>";
            }
            else if (!string.IsNullOrEmpty(subText))
            {
                tmp.text = $"<b>{levelName}</b>\n<size=18><color=#9EA7BD>{subText}</color></size>";
            }
            else
            {
                tmp.text = $"<b>{levelName}</b>";
            }

            tmp.fontSize = 28f;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }
    }

    [ContextMenu("Toggle Debug Button")]
    public void ToggleDebugButton()
    {
        ShowDebugButton = !ShowDebugButton;
    }
}
