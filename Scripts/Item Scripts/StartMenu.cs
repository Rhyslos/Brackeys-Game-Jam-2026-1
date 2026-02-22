using Godot;

public partial class StartMenu : CanvasLayer
{
    // exported variables
    [Export] public Button StartButton;

    // initialization functions
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        GetTree().Paused = true;
        
        CallDeferred(MethodName.ForceMouseVisible);

        if (StartButton != null)
        {
            StartButton.Pressed += OnStartButtonPressed;
        }
    }

    // input functions
    private void ForceMouseVisible()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    // signal functions
    private void OnStartButtonPressed()
    {
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
        QueueFree();
    }
}