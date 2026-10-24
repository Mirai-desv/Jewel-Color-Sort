using UnityEngine;

/// <summary>Kết quả tính điểm của 1 lần thắng.</summary>
public readonly struct ScoreResult
{
    public readonly int BaseScore;
    public readonly int TimeBonus;
    public readonly int MoveBonus;
    public readonly int UndoPenalty;
    public readonly int Total;
    public readonly float ElapsedSeconds;
    public readonly int Moves;
    public readonly int Undos;

    public ScoreResult(int baseScore, int timeBonus, int moveBonus, int undoPenalty, int total,
                       float elapsedSeconds, int moves, int undos)
    {
        BaseScore = baseScore;
        TimeBonus = timeBonus;
        MoveBonus = moveBonus;
        UndoPenalty = undoPenalty;
        Total = total;
        ElapsedSeconds = elapsedSeconds;
        Moves = moves;
        Undos = undos;
    }
}

/// <summary>
/// Điểm = Base + ThưởngThờiGian + ThưởngNướcĐi - PhạtUndo (không âm).
///  - Thưởng thời gian: giảm tuyến tính từ Max về 0 trong TimeBonusWindow giây.
///  - Thưởng nước đi: Max trừ dần khi số nước vượt Par Moves.
/// </summary>
public static class ScoreCalculator
{
    public static ScoreResult Calculate(GameConfig config, LevelData level, float elapsed, int moves, int undos)
    {
        int baseScore = config != null ? config.BaseScore : 1000;
        int maxTime = config != null ? config.MaxTimeBonus : 500;
        float window = config != null ? config.TimeBonusWindow : 120f;
        int maxMove = config != null ? config.MaxMoveBonus : 500;
        int movePenalty = config != null ? config.MovePenalty : 25;
        int undoPenalty = config != null ? config.UndoPenalty : 50;
        int defaultPar = config != null ? config.DefaultParMoves : 30;

        int par = level != null && level.ParMoves > 0 ? level.ParMoves : defaultPar;

        int timeBonus = Mathf.RoundToInt(maxTime * Mathf.Clamp01(1f - elapsed / Mathf.Max(1f, window)));
        int over = Mathf.Max(0, moves - par);
        int moveBonus = Mathf.Max(0, maxMove - over * movePenalty);
        int undoPen = undos * undoPenalty;
        int total = Mathf.Max(0, baseScore + timeBonus + moveBonus - undoPen);

        return new ScoreResult(baseScore, timeBonus, moveBonus, undoPen, total, elapsed, moves, undos);
    }
}