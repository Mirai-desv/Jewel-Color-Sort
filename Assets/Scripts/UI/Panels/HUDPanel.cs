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
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Slider progressSlider;

    private float _elapsed;
    private int _shownSeconds = -1;
    private int _shownCoins = -1;

    protected override void Awake()
    {
        base.Awake();
        EnsureTextSorting();
    }

    protected override void OnShow()
    {
        base.OnShow();
        UpdateLevelText();
    }

    private void OnEnable()
    {
        UpdateLevelText();
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
        UpdateLevelText();
    }

    public void UpdateLevelText()
    {
        if (levelText == null) return;
        EnsureTextSorting();
        int idx = (levelManager != null && levelManager.CurrentIndex >= 0)
            ? levelManager.CurrentIndex
            : SaveManager.CurrentLevelIndex;
        levelText.SetText("Level {0}", idx + 1);
    }

    private void EnsureTextSorting()
    {
        if (levelText == null) return;
        Canvas c = levelText.GetComponent<Canvas>();
        if (c == null) c = levelText.gameObject.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 205; // Nổi lên trên khung gỗ Level Image (200)

        RectTransform rt = levelText.rectTransform;
        if (rt != null && rt.sizeDelta.x < 350f)
        {
            rt.sizeDelta = new Vector2(350f, Mathf.Max(60f, rt.sizeDelta.y));
        }
        levelText.alignment = TextAlignmentOptions.Center;
    }

    private void OnProgressChanged(float progress)
    {
        if (progressSlider != null) progressSlider.value = progress;
        if (progressText != null) progressText.SetText("{0}%", Mathf.RoundToInt(progress * 100f));
    }
}
