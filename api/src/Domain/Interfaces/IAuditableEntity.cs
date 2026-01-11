namespace SteadyLearn.Domain.Interfaces;

/// <summary>
/// Interface for entities that should track audit information.
/// Automatically populated by the database context.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    DateTime? DeletedAt { get; set; }
    bool IsDeleted { get; set; }
}
