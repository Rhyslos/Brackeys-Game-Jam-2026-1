using Godot;

[GlobalClass]
public partial class ItemData : Resource
{
    [ExportGroup("Visuals")]
    [Export] public string DisplayName { get; set; } = "New Item";
    [Export] public Texture2D Icon { get; set; }

    [ExportGroup("Identity")]
    [Export] public string ItemID { get; set; } = "";

    [ExportGroup("Stats")]
    [Export] public string Description { get; set; } = "";
    
    [Export] public bool IsConsumable { get; set; } = false;
}