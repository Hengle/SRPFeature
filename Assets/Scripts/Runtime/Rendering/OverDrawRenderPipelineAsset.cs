using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Runtime
{
    [ExcludeFromPreset]
    public sealed class OverDrawRenderPipelineAsset : RenderPipelineAsset
    {
        public Shader OverDrawShader;
            
        protected override RenderPipeline CreatePipeline()
        {
            return new OverDrawRenderPipeline(OverDrawShader);
        }
    }
}
