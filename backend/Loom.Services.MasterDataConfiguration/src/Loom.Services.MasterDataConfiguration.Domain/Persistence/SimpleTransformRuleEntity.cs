using Loom.Services.MasterDataConfiguration.Domain.Transformation;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class SimpleTransformRuleEntity
{
    public Guid Id { get; set; }
    public Guid TransformationSpecId { get; set; }
    public string SourcePath { get; set; } = default!;
    public string TargetPath { get; set; } = default!;
    public Guid? ConverterId { get; set; }
    public bool Required { get; set; }
    public int Order { get; set; }

    public TransformationSpecEntity TransformationSpec { get; set; } = null!;

    public SimpleTransformRule ToDomain()
    {
        return new SimpleTransformRule
        {
            Id = Id,
            TransformationSpecId = TransformationSpecId,
            SourcePath = SourcePath,
            TargetPath = TargetPath,
            ConverterId = ConverterId,
            Required = Required,
            Order = Order
        };
    }

    public static SimpleTransformRuleEntity FromDomain(SimpleTransformRule rule)
    {
        return new SimpleTransformRuleEntity
        {
            Id = rule.Id,
            TransformationSpecId = rule.TransformationSpecId,
            SourcePath = rule.SourcePath,
            TargetPath = rule.TargetPath,
            ConverterId = rule.ConverterId,
            Required = rule.Required,
            Order = rule.Order
        };
    }
}
