/// <summary>Chơi lại level hiện tại.</summary>
public class RestartButton : BaseButton
{
    protected override void OnClicked() => Game.RestartLevel();
}
