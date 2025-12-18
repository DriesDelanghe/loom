using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record UpdateFieldDefinitionCommand(
    Guid FieldDefinitionId,
    string? Path,
    FieldType? FieldType,
    ScalarType? ScalarType,
    Guid? ElementSchemaId,
    bool? Required,
    string? Description
);


