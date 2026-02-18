using Godot;
using System;

public enum ItemCategory
{
	CraftingResource,
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

	private ItemCategory _category;
	[Export]
	public ItemCategory Category
	{
		get => _category;
		set
		{
			if (_category == value) return;
			_category = value;
			NotifyPropertyListChanged();
		}
	}

	[Export] public CraftingResource PrimaryCraftingResource { get; set; }
	[Export] public CraftingResource SecondaryCraftingResource { get; set; }

	[Export] public string DoorId { get; set; } = "";
	[Export] public bool IsWinningPart { get; set; } = false;

	[Export] public float HealthGain { get; set; }
	[Export] public float StaminaGain { get; set; }

	[Export] public float EffectDuration { get; set; }
	[Export] public bool IsRegenBoost { get; set; }

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		string name = property["name"].AsStringName();

		if ((name == nameof(PrimaryCraftingResource) || name == nameof(SecondaryCraftingResource)) && Category != ItemCategory.CraftingResource)
		{
			HideProperty(property);
		}

		if (name == nameof(DoorId) && Category != ItemCategory.CraftingResource)
		{
			HideProperty(property);
		}

		if (name == nameof(IsWinningPart) && Category != ItemCategory.RepairItem)
		{
			HideProperty(property);
		}

		if (Category != ItemCategory.Consumable)
		{
			if (name == nameof(HealthGain) || name == nameof(StaminaGain) || 
				name == nameof(EffectDuration) || name == nameof(IsRegenBoost))
			{
				HideProperty(property);
			}
		}
	}

	private void HideProperty(Godot.Collections.Dictionary property)
	{
		property["usage"] = (int)(PropertyUsageFlags.NoEditor | PropertyUsageFlags.Internal);
	}
}
