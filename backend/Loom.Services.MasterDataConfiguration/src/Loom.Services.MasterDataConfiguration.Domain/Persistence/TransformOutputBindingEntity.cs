using Loom.Services.MasterDataConfiguration.Domain.Transformation;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class TransformOutputBindingEntity
{
    public Guid Id { get; set; }
    public Guid TransformationSpecId { get; set; }
    public string TargetPath { get; set; } = default!;
    public Guid FromNodeId { get; set; }

    public TransformationSpecEntity TransformationSpec { get; set; } = null!;

    public TransformOutputBinding ToDomain()
    {
        return new TransformOutputBinding
        {
            Id = Id,
            TransformationSpecId = TransformationSpecId,
            TargetPath = TargetPath,
            FromNodeId = FromNodeId
        };
    }

    public static TransformOutputBindingEntity FromDomain(TransformOutputBinding binding)
    {
        return new TransformOutputBindingEntity
        {
            Id = binding.Id,
            TransformationSpecId = binding.TransformationSpecId,
            TargetPath = binding.TargetPath,
            FromNodeId = binding.FromNodeId
        };
    }
}
