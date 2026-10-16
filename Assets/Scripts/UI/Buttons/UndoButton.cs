public class UndoButton : BaseButton
{
    protected override void OnClicked() => Game.Undo();
}
