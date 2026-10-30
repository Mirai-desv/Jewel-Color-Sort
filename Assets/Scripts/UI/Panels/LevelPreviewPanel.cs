using UnityEngine;
using UnityEngine.UI;

/// Popup xem ảnh đầy đủ của level đang chơi (dùng chung LevelData.PreviewImage với Main Menu).
/// Khung ảnh không phủ kín màn hình, phần tối bên ngoài là Backdrop: bấm vào đó sẽ đóng popup.
public class LevelPreviewPanel : BasePanel
{
    [Header("Tham chiếu")]
    [SerializeField] private LevelManager levelManager;

    [Header("Hiển thị")]
    [SerializeField] private Image previewImage;
    [Tooltip("Tuỳ chọn: AspectRatioFitter (Fit In Parent) trên khung để khung ôm đúng tỉ lệ ảnh.")]
    [SerializeField] private AspectRatioFitter frameFitter;

    [Header("Đóng popup")]
    [Tooltip("Button phủ kín màn hình, nằm phía sau khung. Bấm vào đây là đóng.")]
    [SerializeField] private Button backdropButton;

    protected override void Awake()
    {
        base.Awake();

        // Giống SettingPanel: luôn nằm trên cả hạt thế giới và animation
        Canvas c = GetComponent<Canvas>();
        if (c == null) c = gameObject.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 500;
        if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();

        if (backdropButton != null) backdropButton.onClick.AddListener(OnBackdropClicked);
    }

    private void OnDestroy()
    {
        if (backdropButton != null) backdropButton.onClick.RemoveListener(OnBackdropClicked);
    }

    protected override void OnShow()
    {
        transform.SetAsLastSibling();

        LevelData level = levelManager != null ? levelManager.CurrentLevel : null;
        Sprite sprite = level != null ? level.PreviewImage : null;

        if (previewImage != null) previewImage.sprite = sprite;

        if (frameFitter != null && sprite != null && sprite.rect.height > 0f)
            frameFitter.aspectRatio = sprite.rect.width / sprite.rect.height;
    }

    private void OnBackdropClicked()
    {
        AudioManager.PlaySfx(Sfx.Click);
        if (GameManager.Instance != null) GameManager.Instance.ClosePreview();
    }
}