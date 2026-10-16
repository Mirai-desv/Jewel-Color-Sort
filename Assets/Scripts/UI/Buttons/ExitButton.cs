/// <summary>Thoát game.</summary>
public class ExitButton : BaseButton
{
    protected override void OnClicked() => Game.QuitGame();
}
