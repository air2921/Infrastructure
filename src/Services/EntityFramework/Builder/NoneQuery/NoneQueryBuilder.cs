using System.ComponentModel;

namespace Infrastructure.Services.EntityFramework.Builder.NoneQuery;

/// <summary>
/// Abstract base class for command builders that perform database operations without returning query results.
/// Serves as the foundation for create, update, and delete operation builders.
/// </summary>
public abstract class NoneQueryBuilder
{
    /// <summary>
    /// Gets whether changes should be immediately persisted to the database.
    /// When true, changes are saved immediately; when false, changes are tracked but not persisted until explicitly saved.
    /// </summary>
    /// <remarks>
    /// This property is internal and intended for framework use only.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool SaveChanges { get; private set; }

    /// <summary>
    /// Configures whether the operation should persist changes to the database immediately.
    /// </summary>
    /// <param name="save">
    /// If true, changes will be saved immediately (default behavior for most operations).
    /// If false, changes will be tracked but not persisted until an explicit save is performed.
    /// </param>
    /// <returns>The current builder instance for fluent chaining.</returns>
    public NoneQueryBuilder WithSaveChanges(bool save)
    {
        SaveChanges = save;
        return this;
    }
}
