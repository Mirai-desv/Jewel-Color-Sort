using UnityEngine;

public enum Sfx { Click, Select, Move, Invalid, Win, Lose }

/// <summary>
/// Phát nhạc nền và hiệu ứng âm thanh. Clip nào để trống thì bỏ qua (chưa có âm thanh vẫn chạy bình thường).
/// Gọi từ bất kỳ đâu: AudioManager.PlaySfx(Sfx.Click);
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Nguồn phát (để trống thì tự tạo)")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clip")]
    [SerializeField] private AudioClip music;
    [SerializeField] private AudioClip click;
    [SerializeField] private AudioClip select;
    [SerializeField] private AudioClip move;
    [SerializeField] private AudioClip invalid;
    [SerializeField] private AudioClip win;
    [SerializeField] private AudioClip lose;

    private void Awake()
    {
        Instance = this;

        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        sfxSource.playOnAwake = false;

        ApplyMusic();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Phát hiệu ứng (an toàn khi chưa có AudioManager trong scene).</summary>
    public static void PlaySfx(Sfx sfx)
    {
        if (Instance != null) Instance.Play(sfx);
    }

    public void SetMusicOn(bool on)
    {
        SaveManager.MusicOn = on;
        ApplyMusic();
    }

    public void SetSfxOn(bool on) => SaveManager.SfxOn = on;

    private void Play(Sfx sfx)
    {
        if (!SaveManager.SfxOn) return;

        AudioClip clip = sfx switch
        {
            Sfx.Click => click,
            Sfx.Select => select,
            Sfx.Move => move,
            Sfx.Invalid => invalid,
            Sfx.Win => win,
            Sfx.Lose => lose,
            _ => null
        };

        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    private void ApplyMusic()
    {
        if (music == null) return;

        musicSource.clip = music;
        if (SaveManager.MusicOn)
        {
            if (!musicSource.isPlaying) musicSource.Play();
        }
        else
        {
            musicSource.Stop();
        }
    }
}
