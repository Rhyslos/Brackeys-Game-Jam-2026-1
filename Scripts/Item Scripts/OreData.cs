using Godot;

[GlobalClass]
public partial class OreData : Resource
{
    [Export] public string OreName = "Unknown Ore";
    [Export] public Material OreMaterial;
    [Export] public float MaxHealth = 100.0f;
    [Export] public Mesh OreMesh;
    [Export] public AudioStream AmbientSound;
    [Export] public Vector3 VisualScale = new Vector3(1, 1, 1);
}