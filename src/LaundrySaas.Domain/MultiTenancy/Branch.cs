using LaundrySaas.SharedKernel.Interfaces;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.MultiTenancy;

public class Branch : Entity, IMustHaveTenant
{
    private readonly List<Pos> _posList = new();

    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string PhoneNumber { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<Pos> PosList => _posList.AsReadOnly();

    public Branch(Guid id, Guid tenantId, string name, string address, string phoneNumber) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Address = address;
        PhoneNumber = phoneNumber;
        IsActive = true;
    }

    // EF Core Constructor
    private Branch()
    {
        Name = null!;
        Address = null!;
        PhoneNumber = null!;
    }

    public void AddPos(Guid posId, string name)
    {
        if (!_posList.Any(p => p.Id == posId))
        {
            _posList.Add(new Pos(posId, TenantId, Id, name));
        }
    }

    public void UpdateDetails(string name, string address, string phoneNumber)
    {
        Name = name;
        Address = address;
        PhoneNumber = phoneNumber;
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
