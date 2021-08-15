using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class TextureChannelCombineTools : EditorWindow
    {
        [MenuItem("Arts Tools/Texture/ChannelCombine[window]")]
        public static void ShowWindow()
        {
            TextureChannelCombineTools wnd = GetWindow<TextureChannelCombineTools>();
            wnd.titleContent = new GUIContent("TextureChannelCombineTools");
        }
    }
}

