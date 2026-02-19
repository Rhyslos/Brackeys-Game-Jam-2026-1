using Godot;

public partial class DroppedResource : RigidBody3D
{
    public OreData Data;

    public override void _Ready()
    {
        if (Data != null)
        {
            ApplyOreData();
        }
    }

    private void ApplyOreData()
    {
        var meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");

        if (meshInstance != null)
        {
            meshInstance.Mesh = Data.OreMesh;

            meshInstance.Scale = Data.VisualScale;

            StandardMaterial3D material = new StandardMaterial3D();
            material.AlbedoColor = Data.OreColor;
            
            meshInstance.SetSurfaceOverrideMaterial(0, material);
        }

        Name = $"Dropped_{Data.OreName}";
    }
}