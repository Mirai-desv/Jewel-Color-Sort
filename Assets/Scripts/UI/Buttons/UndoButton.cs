/// <summary>Hoàn tác nước đi gần nhất.</summary>
public class UndoButton : BaseButton
{
    protected override void OnClicked() => Game.Undo();
}
