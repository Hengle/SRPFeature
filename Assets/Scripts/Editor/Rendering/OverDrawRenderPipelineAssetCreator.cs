using Game.Runtime;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Game.Editor
{
    public static class OverDrawRenderPipelineAssetCreator
    {
        [MenuItem("Assets/Create/Rendering/OverDrawRenderPipelineAsset", priority = 1)]
        private static void Create()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<CreateOverDrawRenderPipelineAsset>(),
                "OverDrawRenderPipelineAsset.asset",
                null,
                null);
        }
    }

    internal class CreateOverDrawRenderPipelineAsset : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var instance = CreateInstance<OverDrawRenderPipelineAsset>();
            AssetDatabase.CreateAsset(instance, pathName);
        }
    }
}