namespace SteadyLearn.Domain.Entities;

/// <summary>
/// User roles in the SteadyLearn system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Administrator - can create and manage courses.
    /// </summary>
    Admin = 0,

    /// <summary>
    /// Student - can view and take courses.
    /// </summary>
    Student = 1
}
