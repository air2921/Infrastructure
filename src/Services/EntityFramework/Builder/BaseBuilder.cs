using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Entity;
using System.ComponentModel;

namespace Infrastructure.Services.EntityFramework.Builder;

/// <summary>
/// An abstract base class for building queries or commands with common functionality.
/// Provides shared functionality like timeout configuration and constraint management.
/// </summary>
/// <typeparam name="TBuilder">The type of the derived builder (for fluent chaining).</typeparam>
/// <typeparam name="TEntity">The type of the entity being built for.</typeparam>
public abstract class BaseBuilder<TBuilder, TEntity>
    where TBuilder : BaseBuilder<TBuilder, TEntity>
    where TEntity : EntityBase
{
    /// <summary>
    /// A flag indicating whether to ignore builder constraints.
    /// This should be used with caution as it bypasses safety checks.
    /// </summary>
    private bool _ignoreBuilderConstraints = false;

    /// <summary>
    /// Gets a value indicating whether builder constraints are currently being ignored.
    /// </summary>
    /// <value>
    /// <c>true</c> if builder constraints are ignored; otherwise, <c>false</c>.
    /// </value>
    protected bool IsIgnoredBuilderConstraints => _ignoreBuilderConstraints;

    /// <summary>
    /// Gets or sets the timeout duration for the operation execution.
    /// This property is internal and not intended for direct use outside the builder.
    /// </summary>
    /// <value>
    /// The timeout duration for the operation. Default is <see cref="TimeSpan.Zero"/> (no timeout).
    /// </value>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal TimeSpan Timeout { get; private set; } = TimeSpan.Zero;

    /// <summary>
    /// Disables builder constraints (like timeout limits).
    /// This method should be used with caution as it bypasses safety checks.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Marked as obsolete to warn against casual usage. Only use when absolutely necessary.
    /// </remarks>
    [Obsolete("Do not use disabling builder restrictions unless it is done intentionally")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public TBuilder WithIgnoreBuilderConstraints()
    {
        _ignoreBuilderConstraints = true;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the timeout duration for the operation execution.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="InvalidArgumentException">
    /// Thrown when:
    /// <list type="bullet">
    /// <item><description>Timeout is <see cref="TimeSpan.Zero"/></description></item>
    /// <item><description>Timeout is less than 5 seconds (unless constraints are ignored)</description></item>
    /// <item><description>Timeout is more than 3 minutes (unless constraints are ignored)</description></item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// The default constraints require timeouts between 5 seconds and 3 minutes.
    /// Use <see cref="WithIgnoreBuilderConstraints"/> to bypass these constraints when necessary.
    /// </remarks>
    public TBuilder WithTimeout(TimeSpan timeout)
    {
        if (timeout == TimeSpan.Zero)
            throw new InvalidArgumentException($"Using a {nameof(WithTimeout)} with {nameof(TimeSpan.Zero)} is not allowed");

        if (timeout < TimeSpan.FromSeconds(5) && !IsIgnoredBuilderConstraints)
            throw new InvalidArgumentException($"Using a {nameof(WithTimeout)} using timeout less than 5 seconds is not allowed");

        if (timeout >= TimeSpan.FromMinutes(3) && !IsIgnoredBuilderConstraints)
            throw new InvalidArgumentException($"Using a {nameof(WithTimeout)} using timeout more than 3 minutes is not allowed");

        Timeout = timeout;
        return (TBuilder)this;
    }
}
