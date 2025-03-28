using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Services.MongoDatabase.Document;

/// <summary>
/// An abstract base class for all entities stored in a MongoDB collection.
/// Provides common document structure with audit fields and collection name specification.
/// </summary>
/// <remarks>
/// All MongoDB entities should inherit from this class and implement:
/// 1. CollectionName - to specify MongoDB collection name
/// 2. Any additional entity-specific properties
/// </remarks>
public abstract class DocumentBase
{
    /// <summary>
    /// Unique document identifier (Primary Key)
    /// Automatically initialized with new ObjectId converted to string
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>
    /// Document creation timestamp in UTC
    /// </summary>
    [BsonElement("created_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Document last modification timestamp in UTC
    /// Null if never modified
    /// </summary>
    [BsonElement("updated_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonIgnoreIfNull]
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Name of the MongoDB collection where documents of this type are stored
    /// Must be implemented by derived classes
    /// </summary>
    [BsonIgnore]
    public abstract string CollectionName { get; }
}

