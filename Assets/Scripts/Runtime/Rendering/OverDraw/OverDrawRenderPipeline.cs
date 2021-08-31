using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

namespace Game.Runtime
{
    public sealed class OverDrawRenderPipeline : RenderPipeline
    {
        private readonly Shader _overDrawShader;
        private Material _overDrawMaterial;

        private ScriptableCullingParameters _cullingParams;
        private readonly List<ShaderTagId> _shaderTagIdList;

        private ProfilingSampler _clearRenderTargetSampler;
        private ProfilingSampler _drawOpaqueSampler;
        private ProfilingSampler _drawSkyboxSampler;
        private ProfilingSampler _drawTransparentSampler;

        public OverDrawRenderPipeline(Shader shader)
        {
            _overDrawShader = shader;
            _shaderTagIdList = new List<ShaderTagId>()
            {
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("LightweightForward")
            };

            _clearRenderTargetSampler = new ProfilingSampler("Clear Render Target");
            _drawOpaqueSampler = new ProfilingSampler("Render Opaque");
            _drawSkyboxSampler = new ProfilingSampler("Render Skybox");
            _drawTransparentSampler = new ProfilingSampler("Render Transparent");
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

                var cameraRenderingSampler = new ProfilingSampler(camera.name);
                var profilingCMD = CommandBufferPool.Get(cameraRenderingSampler.name);
                cameraRenderingSampler.Begin(profilingCMD);
                context.ExecuteCommandBuffer(profilingCMD);
                profilingCMD.Clear();

                if (!camera.TryGetCullingParameters(false, out _cullingParams))
                {
                    continue;
                }

                var cullingResults = context.Cull(ref _cullingParams);
                context.SetupCameraProperties(camera);

                //render target clear flag
                {
                    var cmd = CommandBufferPool.Get(_clearRenderTargetSampler.name);
                    using (new ProfilingScope(cmd, _clearRenderTargetSampler))
                    {
                        context.ExecuteCommandBuffer(cmd);
                        cmd.Clear();
                        var clearDepth = true;
                        var clearColor = true;
#if UNIVERSAL_RENDER_PIPELINE
                        var additionalCameraData = camera.GetComponent<UniversalAdditionalCameraData>();
                        if (additionalCameraData.renderType == CameraRenderType.Overlay)
                        {
                            clearDepth = additionalCameraData.clearDepth;
                            clearColor = false;
                        }
#else
                        var clearFlags = camera.clearFlags;
                        clearDepth = clearFlags != CameraClearFlags.Nothing;
                        clearColor = clearFlags == CameraClearFlags.Color;
#endif
                        cmd.ClearRenderTarget(clearDepth, clearColor, camera.backgroundColor);
                    }

                    context.ExecuteCommandBuffer(cmd);
                    CommandBufferPool.Release(cmd);
                }

                // render state
                var sortingSettings = new SortingSettings(camera);
                var filteringSettings = new FilteringSettings();
                var renderStateBlock = new RenderStateBlock();
                var drawingSettings = new DrawingSettings();
                filteringSettings.layerMask = camera.cullingMask;
                filteringSettings.sortingLayerRange = SortingLayerRange.all;
                for (var i = 0; i < _shaderTagIdList.Count; i++)
                {
                    drawingSettings.SetShaderPassName(i, _shaderTagIdList[i]);
                }

                if (camera.cameraType == CameraType.Game)
                {
                    drawingSettings.overrideMaterial = _overDrawMaterial;
                }

                // draw opaque
                {
                    var cmd = CommandBufferPool.Get(_drawOpaqueSampler.name);
                    using (new ProfilingScope(cmd, _drawOpaqueSampler))
                    {
                        context.ExecuteCommandBuffer(cmd);
                        cmd.Clear();
                        sortingSettings.criteria = SortingCriteria.CommonOpaque;
                        filteringSettings.renderQueueRange = RenderQueueRange.opaque;
                        renderStateBlock.mask = RenderStateMask.Depth;
                        renderStateBlock.depthState = new DepthState(true);
                        drawingSettings.sortingSettings = sortingSettings;
                        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);
                    }

                    context.ExecuteCommandBuffer(cmd);
                    CommandBufferPool.Release(cmd);
                }

                // skybox
                {
                    var cmd = CommandBufferPool.Get(_drawSkyboxSampler.name);
                    using (new ProfilingScope(cmd, _drawSkyboxSampler))
                    {
                        context.ExecuteCommandBuffer(cmd);
                        cmd.Clear();
                        var drawSkyBox = false;
#if UNIVERSAL_RENDER_PIPELINE
                        var additionalCameraData = camera.GetComponent<UniversalAdditionalCameraData>();
                        if (additionalCameraData.renderType == CameraRenderType.Base)
                        {
                            drawSkyBox = camera.clearFlags == CameraClearFlags.Skybox;
                        }
#else
                        drawSkyBox = camera.clearFlags == CameraClearFlags.Skybox;
#endif
                        if (drawSkyBox)
                        {
                            context.DrawSkybox(camera);
                        }
                    }

                    context.ExecuteCommandBuffer(cmd);
                    CommandBufferPool.Release(cmd);
                }

                // draw transparent
                {
                    var cmd = CommandBufferPool.Get(_drawTransparentSampler.name);
                    using (new ProfilingScope(cmd, _drawTransparentSampler))
                    {
                        context.ExecuteCommandBuffer(cmd);
                        cmd.Clear();
                        sortingSettings.criteria = SortingCriteria.CommonTransparent;
                        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
                        renderStateBlock.mask = RenderStateMask.Depth;
                        renderStateBlock.depthState = new DepthState(false);
                        ;
                        drawingSettings.sortingSettings = sortingSettings;
                        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);
                    }

                    context.ExecuteCommandBuffer(cmd);
                    CommandBufferPool.Release(cmd);
                }

                cameraRenderingSampler.End(profilingCMD);
                context.ExecuteCommandBuffer(profilingCMD);
                CommandBufferPool.Release(profilingCMD);

                // submit
                context.Submit();

                EndCameraRendering(context, camera);
            }

            EndContextRendering(context, cameras);
        }
    }
}