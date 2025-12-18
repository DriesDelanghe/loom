using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record AddFieldDefinitionCommand(
    Guid DataSchemaId,
    string Path,
    FieldType FieldType,
    ScalarType? ScalarType,
    Guid? ElementSchemaId,
    bool Required,
    string? Description
);


