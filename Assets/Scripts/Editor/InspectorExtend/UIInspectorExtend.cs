using Game.Runtime;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public static class UIInspectorExtend
    {
        [MenuItem("CONTEXT/UIAnimationModule/Remove Component")]
        public static void RemoveUIAnimationModule(MenuCommand cmd)
        {
            var module = (UIAnimationModule)cmd.context;
            var player = module.Player;
            Object.DestroyImmediate(module);
            Object.DestroyImmediate(player);
        }
    }
}
