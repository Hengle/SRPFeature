using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace Game.Runtime
{
    public sealed class OverDrawRenderPipeline : RenderPipeline
    {
        private readonly Shader _overDrawShader;
        private Material _overDrawMaterial;

        public OverDrawRenderPipeline(Shader shader)
        {
            _overDrawShader = shader;
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            Render(context, new List<Camera>(cameras));
        }

        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            if (_overDrawMaterial == null)
            {
                if (_overDrawShader != null)
                {
                    _overDrawMaterial = new Material(_overDrawShader);
                }
            }

            if (_overDrawMaterial == null)
            {
                return;
            }

            BeginContextRendering(context, cameras);

            foreach (var camera in cameras)
            {
                
            }
            
            EndContextRendering(context, cameras);
        }
    }
}
