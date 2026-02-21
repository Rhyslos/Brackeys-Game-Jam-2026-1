using Godot;

[Tool]
public partial class CraterTreeSpawner : Node3D
{
    // exported variables
    [ExportCategory("Spawner Settings")]
    [Export] public Mesh TreeMesh;
    [Export] public int TreeCount = 150;
    [Export] public float BaseRadius = 250.0f;
    [Export] public float RadiusVariation = 30.0f;
    [Export] public float HeightOffset = 80.0f;

    [ExportCategory("Tree Variations")]
    [Export] public float MinScale = 3.0f;
    [Export] public float MaxScale = 6.0f;

    [ExportCategory("Editor Actions")]
    [Export]
    public bool GenerateTrees
    {
        get => false;
        set
        {
            if (value) Generate();
        }
    }

    [Export]
    public bool ClearTrees
    {
        get => false;
        set
        {
            if (value) Clear();
        }
    }

    // generation functions
    private void Generate()
    {
        if (TreeMesh == null)
        {
            GD.PrintErr("TreeMesh is not assigned! Please assign a Mesh resource first.");
            return;
        }

        Clear();

        for (int i = 0; i < TreeCount; i++)
        {
            MeshInstance3D treeInstance = new MeshInstance3D();
            treeInstance.Mesh = TreeMesh;
            AddChild(treeInstance);

            float angle = (float)GD.RandRange(0, Mathf.Tau);
            float distance = BaseRadius + (float)GD.RandRange(-RadiusVariation, RadiusVariation);

            float x = Mathf.Cos(angle) * distance;
            float z = Mathf.Sin(angle) * distance;

            treeInstance.Position = new Vector3(x, HeightOffset, z);

            float randomYRotation = (float)GD.RandRange(0, Mathf.Tau);
            treeInstance.Rotation = new Vector3(0, randomYRotation, 0);

            float randomScale = (float)GD.RandRange(MinScale, MaxScale);
            treeInstance.Scale = new Vector3(randomScale, randomScale, randomScale);
            
            if (Engine.IsEditorHint())
            {
                treeInstance.Owner = GetTree().EditedSceneRoot;
            }
        }
        
        GD.Print($"Successfully generated {TreeCount} trees along the crater rim.");
    }

    // cleanup functions
    private void Clear()
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }
    }
}