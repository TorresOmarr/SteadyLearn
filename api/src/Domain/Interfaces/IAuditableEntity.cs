namespace SteadyLearn.Domain.Interfaces;

/// <summary>
/// Interface for entities that should track audit information.
/// Automatically populated by the database context.
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset? UpdatedAt { get; }
    DateTimeOffset? DeletedAt { get; }
}
