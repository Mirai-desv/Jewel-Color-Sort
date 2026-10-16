/// <summary>Sang level tiếp theo (dùng trong WinPanel).</summary>
public class NextLevelButton : BaseButton
{
    protected override void OnClicked() => Game.NextLevel();
}
