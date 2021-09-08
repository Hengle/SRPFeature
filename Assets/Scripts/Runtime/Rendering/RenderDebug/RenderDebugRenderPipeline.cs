using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

namespace Game.Runtime
{
    public sealed class RenderDebugRenderPipeline : RenderPipeline
    {
        private Material _drawOpaqueMaterial;
        private Material _drawTransparentMaterial;
        private Material _mipmapColorMaterial;
        private Material _mipmapTextureMaterial;

        private ScriptableCullingParameters _cullingParams;
        private readonly List<ShaderTagId> _shaderTagIdList;

        public RenderDebugRenderPipeline()
        {
            _shaderTagIdList = new List<ShaderTagId>();
        }

        public void SetUp(IEnumerable<string> passNames, Material overdrawOpaque, Material overdrawTransparent, Material mipmapColor, Material mipmapTexture)
        {
            foreach (var name in passNames)
            {
                _shaderTagIdList.Add(new ShaderTagId(name));
            }
            _drawOpaqueMaterial = overdrawOpaque;
            _drawTransparentMaterial = overdrawTransparent;
            _mipmapColorMaterial = mipmapColor;
            _mipmapTextureMaterial = mipmapTexture;
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            Render(context, new List<Camera>(cameras));
        }

        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            cameras.Sort((camera1, camera2) => (int)camera1.depth - (int)camera2.depth);

            BeginContextRendering(context, cameras);

            foreach (var camera in cameras)
            {
                BeginCameraRendering(context, camera);

                // culling
                if (!camera.TryGetCullingParameters(false, out _cullingParams))
                {
                    continue;
                }
                var cullingResults = context.Cull(ref _cullingParams);
                
                context.SetupCameraProperties(camera);
                
                // adapt universal render pipeline
                var universalAdditionalCameraData = camera.GetComponent<UniversalAdditionalCameraData>();
                
                //rt clear flag
                var clearDepth = true;
                var clearColor = true;
                if (universalAdditionalCameraData !=null)
                {
                    if (universalAdditionalCameraData.renderType == CameraRenderType.Overlay)
                    {
                        clearDepth = universalAdditionalCameraData.clearDepth;
                        clearColor = false;
                    }
                }
                else
                {
                    var clearFlags = camera.clearFlags;
                    clearDepth = clearFlags != CameraClearFlags.Nothing;
                    clearColor = clearFlags == CameraClearFlags.Color;
                }

                var cmd = CommandBufferPool.Get("Clear");
                cmd.ClearRenderTarget(clearDepth, clearColor, camera.backgroundColor);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);

                // render state
                var sortingSettings = new SortingSettings(camera);
                var filteringSettings = new FilteringSettings(RenderQueueRange.all, camera.cullingMask);
                var drawingSettings = new DrawingSettings();
                for (var i = 0; i < _shaderTagIdList.Count; i++)
                {
                    drawingSettings.SetShaderPassName(i, _shaderTagIdList[i]);
                }

                if (camera.cameraType == CameraType.Game)
                {
                    drawingSettings.overrideMaterial = _drawOpaqueMaterial;
                }

                // draw opaque
                sortingSettings.criteria = SortingCriteria.CommonOpaque;
                filteringSettings.renderQueueRange = RenderQueueRange.opaque;
                drawingSettings.sortingSettings = sortingSettings;
                if (camera.cameraType == CameraType.Game)
                {
                    drawingSettings.overrideMaterial = _drawOpaqueMaterial;
                }
                context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

                // skybox
                var drawSkyBox = false;
                if (universalAdditionalCameraData != null)
                {
                    if (universalAdditionalCameraData.renderType == CameraRenderType.Base)
                    {
                        drawSkyBox = camera.clearFlags == CameraClearFlags.Skybox;
                    }
                }
                else
                {
                    drawSkyBox = camera.clearFlags == CameraClearFlags.Skybox;
                }
                if (drawSkyBox)
                {
                    context.DrawSkybox(camera);
                }

                // draw transparent
                sortingSettings.criteria = SortingCriteria.CommonTransparent;
                filteringSettings.renderQueueRange = RenderQueueRange.transparent;
                drawingSettings.sortingSettings = sortingSettings;
                if (camera.cameraType == CameraType.Game)
                {
                    drawingSettings.overrideMaterial = _drawTransparentMaterial;
                }
                context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

                // submit
                context.Submit();

                EndCameraRendering(context, camera);
            }

            EndContextRendering(context, cameras);
        }
    }
}