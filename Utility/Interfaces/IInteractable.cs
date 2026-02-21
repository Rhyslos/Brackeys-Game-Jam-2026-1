using Godot;

public interface IInteractable
{
    // interaction functions
    string GetInteractionText();
    void Interact(Node interactor);
}