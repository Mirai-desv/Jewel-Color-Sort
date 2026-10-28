using UnityEngine;

/// <summary>
/// Lưu tiến độ và cài đặt bằng PlayerPrefs. Là static class nên KHÔNG cần đặt vào scene.
/// </summary>
public static class SaveManager
{
    private const string LevelKey = "bead.level";
    private const string HighestKey = "bead.highest";
    private const string CoinsKey = "bead.coins";
    private const string ScoreKey = "bead.score";
    private const string BestPrefix = "bead.best.";
    private const string ItemsKey = "bead.items";
    private const string MusicKey = "bead.music";
    private const string SfxKey = "bead.sfx";
    private const string MusicVolumeKey = "bead.musicVolume";
    private const string SfxVolumeKey = "bead.sfxVolume";

    private const string DialogueSeenPrefix = "bead.dialogueSeen.";

    // Level sẽ chơi khi bấm Start
    public static int CurrentLevelIndex
    {
        get => PlayerPrefs.GetInt(LevelKey, 0);
        set => PlayerPrefs.SetInt(LevelKey, Mathf.Max(0, value));
    }

    // Số level đã mở khoá = chỉ số level cao nhất được chơi. Level i &lt; giá trị này là đã thắng, level == giá trị này là đang chơi, level lớn hơn là chưa mở.
    public static int HighestUnlockedLevel
    {
        get => PlayerPrefs.GetInt(HighestKey, 0);
        set => PlayerPrefs.SetInt(HighestKey, Mathf.Max(0, value));
    }

    public static bool HasSeenDialogue(int levelIndex) => PlayerPrefs.GetInt(DialogueSeenPrefix + levelIndex, 0) == 1;
    public static void MarkDialogueSeen(int levelIndex) => PlayerPrefs.SetInt(DialogueSeenPrefix + levelIndex, 1);

    public static int Coins
    {
        get => PlayerPrefs.GetInt(CoinsKey, 0);
        set => PlayerPrefs.SetInt(CoinsKey, Mathf.Max(0, value));
    }

    /// <summary>Tổng điểm tích luỹ.</summary>
    public static int TotalScore
    {
        get => PlayerPrefs.GetInt(ScoreKey, 0);
        set => PlayerPrefs.SetInt(ScoreKey, Mathf.Max(0, value));
    }

    public static int GetLevelBestScore(int levelIndex) => PlayerPrefs.GetInt(BestPrefix + levelIndex, 0);

    public static void SetLevelBestScore(int levelIndex, int score)
    {
        if (score > GetLevelBestScore(levelIndex)) PlayerPrefs.SetInt(BestPrefix + levelIndex, score);
    }

    // ---------- Kho vật phẩm (lưu dạng "id1,id2,...") ----------

    public static string[] GetItems()
    {
        string raw = PlayerPrefs.GetString(ItemsKey, "");
        return raw.Length == 0 ? System.Array.Empty<string>() : raw.Split(',');
    }

    public static bool HasItem(string id) =>
        !string.IsNullOrEmpty(id) && System.Array.IndexOf(GetItems(), id) >= 0;

    public static void AddItem(string id)
    {
        if (string.IsNullOrEmpty(id) || HasItem(id)) return;
        string raw = PlayerPrefs.GetString(ItemsKey, "");
        PlayerPrefs.SetString(ItemsKey, raw.Length == 0 ? id : raw + "," + id);
    }

    // ---------- Cài đặt ----------

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

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        set => PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        set => PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
    }

    public static void Save() => PlayerPrefs.Save();

    /// <summary>Xoá tiến độ, tiền, điểm, vật phẩm (giữ lại cài đặt âm thanh). Dùng khi test.</summary>
    public static void ResetProgress()
    {
        int highest = HighestUnlockedLevel;
        for (int i = 0; i <= highest; i++) PlayerPrefs.DeleteKey(BestPrefix + i);
        for (int i = 0; i < 200; i++) PlayerPrefs.DeleteKey(DialogueSeenPrefix + i);

        PlayerPrefs.DeleteKey(LevelKey);
        PlayerPrefs.DeleteKey(HighestKey);
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.DeleteKey(ScoreKey);
        PlayerPrefs.DeleteKey(ItemsKey);
        PlayerPrefs.Save();
    }
}