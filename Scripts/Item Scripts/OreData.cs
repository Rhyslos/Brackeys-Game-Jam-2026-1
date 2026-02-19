using Godot;

[GlobalClass]
public partial class OreData : Resource
{
    [Export] public string OreName = "Unknown Ore";
    [Export] public Color OreColor = new Color(1, 1, 1);
    [Export] public float Value = 10.0f;
    [Export] public Mesh OreMesh;
    [Export] public Vector3 VisualScale = new Vector3(1, 1, 1);
}