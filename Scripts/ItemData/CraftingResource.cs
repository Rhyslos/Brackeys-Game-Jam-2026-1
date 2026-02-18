using Godot;
using System;

[GlobalClass]
public partial class CraftingResource : Resource
{
    [Export] public string ResourceName { get; set; }
    [Export] public string ResourceID { get; set; }
}
