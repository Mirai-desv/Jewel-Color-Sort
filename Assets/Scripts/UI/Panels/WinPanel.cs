using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinPanel : BasePanel
{
    //[SerializeField] private Image winImage;
    //[SerializeField] private Sprite titleSprite;
    [SerializeField] private TMP_Text nextButtonLabel;

    [Header("Chỉ hiện khi thắng lần đầu (tuỳ chọn)")]
    [SerializeField] private GameObject rewardRoot;
    [SerializeField] private TMP_Text rewardText;       // xu nhận được
    [SerializeField] private GameObject scoreRoot;
    [SerializeField] private TMP_Text scoreText;        // điểm màn này
    [SerializeField] private TMP_Text breakdownText;    // chi tiết thời gian / nước đi / undo --> Ko cần
    [SerializeField] private TMP_Text totalScoreText;   // tổng điểm tích luỹ --> Ko cần nốt

    [Header("Chỉ hiện khi chơi lại level đã thắng (tuỳ chọn)")]
    [SerializeField] private GameObject replayNote;     // vd "Replay - no score"

    // GameManager gọi trước khi hiện panel
    public void Setup(int levelNumber, int coins, ScoreResult score, bool awarded, int totalScore, bool hasNext)
    {
        //if (winImage != null) winImage.sprite = titleSprite;
        //if (nextButtonLabel != null) nextButtonLabel.SetText(hasNext ? "Next Level" : "Play Again");

        if (rewardRoot != null) rewardRoot.SetActive(awarded);
        if (scoreRoot != null) scoreRoot.SetActive(awarded);
        if (replayNote != null) replayNote.SetActive(!awarded);

        if (!awarded) return;

        if (rewardText != null) rewardText.SetText("+{0}", coins);
        if (scoreText != null) scoreText.SetText("{0}", score.Total);
        if (totalScoreText != null) totalScoreText.SetText("{0}", totalScore);

        if (breakdownText != null)
        {
            int sec = (int)score.ElapsedSeconds;
            breakdownText.SetText(
                "Base  +" + score.BaseScore + "\n" +
                "Time " + (sec / 60).ToString("00") + ":" + (sec % 60).ToString("00") + "  +" + score.TimeBonus + "\n" +
                "Moves " + score.Moves + "  +" + score.MoveBonus + "\n" +
                "Undo x" + score.Undos + "  -" + score.UndoPenalty);
        }
    }
}