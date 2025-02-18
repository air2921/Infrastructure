using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services.EntityFramework.Entity;

public abstract class EntityBase
{
    [Key]
    public virtual string Id { get; set; } = Guid.NewGuid().ToString();
}
