/// <summary>Bắt đầu chơi từ level đã lưu.</summary>
public class StartButton : BaseButton
{
    protected override void OnClicked() => Game.StartGame();
}
