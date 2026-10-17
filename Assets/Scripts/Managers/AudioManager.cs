using UnityEngine;

public enum Sfx { Click, Select, Move, Invalid, Win, Lose }

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AudioManager>();
            }
            return _instance;
        }
        private set => _instance = value;
    }

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
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        _instance = this;

        // Nếu PlayerPrefs lưu nhầm 0 khi âm thanh đang bật, khôi phục về 1
        if (SaveManager.MusicOn && SaveManager.MusicVolume <= 0.01f)
            SaveManager.MusicVolume = 1f;
        if (SaveManager.SfxOn && SaveManager.SfxVolume <= 0.01f)
            SaveManager.SfxVolume = 1f;

        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = SaveManager.SfxVolume;

        ApplyMusic();
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    // Phát hiệu ứng (an toàn khi chưa có AudioManager trong scene)
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

    public void SetMusicVolume(float volume)
    {
        SaveManager.MusicVolume = volume;
        if(volume > 0f && !SaveManager.MusicOn) SaveManager.MusicOn = true;
        ApplyMusic();
    }

    public void SetSfxVolume(float volume)
    {
        SaveManager.SfxVolume = volume;
        if(volume > 0f && !SaveManager.SfxOn) SaveManager.SfxOn = true;
        if (sfxSource != null) sfxSource.volume = volume;
    }   

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

        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, SaveManager.SfxVolume);
        }
    }

    private void ApplyMusic()
    {
        if (musicSource == null) return;

        musicSource.volume = SaveManager.MusicOn ? SaveManager.MusicVolume : 0f;

        if(music == null) return;

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
