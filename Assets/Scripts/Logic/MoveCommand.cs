using System.Collections.Generic;

public enum MoveKind { GridToGrid, GridToTray, TrayToGrid }

/// <summary>1 hạt di chuyển. From/To là index ô lưới, hoặc index ô khay tuỳ theo Kind.</summary>
public readonly struct MoveStep
{
    public readonly MoveKind Kind;
    public readonly int From;
    public readonly int To;
    public readonly Type BeadType;

    public MoveStep(MoveKind kind, int from, int to, Type beadType)
    {
        Kind = kind;
        From = from;
        To = to;
        BeadType = beadType;
    }
}

/// <summary>1 nước đi của người chơi = nhiều hạt di chuyển cùng lúc. Dùng để Undo.</summary>
public class MoveCommand
{
    private readonly List<MoveStep> _steps = new List<MoveStep>(16);

    public IReadOnlyList<MoveStep> Steps => _steps;

    public void Add(MoveKind kind, int from, int to, Type beadType) =>
        _steps.Add(new MoveStep(kind, from, to, beadType));
}
