using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;
using System.Collections.Generic;


namespace Game.Examples
{
    public class RenderPass : MonoBehaviour
    {
        private NativeArray<AttachmentDescriptor> _renderingAttachments;
        
        private void OnEnable()
        {
            RenderPipelineManager.beginContextRendering += Rendering;
            _renderingAttachments = new NativeArray<AttachmentDescriptor>(1, Allocator.Temp);
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginContextRendering += Rendering;
            _renderingAttachments.Dispose();
        }

        private void Rendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            foreach (var cam in cameras)
            {
                
            }
        }
    }
}