namespace Infrastructure.Services.MongoDatabase.Document;

/// <summary>
/// An abstract base class for all entities stored in a MongoDB collection.
/// This class provides a common structure for documents, including a unique identifier
/// and a mechanism to specify the collection name for each derived document type.
/// </summary>
/// <remarks>
/// All entities that should be stored in MongoDB must inherit from this class
/// and implement the <see cref="CollectionName"/> property to specify the collection name.
/// </remarks>
public abstract class DocumentBase
{
    /// <summary>
    /// Gets or sets the unique identifier for the document.
    /// The ID is automatically initialized with a new GUID as a string when the document is created.
    /// </summary>
    /// <example>
    /// <code>
    /// var document = new MyDocument();
    /// Console.WriteLine(document.Id); // Outputs a new GUID as a string.
    /// </code>
    /// </example>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the name of the MongoDB collection where the document is stored.
    /// This property must be implemented by derived classes to specify the collection name.
    /// </summary>
    /// <example>
    /// <code>
    /// public class MyDocument : DocumentBase
    /// {
    ///     public override string CollectionName => "MyCollection";
    /// }
    /// </code>
    /// </example>
    public abstract string CollectionName { get; }
}

