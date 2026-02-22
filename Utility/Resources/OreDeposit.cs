using Godot;
using Godot.Collections;

public partial class OreDeposit : StaticBody3D
{
    // exported variables
    [ExportCategory("Data")]
    [Export] public OreData DepositData;
    [Export] public PackedScene DroppedOreScene;

    [ExportCategory("Stats")]
    [Export] public float MaxHealth = 100.0f;
    [Export] public int TotalDrops = 5;

    [Export] public float minRange = 1.0f;
    [Export] public float maxRange = 5.0f;

    // state variables
    private float _currentHealth;
    private int _dropsRemaining;
    private Array<Node> _spawnPoints;

    private ProgressBar _healthBar;
    private Sprite3D _healthBarSprite;
    private Timer _hideTimer;
    private AudioStreamPlayer3D _ambientAudioPlayer;

    // initialization functions
    public override void _Ready()
    {
        if (DepositData != null)
        {
            MaxHealth = DepositData.MaxHealth;
        }

        _currentHealth = MaxHealth;
        _dropsRemaining = TotalDrops;

        _healthBar = GetNode<ProgressBar>("Sprite3D/SubViewport/ProgressBar");
        _healthBarSprite = GetNode<Sprite3D>("Sprite3D");
        _hideTimer = GetNode<Timer>("Timer");

        _healthBarSprite.Visible = false;
        _healthBar.MaxValue = MaxHealth;
        _healthBar.Value = _currentHealth;

        _hideTimer.Timeout += OnHideTimerTimeout;

        _spawnPoints = GetNode("SpawnPoints").GetChildren();

        if (DepositData != null && DepositData.OreMesh != null)
        {
            foreach (Node node in _spawnPoints)
            {
                if (node is Marker3D marker)
                {
                    MeshInstance3D visualMesh = new MeshInstance3D();
                    visualMesh.Mesh = DepositData.OreMesh;
                    visualMesh.Scale = DepositData.VisualScale;

                    if (DepositData.OreMaterial != null)
                    {
                        visualMesh.SetSurfaceOverrideMaterial(0, DepositData.OreMaterial);
                    }

                    marker.AddChild(visualMesh);
                }
            }
        }

        if (DepositData != null && DepositData.AmbientSound != null)
        {
            _ambientAudioPlayer = new AudioStreamPlayer3D();
            _ambientAudioPlayer.Stream = DepositData.AmbientSound;
            _ambientAudioPlayer.PitchScale = 4.0f;
            _ambientAudioPlayer.MaxDistance = 8.0f;
            _ambientAudioPlayer.Autoplay = true;
            AddChild(_ambientAudioPlayer);
        }
    }

    // gameplay functions
    public void Mine(float damage)
    {
        if (_currentHealth <= 0) return;

        _currentHealth -= damage;
        _healthBar.Value = _currentHealth;
        _healthBarSprite.Visible = true;
        _hideTimer.Start();

        float healthPerDrop = MaxHealth / TotalDrops;
        int expectedDropsRemaining = Mathf.CeilToInt(_currentHealth / healthPerDrop);

        if (_dropsRemaining > expectedDropsRemaining)
        {
            int toSpawn = _dropsRemaining - expectedDropsRemaining;
            SpawnOreDrops(toSpawn);
            _dropsRemaining = expectedDropsRemaining;
        }

        if (_currentHealth <= 0)
        {
            GetNode<CollisionShape3D>("CollisionShape3D").SetDeferred(CollisionShape3D.PropertyName.Disabled, true);
            
            _healthBarSprite.Visible = false;
            _hideTimer.Stop();

            if (_ambientAudioPlayer != null && _ambientAudioPlayer.Playing)
            {
                _ambientAudioPlayer.Stop();
            }
        }
    }

    // spawner functions
    private void SpawnOreDrops(int amount)
    {
        bool hasAssignedDropAudio = false;

        for (int i = 0; i < amount; i++)
        {
            if (_spawnPoints.Count == 0) 
            {
                GD.PrintErr($"{Name}: Not enough Marker3Ds to spawn all drops!");
                return;
            }

            Marker3D spawnPoint = (Marker3D)_spawnPoints[0];
            _spawnPoints.RemoveAt(0);

            if (DroppedOreScene == null)
            {
                GD.PrintErr($"{Name}: DroppedOreScene is missing in Inspector!");
                return;
            }

            float dropForce = (float)GD.RandRange(minRange, maxRange);

            var drop = DroppedOreScene.Instantiate<DroppedResource>();
            drop.Data = DepositData;

            GetTree().CurrentScene.AddChild(drop);

            if (!hasAssignedDropAudio && DepositData != null && DepositData.AmbientSound != null)
            {
                AudioStreamPlayer3D dropAudio = new AudioStreamPlayer3D();
                dropAudio.Stream = DepositData.AmbientSound;
                dropAudio.PitchScale = 4.0f;
                dropAudio.MaxDistance = 8.0f;
                dropAudio.Autoplay = true;
                drop.AddChild(dropAudio);
                
                hasAssignedDropAudio = true;
            }

            drop.GlobalPosition = spawnPoint.GlobalPosition;
            Vector3 popForce = spawnPoint.GlobalBasis.Y * dropForce;
            drop.ApplyCentralImpulse(popForce);

            spawnPoint.QueueFree();
        }
    }

    // timer functions
    private void OnHideTimerTimeout()
    {
        _healthBarSprite.Visible = false;
    }
}