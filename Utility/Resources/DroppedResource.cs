using Godot;
using System.Collections.Generic;

public partial class DroppedResource : RigidBody3D, IInteractable
{
    // data variables
    public OreData Data;
    private static HashSet<string> _activeAudioTypes = new HashSet<string>();

    // initialization functions
    public override void _Ready()
    {
        if (Data != null)
        {
            ApplyOreData();
            HandleAmbientAudio();
        }
    }

    // audio functions
    private void HandleAmbientAudio()
    {
        if (Data.AmbientSound == null || _activeAudioTypes.Contains(Data.OreName)) 
            return;

        var audioPlayer = GetNodeOrNull<AudioStreamPlayer3D>("AmbientPlayer");
        if (audioPlayer == null) return;

        audioPlayer.Stream = Data.AmbientSound;
        audioPlayer.Play();
        _activeAudioTypes.Add(Data.OreName);

        TreeExiting += () => _activeAudioTypes.Remove(Data.OreName);
    }

    // visual functions
    private void ApplyOreData()
    {
        var meshInstance = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
        if (meshInstance == null) return;

        meshInstance.Mesh = Data.OreMesh;
        meshInstance.Scale = Data.VisualScale;

        if (Data.OreMaterial != null)
            meshInstance.SetSurfaceOverrideMaterial(0, Data.OreMaterial);
            
        var colShape = GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
        if (colShape != null && meshInstance.Mesh != null)
            colShape.Shape = meshInstance.Mesh.CreateConvexShape();

        Name = $"Dropped_{Data.OreName}";
    }

    // interaction functions
    public string GetInteractionText()
    {
        return Data != null ? $"Pickup {Data.OreName}" : "Pickup";
    }

    public void Interact(Node interactor)
    {
        if (interactor.HasMethod("AddInventoryItem"))
        {
            interactor.Call("AddInventoryItem", Data.OreName, 1);
            QueueFree();
        }
    }
}