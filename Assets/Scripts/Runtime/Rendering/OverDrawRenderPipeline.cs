using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace Game.Runtime
{
    public sealed class OverDrawRenderPipeline : RenderPipeline
    {
        private readonly Shader _overDrawShader;
        private Material _overDrawMaterial;

        private ScriptableCullingParameters _cullingParams;

        private readonly List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("LightweightForward")
        };

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

            cameras.Sort((camera1, camera2) => (int)camera1.depth - (int)camera2.depth);

            BeginContextRendering(context, cameras);

            foreach (var camera in cameras)
            {
                BeginCameraRendering(context, camera);

                if (!camera.TryGetCullingParameters(false, out _cullingParams))
                {
                    continue;
                }

                var cullingResults = context.Cull(ref _cullingParams);
                context.SetupCameraProperties(camera);

                //camera clear flag
                var cmd = CommandBufferPool.Get();
                var clearFlags = camera.clearFlags;
                var drawSkyBox = clearFlags == CameraClearFlags.Skybox;
                var clearDepth = clearFlags != CameraClearFlags.Nothing;
                var clearColor = clearFlags == CameraClearFlags.Color;
                cmd.ClearRenderTarget(clearDepth, clearColor, camera.backgroundColor);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
                
                // draw opaque
                var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
                var filteringSettings = new FilteringSettings(RenderQueueRange.opaque, camera.cullingMask);
                var renderStateBlock = new RenderStateBlock { depthState = new DepthState(true) };
                var drawingSettings = new DrawingSettings(_shaderTagIdList[0], sortingSettings);
                for (var i = 1; i < _shaderTagIdList.Count; i++)
                {
                    drawingSettings.SetShaderPassName(i, _shaderTagIdList[i]);
                }
                if (camera.cameraType == CameraType.Game)  // only game view
                {
                    drawingSettings.overrideMaterial = _overDrawMaterial;
                }
                context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);

                if (drawSkyBox)
                {
                    context.DrawSkybox(camera);
                }
                
                // draw transparent
                sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonTransparent };
                filteringSettings = new FilteringSettings(RenderQueueRange.transparent, camera.cullingMask);
                renderStateBlock = new RenderStateBlock { depthState = new DepthState(false) };
                drawingSettings = new DrawingSettings(_shaderTagIdList[0], sortingSettings);
                for (var i = 1; i < _shaderTagIdList.Count; i++)
                {
                    drawingSettings.SetShaderPassName(i, _shaderTagIdList[i]);
                }
                if (camera.cameraType == CameraType.Game)  // only game view
                {
                    drawingSettings.overrideMaterial = _overDrawMaterial;
                }
                context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);
                
                context.Submit();
                
                EndCameraRendering(context, camera);
            }

            EndContextRendering(context, cameras);
        }
    }
}