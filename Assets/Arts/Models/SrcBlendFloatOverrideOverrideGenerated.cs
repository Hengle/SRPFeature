using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Rendering
{
    [MaterialProperty("_SrcBlend", MaterialPropertyFormat.Float)]
    struct SrcBlendFloatOverride : IComponentData
    {
        public float Value;
    }
}
