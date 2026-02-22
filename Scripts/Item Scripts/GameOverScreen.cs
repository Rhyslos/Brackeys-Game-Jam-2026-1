using Godot;

public partial class GameOverScreen : CanvasLayer
{
    // exported variables
    [Export] public Label TitleLabel;
    [Export] public Label ReasonLabel;
    [Export] public Button RestartButton;
    [Export] public Button QuitButton;

    // initialization functions
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        
        if (RestartButton != null)
        {
            RestartButton.Pressed += OnRestartButtonPressed;
        }
        if (QuitButton != null)
        {
            QuitButton.Pressed += OnQuitButtonPressed;
        }
    }

    // ui functions
    public void SetupScreen(bool isWin, string reason)
    {
        GetTree().Paused = true;
        CallDeferred(MethodName.ForceMouseVisible);
        
        if (TitleLabel != null)
        {
            TitleLabel.Text = isWin ? "ESCAPED" : "DESOLATED";
            TitleLabel.Modulate = isWin ? new Color(0, 1, 0) : new Color(1, 0, 0);
        }
        
        if (ReasonLabel != null)
        {
            ReasonLabel.Text = reason;
        }
    }

    // input functions
    private void ForceMouseVisible()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    // signal functions
    private void OnRestartButtonPressed()
    {
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}