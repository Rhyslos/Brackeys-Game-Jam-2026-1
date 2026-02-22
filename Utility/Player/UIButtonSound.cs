using Godot;

public partial class UIButtonSound : Node
{
    // exported variables
    [Export] public AudioStream HoverSound;
    [Export] public AudioStream ClickSound;

    // state variables
    private AudioStreamPlayer _audioPlayer;

    // initialization functions
    public override void _Ready()
    {
        _audioPlayer = new AudioStreamPlayer();
        AddChild(_audioPlayer);

        if (GetParent() is Button button)
        {
            button.Pressed += OnButtonPressed;
            button.MouseEntered += OnMouseEntered;
        }
    }

    // audio functions
    private void PlaySound(AudioStream stream)
    {
        if (stream == null) return;
        _audioPlayer.Stream = stream;
        _audioPlayer.Play();
    }

    // signal functions
    private void OnButtonPressed()
    {
        PlaySound(ClickSound);
    }

    private void OnMouseEntered()
    {
        PlaySound(HoverSound);
    }
}