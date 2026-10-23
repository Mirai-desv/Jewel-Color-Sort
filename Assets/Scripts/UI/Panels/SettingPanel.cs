using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bảng cài đặt âm thanh. Có thể dùng Toggle (bật/tắt), Slider (âm lượng), hoặc cả hai;
/// ô nào không gán trong Inspector thì bỏ qua. Nút đóng dùng ResumeButton.
/// Toggle đóng vai trò công tắc tắt tiếng, Slider chỉnh mức to nhỏ.
/// </summary>
public class SettingPanel : BasePanel
{
    [Header("Bật / tắt (tuỳ chọn)")]
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;

    [Header("Âm lượng (tuỳ chọn)")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    protected override void Awake()
    {
        base.Awake();

        // Đảm bảo SettingPanel luôn có mức ưu tiên cao nhất (nằm trên cả hạt thế giới và animation)
        Canvas c = GetComponent<Canvas>();
        if (c == null) c = gameObject.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 500;
        if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();

        if (musicToggle != null) musicToggle.onValueChanged.AddListener(OnMusicToggled);
        if (sfxToggle != null) sfxToggle.onValueChanged.AddListener(OnSfxToggled);

        SetupSlider(musicSlider, OnMusicVolumeChanged);
        SetupSlider(sfxSlider, OnSfxVolumeChanged);
    }

    protected override void OnShow()
    {
        transform.SetAsLastSibling();
        RefreshToggles();
        RefreshSliders();
    }

    // ---------- Toggle ----------

    private void OnMusicToggled(bool on)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicOn(on);
        else SaveManager.MusicOn = on;

        // Bật lại mà thanh đang ở 0 thì đưa lên mức đầy, nếu không sẽ vẫn im
        if (on && SaveManager.MusicVolume <= 0f) SetMusicVolume(1f);
        RefreshSliders();
    }

    private void OnSfxToggled(bool on)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSfxOn(on);
        else SaveManager.SfxOn = on;

        if (on && SaveManager.SfxVolume <= 0f) SetSfxVolume(1f);
        RefreshSliders();
    }

    // ---------- Slider ----------

    private void OnMusicVolumeChanged(float value)
    {
        SetMusicVolume(value);
        RefreshToggles(); // kéo lên trên 0 có thể đã tự bật lại nhạc
    }

    private void OnSfxVolumeChanged(float value)
    {
        SetSfxVolume(value);
        RefreshToggles();
    }

    private static void SetupSlider(Slider slider, UnityEngine.Events.UnityAction<float> onChanged)
    {
        if (slider == null) return;

        // Ép đúng thang 0..1 để khỏi phụ thuộc cấu hình trong Inspector
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.onValueChanged.AddListener(onChanged);
    }

    private static void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value);
        else SaveManager.MusicVolume = value;
    }

    private static void SetSfxVolume(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(value);
        else SaveManager.SfxVolume = value;
    }

    // ---------- Đồng bộ hiển thị với dữ liệu đã lưu (không kích hoạt lại sự kiện) ----------

    private void RefreshToggles()
    {
        if (musicToggle != null) musicToggle.SetIsOnWithoutNotify(SaveManager.MusicOn);
        if (sfxToggle != null) sfxToggle.SetIsOnWithoutNotify(SaveManager.SfxOn);
    }

    private void RefreshSliders()
    {
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(SaveManager.MusicVolume);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(SaveManager.SfxVolume);
    }
}