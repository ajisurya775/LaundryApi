using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Users;

public class MenuItem : Entity
{
    public string Key { get; private set; }
    public string DisplayName { get; private set; }
    public string Path { get; private set; }
    public string Icon { get; private set; }
    public Guid? ParentId { get; private set; }
    public int OrderNumber { get; private set; }
    public bool IsActive { get; private set; }

    public MenuItem(Guid id, string key, string displayName, string path, string icon, Guid? parentId, int orderNumber) : base(id)
    {
        Key = key;
        DisplayName = displayName;
        Path = path;
        Icon = icon;
        ParentId = parentId;
        OrderNumber = orderNumber;
        IsActive = true;
    }

    // EF Core Constructor
    private MenuItem()
    {
        Key = null!;
        DisplayName = null!;
        Path = null!;
        Icon = null!;
    }

    public void UpdateDetails(string displayName, string path, string icon, Guid? parentId, int orderNumber)
    {
        DisplayName = displayName;
        Path = path;
        Icon = icon;
        ParentId = parentId;
        OrderNumber = orderNumber;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
