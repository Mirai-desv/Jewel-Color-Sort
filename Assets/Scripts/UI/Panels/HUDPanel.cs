using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : BasePanel
{
    [Header("Tham chiếu")]
    [SerializeField] private LevelSession levelSession;
    [SerializeField] private LevelManager levelManager;

    [Header("Hiển thị (tuỳ chọn)")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text scoreText;     // tổng điểm tích luỹ
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Slider progressSlider;

    private int _shownSeconds = -1;
    private int _shownCoins = -1;
    private int _shownScore = -1;

    private void OnEnable()
    {
        if (levelSession == null) return;
        levelSession.LevelStarted += OnLevelStarted;
        levelSession.ProgressChanged += OnProgressChanged;
    }

    private void OnDisable()
    {
        if (levelSession == null) return;
        levelSession.LevelStarted -= OnLevelStarted;
        levelSession.ProgressChanged -= OnProgressChanged;
    }

    private void Update()
    {
        // Dùng đồng hồ của LevelSession để khớp với điểm thời gian
        float elapsed = levelSession != null ? levelSession.ElapsedTime : 0f;
        int seconds = (int)elapsed;
        if (seconds != _shownSeconds)
        {
            _shownSeconds = seconds;
            if (timerText != null) timerText.text = $"{seconds / 60:00}:{seconds % 60:00}";
        }

        int coins = SaveManager.Coins;
        if (coins != _shownCoins)
        {
            _shownCoins = coins;
            if (coinsText != null) coinsText.SetText("{0}", coins);
        }

        int score = SaveManager.TotalScore;
        if (score != _shownScore)
        {
            _shownScore = score;
            if (scoreText != null) scoreText.SetText("{0}", score);
        }
    }

    private void OnLevelStarted()
    {
        _shownSeconds = -1;

        if (levelText != null && levelManager != null)
            levelText.SetText("Lv {0}", levelManager.CurrentIndex + 1);
    }

    private void OnProgressChanged(float progress)
    {
        if (progressSlider != null) progressSlider.value = progress;
        if (progressText != null) progressText.SetText("{0}%", Mathf.RoundToInt(progress * 100f));
    }
}