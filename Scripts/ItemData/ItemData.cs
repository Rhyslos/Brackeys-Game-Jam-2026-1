using Godot;
using System;

public enum ItemCategory
{
	Key,
	RepairItem,
	Consumable,
}


[Tool]
[GlobalClass]
public partial class ItemData : Resource
{
	[Export] public string Name { get; set; } = "";
	[Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
	[Export] public Texture2D Icon { get; set; }


	[ExportCategory("Category")]
	[Export] public ItemCategory Category { get; set; }


	[ExportCategory("Item Stats")]
	private bool _isConsumable = false;
	[Export] public bool IsConsumable
    {
        get => _isConsumable;
        set
        {
            if (_isConsumable == value) return;
            
            _isConsumable = value;
            EmitChanged();
            NotifyPropertyListChanged(); 
        }
    }

	// Item Stats if consumable
	[Export] public float UseTime { get; set; } = 1f;
	
}
