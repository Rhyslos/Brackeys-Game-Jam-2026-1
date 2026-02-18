using Godot;
using System;

[GlobalClass]
public partial class CraftingResource : Resource
{
    [Export] public new string ResourceName { get; set; }
    [Export] public string ResourceID { get; set; }
}
