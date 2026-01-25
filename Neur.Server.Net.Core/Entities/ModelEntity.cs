using Neur.Server.Net.Core.Abstractions;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Records;

namespace Neur.Server.Net.Core.Entities;

public class ModelEntity : Entity {
    public string Name { get; set; }
    public string ModelName { get; set; }
    public string Context {get; set;} = string.Empty;
    public ModelType Type { get; set; }
    public string Version { get; set; }
    public ModelStatus Status { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; init; }
    
    private ModelEntity() {}

    public ModelEntity(string name, string modelName, string context, ModelType type, string version, 
        ModelStatus status, DateTime createdAt, DateTime? updatedAt = null) {
        Id = Guid.NewGuid();
        Name = name;
        ModelName = modelName;
        Context = context;
        Type = type;
        Version = version;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}