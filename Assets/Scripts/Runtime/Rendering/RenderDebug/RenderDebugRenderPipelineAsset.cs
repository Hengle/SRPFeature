using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Runtime
{
    [ExcludeFromPreset]
    public sealed class RenderDebugRenderPipelineAsset : RenderPipelineAsset
    {
        public List<string> ShaderPassName;
        public Shader OverDrawOpaqueShader;
        public Shader OverDrawTransparentShader;
        public Shader MipmapColorShader;
        public Shader MipmapTextureShader;
        
        protected override RenderPipeline CreatePipeline()
        {
            var fallbackShader = Shader.Find("Hidden/Universal Render Pipeline/FallbackError");
            var overDrawOpaque = new Material(OverDrawOpaqueShader == null ? fallbackShader : OverDrawOpaqueShader);
            var overDrawTransparent = new Material(OverDrawTransparentShader == null ? fallbackShader : OverDrawTransparentShader);
            var mipmapColor = new Material(MipmapColorShader == null ? fallbackShader : MipmapColorShader);
            var mipmapTexure = new Material(MipmapTextureShader == null ? fallbackShader : MipmapTextureShader);
            var renderPipeline = new RenderDebugRenderPipeline();
            renderPipeline.SetUp(ShaderPassName, overDrawOpaque, overDrawTransparent, mipmapColor, mipmapTexure);
            return renderPipeline;
        }
    }
}
