using Neur.Server.Net.Core.Interfaces;
using Neur.Server.Net.Core.Records;

namespace Neur.Server.Net.Core.Entities;

public class ModelEntity {
    public Guid Id { get; init; }
    public string Name { get; set; }
    
    public string ModelName { get; set; }
    public ModelType Type { get; set; }
    public string Version { get; set; }
    public ModelStatus Status { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; init; }
    
    private ModelEntity() {}

    private ModelEntity(Guid id, string name, string modelName, ModelType type, string version, ModelStatus status, DateTime? updatedAt, DateTime createdAt) {
        Id = id;
        Name = name;
        ModelName = modelName;
        Type = type;
        Version = version;
        Status = status;
        UpdatedAt = updatedAt;
        CreatedAt = createdAt;
    }

    public static ModelEntity Create(Guid id, string name, string modelName, ModelType type, string version, ModelStatus status, DateTime createdAt) {
        return new ModelEntity(
            id,
            name,
            modelName,
            type,
            version,
            status,
            null,
            createdAt
        );
    }
}