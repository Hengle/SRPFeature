using UnityEngine;
using Game.Runtime;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;


namespace Game.Editor
{
    public static class RenderDebugRenderPipelineAssetCreator
    {
        [MenuItem("Assets/Create/Rendering/RenderDebugRenderPipelineAsset", priority = 1)]
        private static void Create()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<CreateRenderDebugRenderPipelineAsset>(),
                "RenderDebugRenderPipelineAsset.asset",
                null,
                null);
        }
    }

    internal class CreateRenderDebugRenderPipelineAsset : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var instance = CreateInstance<RenderDebugRenderPipelineAsset>();
            AssetDatabase.CreateAsset(instance, pathName);
        }
    }
}