using UnityEngine;

/// <summary>
/// Lưu tiến độ và cài đặt bằng PlayerPrefs. Là static class nên KHÔNG cần đặt vào scene.
/// </summary>
public static class SaveManager
{
    private const string LevelKey = "bead.level";
    private const string CoinsKey = "bead.coins";
    private const string MusicKey = "bead.music";
    private const string SfxKey = "bead.sfx";

    /// <summary>Level sẽ chơi khi bấm Start (0 = Level 1).</summary>
    public static int CurrentLevelIndex
    {
        get => PlayerPrefs.GetInt(LevelKey, 0);
        set => PlayerPrefs.SetInt(LevelKey, Mathf.Max(0, value));
    }

    public static int Coins
    {
        get => PlayerPrefs.GetInt(CoinsKey, 0);
        set => PlayerPrefs.SetInt(CoinsKey, Mathf.Max(0, value));
    }

    public static bool MusicOn
    {
        get => PlayerPrefs.GetInt(MusicKey, 1) == 1;
        set => PlayerPrefs.SetInt(MusicKey, value ? 1 : 0);
    }

    public static bool SfxOn
    {
        get => PlayerPrefs.GetInt(SfxKey, 1) == 1;
        set => PlayerPrefs.SetInt(SfxKey, value ? 1 : 0);
    }

    public static void Save() => PlayerPrefs.Save();

    /// <summary>Xoá tiến độ và tiền (giữ lại cài đặt âm thanh). Dùng khi test.</summary>
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(LevelKey);
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.Save();
    }
}
