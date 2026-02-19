using Godot;
using Godot.Collections;

public partial class OreDeposit : StaticBody3D
{
    [ExportCategory("Data")]
    [Export] public OreData DepositData;
    [Export] public PackedScene DroppedOreScene;

    [ExportCategory("Stats")]
    [Export] public float MaxHealth = 100.0f;
    [Export] public int TotalDrops = 5;

    [Export] public float minRange = 1.0f;
    [Export] public float maxRange = 5.0f;

    private float _currentHealth;
    private int _dropsRemaining;
    private Array<Node> _spawnPoints;

    private ProgressBar _healthBar;
    private Sprite3D _healthBarSprite;
    private Timer _hideTimer;

    public override void _Ready()
    {
        if (DepositData != null)
        {
            MaxHealth = DepositData.MaxHealth;
        }

        _currentHealth = MaxHealth;
        _dropsRemaining = TotalDrops;

        // Setup UI
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
    }

    public void Mine(float damage)
    {
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
            GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;

            GetNode<MeshInstance3D>("MeshInstance3D").Visible = false;
            _healthBarSprite.Visible = false;

            var timer = GetTree().CreateTimer(0.1f);
            timer.Timeout += () => QueueFree();
        }
    }

    private void SpawnOreDrops(int amount)
    {
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

            drop.GlobalPosition = spawnPoint.GlobalPosition;
            Vector3 popForce = spawnPoint.GlobalBasis.Y * dropForce;
            GD.Print(dropForce);
            drop.ApplyCentralImpulse(popForce);

            spawnPoint.QueueFree();
        }
    }

    private void OnHideTimerTimeout()
    {
        _healthBarSprite.Visible = false;
    }
}