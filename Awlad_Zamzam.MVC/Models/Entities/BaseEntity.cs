namespace Awlad_Zamzam.MVC.Models.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; protected set; }

    public DateTime? UpdatedAt { get; protected set; }
}