namespace CUInventory.Abstractions;

public interface IIsActive
{
    bool IsActive { get; }
    public void Activate();
    public void Deactivate();
    public void ToggleIsActive();
}
