using Godot;
using System.Collections.Generic;
using System;

public partial class InventoryManager : Control
{
    // ui variables
    [Export] public VBoxContainer ItemsColumn;
    [Export] public Button CraftRepairPartButton;
    [Export] public Button CraftFuelRodButton;
    [Export] public Button CraftOxygenTankButton;
    [Export] public Button ConsumeOxygenButton; 

    // references variables
    [Export] public Player PlayerNode;

    // data variables
    private Dictionary<string, int> _inventory = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    // initialization functions
    public override void _Ready()
    {
        if (CraftRepairPartButton != null)
        {
            CraftRepairPartButton.Pressed += CraftRepairPart;
        }
        
        if (CraftFuelRodButton != null)
        {
            CraftFuelRodButton.Pressed += CraftFuelRod;
        }
        
        if (CraftOxygenTankButton != null)
        {
            CraftOxygenTankButton.Pressed += CraftOxygenTank;
        }
        
        if (ConsumeOxygenButton != null)
        {
            ConsumeOxygenButton.Pressed += ConsumeOxygen;
        }

        UpdateUI();
    }

    // inventory functions
    public void AddItem(string itemName, int amount)
    {
        if (_inventory.ContainsKey(itemName))
        {
            _inventory[itemName] += amount;
        }
        else
        {
            _inventory[itemName] = amount;
        }
        
        UpdateUI();
        CheckWinCondition();
    }

    public void RemoveItem(string itemName, int amount)
    {
        if (_inventory.ContainsKey(itemName))
        {
            _inventory[itemName] -= amount;
            if (_inventory[itemName] <= 0)
            {
                _inventory.Remove(itemName);
            }
        }
    }

    private bool HasItems(string item1, int amount1, string item2, int amount2)
    {
        int count1 = _inventory.ContainsKey(item1) ? _inventory[item1] : 0;
        int count2 = _inventory.ContainsKey(item2) ? _inventory[item2] : 0;
        return count1 >= amount1 && count2 >= amount2;
    }

    private bool HasItems(string item1, int amount1, string item2, int amount2, string item3, int amount3)
    {
        int count1 = _inventory.ContainsKey(item1) ? _inventory[item1] : 0;
        int count2 = _inventory.ContainsKey(item2) ? _inventory[item2] : 0;
        int count3 = _inventory.ContainsKey(item3) ? _inventory[item3] : 0;
        return count1 >= amount1 && count2 >= amount2 && count3 >= amount3;
    }

    // crafting functions
    private void CraftRepairPart()
    {
        if (HasItems("Copper", 1, "Titanium", 1))
        {
            RemoveItem("Copper", 1);
            RemoveItem("Titanium", 1);
            AddItem("Repair Part", 1);
        }
    }

    private void CraftFuelRod()
    {
        if (HasItems("Uranium", 1, "Ice", 1))
        {
            RemoveItem("Uranium", 1);
            RemoveItem("Ice", 1);
            AddItem("Fuel Rod", 1);
        }
    }

    private void CraftOxygenTank()
    {
        if (HasItems("Ice", 1, "Hematite", 1, "Titanium", 1))
        {
            RemoveItem("Ice", 1);
            RemoveItem("Hematite", 1);
            RemoveItem("Titanium", 1);
            AddItem("Oxygen Tank", 1);
        }
    }

    private void ConsumeOxygen()
    {
        if (_inventory.ContainsKey("Oxygen Tank") && _inventory["Oxygen Tank"] > 0)
        {
            RemoveItem("Oxygen Tank", 1);
            if (PlayerNode != null)
            {
                PlayerNode.AddOxygen(30f);
            }
            UpdateUI();
        }
    }

    // ui functions
    private void UpdateUI()
    {
        if (ItemsColumn != null)
        {
            foreach (Node child in ItemsColumn.GetChildren())
            {
                child.QueueFree();
            }

            foreach (var kvp in _inventory)
            {
                Label itemLabel = new Label();
                itemLabel.Text = $"{kvp.Key}: {kvp.Value}";
                ItemsColumn.AddChild(itemLabel);
            }
        }

        if (CraftRepairPartButton != null)
        {
            CraftRepairPartButton.Disabled = !HasItems("Copper", 1, "Titanium", 1);
        }
        
        if (CraftFuelRodButton != null)
        {
            CraftFuelRodButton.Disabled = !HasItems("Uranium", 1, "Ice", 1);
        }
        
        if (CraftOxygenTankButton != null)
        {
            CraftOxygenTankButton.Disabled = !HasItems("Ice", 1, "Hematite", 1, "Titanium", 1);
        }
        
        if (ConsumeOxygenButton != null)
        {
            ConsumeOxygenButton.Disabled = !(_inventory.ContainsKey("Oxygen Tank") && _inventory["Oxygen Tank"] > 0);
        }
    }

    // game state functions
    private void CheckWinCondition()
    {
        int repairParts = _inventory.ContainsKey("Repair Part") ? _inventory["Repair Part"] : 0;
        int fuelRods = _inventory.ContainsKey("Fuel Rod") ? _inventory["Fuel Rod"] : 0;

        if (repairParts >= 3 && fuelRods >= 3)
        {
            GD.Print("YOU WIN!");
        }
    }
}