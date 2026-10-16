/// <summary>Đóng bảng Setting, quay lại game.</summary>
public class ResumeButton : BaseButton
{
    protected override void OnClicked() => Game.CloseSettings();
}
