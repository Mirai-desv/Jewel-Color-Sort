using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD khi chơi: số level, thời gian chơi (đếm lên), tiền, tiến độ (% ô đã đúng).
/// Ô nào không gán trong Inspector thì bỏ qua.
/// </summary>
public class HUDPanel : BasePanel
{
    [Header("Tham chiếu")]
    [SerializeField] private LevelSession levelSession;
    [SerializeField] private LevelManager levelManager;

    [Header("Hiển thị (tuỳ chọn)")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Slider progressSlider;

    private float _elapsed;
    private int _shownSeconds = -1;
    private int _shownCoins = -1;

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
        // Chỉ đếm giờ khi đang chơi
        if (GameManager.Instance != null && GameManager.Instance.State == GameState.Playing)
            _elapsed += Time.deltaTime;

        int seconds = (int)_elapsed;
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
    }

    private void OnLevelStarted()
    {
        _elapsed = 0f;
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
