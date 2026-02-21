using Godot;
using System.Collections.Generic;

public partial class AmbientBubbles : Node3D
{
    // exported variables
    [Export] public AudioStream BubbleSound;
    [Export] public int PoolSize = 8;
    [Export] public float MaxVolumeDb = 0.0f;
    [Export] public float MinVolumeDb = -60.0f;
    [Export(PropertyHint.Range, "0.0, 1.0")] public float FadeThreshold = 0.8f;
    [Export] public float DefaultZoneRadius = 15.0f;

    [ExportCategory("Playback Dynamics")]
    [Export] public float MinSpawnTime = 0.05f;
    [Export] public float MaxSpawnTime = 0.35f;
    [Export] public float MinPitch = 0.7f;
    [Export] public float MaxPitch = 1.3f;
    [Export(PropertyHint.Range, "0.0, 1.0")] public float EchoChance = 0.3f;
    [Export] public float EchoDelay = 0.12f;
    [Export] public float EchoVolumeOffset = -6.0f;

    // state variables
    private List<Area3D> _activeZones = new List<Area3D>();
    private Node3D _player;
    private List<AudioStreamPlayer> _audioPool = new List<AudioStreamPlayer>();
    private float _spawnTimer;
    private float _currentSpawnTarget;
    private float _currentMasterVolume;

    // initialization functions
    public override void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is Area3D area)
            {
                area.BodyEntered += (body) => OnBodyEntered(body, area);
                area.BodyExited += (body) => OnBodyExited(body, area);
            }
        }

        for (int i = 0; i < PoolSize; i++)
        {
            AudioStreamPlayer player = new AudioStreamPlayer();
            player.Stream = BubbleSound;
            AddChild(player);
            _audioPool.Add(player);
        }

        _currentMasterVolume = MinVolumeDb;
        SetNextSpawnTime();
    }

    // loop functions
    public override void _Process(double delta)
    {
        UpdateMasterVolume(delta);

        if (_currentMasterVolume > MinVolumeDb + 5.0f)
        {
            _spawnTimer += (float)delta;
            if (_spawnTimer >= _currentSpawnTarget)
            {
                _spawnTimer = 0f;
                SetNextSpawnTime();
                PlayDynamicBubble();
            }
        }
    }

    // audio functions
    private void UpdateMasterVolume(double delta)
    {
        if (_player == null || _activeZones.Count == 0)
        {
            _currentMasterVolume = Mathf.Lerp(_currentMasterVolume, MinVolumeDb, (float)delta * 5f);
            return;
        }

        float closestRatio = 1.0f;

        foreach (Area3D zone in _activeZones)
        {
            float radius = DefaultZoneRadius;
            
            foreach (Node zoneChild in zone.GetChildren())
            {
                if (zoneChild is CollisionShape3D shape && shape.Shape != null)
                {
                    if (shape.Shape is SphereShape3D sphere) 
                    {
                        radius = sphere.Radius;
                    }
                    else if (shape.Shape is CylinderShape3D cyl) 
                    {
                        radius = cyl.Radius;
                    }
                }
            }

            float distance = _player.GlobalPosition.DistanceTo(zone.GlobalPosition);
            float ratio = distance / radius;
            
            if (ratio < closestRatio)
            {
                closestRatio = ratio;
            }
        }

        float targetVolume = MinVolumeDb;
        
        if (closestRatio <= FadeThreshold)
        {
            targetVolume = MaxVolumeDb;
        }
        else if (closestRatio <= 1.0f)
        {
            float fadeRatio = Mathf.InverseLerp(FadeThreshold, 1.0f, closestRatio);
            targetVolume = Mathf.Lerp(MaxVolumeDb, MinVolumeDb, fadeRatio);
        }

        _currentMasterVolume = Mathf.Lerp(_currentMasterVolume, targetVolume, (float)delta * 10f);
    }

    private void PlayDynamicBubble(bool isEcho = false, float forcedPitch = 1.0f)
    {
        AudioStreamPlayer availablePlayer = null;
        foreach (AudioStreamPlayer player in _audioPool)
        {
            if (!player.Playing)
            {
                availablePlayer = player;
                break;
            }
        }

        if (availablePlayer == null) return;

        float pitch = isEcho ? forcedPitch : (float)GD.RandRange(MinPitch, MaxPitch);
        float volume = isEcho ? _currentMasterVolume + EchoVolumeOffset : _currentMasterVolume;

        availablePlayer.PitchScale = pitch;
        availablePlayer.VolumeDb = volume;
        availablePlayer.Play();

        if (!isEcho && GD.Randf() <= EchoChance)
        {
            GetTree().CreateTimer(EchoDelay).Timeout += () => PlayDynamicBubble(true, pitch);
        }
    }

    // state functions
    private void SetNextSpawnTime()
    {
        _currentSpawnTarget = (float)GD.RandRange(MinSpawnTime, MaxSpawnTime);
    }

    // signal functions
    private void OnBodyEntered(Node3D body, Area3D area)
    {
        if (body.Name == "Player" || body.IsInGroup("player"))
        {
            _player = body;
            if (!_activeZones.Contains(area))
            {
                _activeZones.Add(area);
            }
        }
    }

    private void OnBodyExited(Node3D body, Area3D area)
    {
        if (body.Name == "Player" || body.IsInGroup("player"))
        {
            if (_activeZones.Contains(area))
            {
                _activeZones.Remove(area);
            }
            
            if (_activeZones.Count == 0)
            {
                _player = null;
            }
        }
    }
}