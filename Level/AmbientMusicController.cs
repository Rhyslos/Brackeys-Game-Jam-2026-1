using Godot;

public partial class AmbientMusicController : Node
{
    // exported variables
    [Export] public Godot.Collections.Array<AudioStream> SuspenseTracks;
    [Export] public float MinDelay = 30.0f;
    [Export] public float MaxDelay = 90.0f;

    // state variables
    private AudioStreamPlayer _audioPlayer;
    private Timer _delayTimer;

    // initialization functions
    public override void _Ready()
    {
        _audioPlayer = new AudioStreamPlayer();
        _audioPlayer.Finished += OnTrackFinished;
        AddChild(_audioPlayer);

        _delayTimer = new Timer();
        _delayTimer.OneShot = true;
        _delayTimer.Timeout += PlayRandomTrack;
        AddChild(_delayTimer);

        StartRandomDelay();
    }

    // audio functions
    private void PlayRandomTrack()
    {
        if (SuspenseTracks == null || SuspenseTracks.Count == 0) return;

        int randomIndex = GD.RandRange(0, SuspenseTracks.Count - 1);
        _audioPlayer.Stream = SuspenseTracks[randomIndex];
        _audioPlayer.Play();
    }

    // timer functions
    private void StartRandomDelay()
    {
        float delay = (float)GD.RandRange(MinDelay, MaxDelay);
        _delayTimer.Start(delay);
    }

    // signal functions
    private void OnTrackFinished()
    {
        StartRandomDelay();
    }
}