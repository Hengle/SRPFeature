using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.Runtime
{
    [Serializable]
    public class PlanarReflectionSettings
    {
        public List<string> ShaderPass;
        [Range(0.1f, 2f)]
        public float RenderScale = 1;
        public float ClipPlaneOffset = -0.1f;
        public LayerMask ReflectLayers = -1;
        public float FarClipPlane = 512;
    }

    public class PlanarReflectionsPass : ScriptableRenderPass
    {
        private Camera _reflectionsCamera;
        private PlanarReflectionSettings _settings;

        public PlanarReflectionsPass(PlanarReflectionSettings settings)
        {
            _settings = settings;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
        }
    }
}