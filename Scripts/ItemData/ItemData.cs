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
	[Export] public ItemCategory Category {get; set; }


	[ExportCategory("Item Stats")]
	private bool _IsConsumable;
	[Export] public bool IsConsumable
	{get => IsConsumable;
		set 
		{
			if (_IsConsumable == value) return;
			_IsConsumable = value;
			EmitChanged(); 
			NotifyPropertyListChanged(); // Refreshes the Inspector view
		}
	}	
}
