using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;

    protected override void Awake()
    {
        base.Awake();

        if (musicToggle != null) musicToggle.onValueChanged.AddListener(OnMusicChanged);
        if (sfxToggle != null) sfxToggle.onValueChanged.AddListener(OnSfxChanged);
    }

    protected override void OnShow()
    {
        // Hiện đúng trạng thái đã lưu mà không kích hoạt lại sự kiện đổi giá trị
        if (musicToggle != null) musicToggle.SetIsOnWithoutNotify(SaveManager.MusicOn);
        if (sfxToggle != null) sfxToggle.SetIsOnWithoutNotify(SaveManager.SfxOn);
    }

    private void OnMusicChanged(bool on)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicOn(on);
        else SaveManager.MusicOn = on;
    }

    private void OnSfxChanged(bool on)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSfxOn(on);
        else SaveManager.SfxOn = on;
    }
}
