using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace Game.Editor
{
    public class TextureCustomMipmapCreator : EditorWindow
    {
        [MenuItem("Arts Tools/Texture/CustomMipmap[window]")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<TextureCustomMipmapCreator>();
            wnd.titleContent = new GUIContent("TextureChannelCombineTools");
        }
        
        [Serializable]
        private class MipmapsContainer : ScriptableObject
        {
            [SerializeField]
            public List<Texture2D> Mipmaps;

            public MipmapsContainer()
            {
                Mipmaps = new List<Texture2D>();
            }

            private void OnDestroy()
            {
                foreach (var mipmap in Mipmaps)
                {
                    if (mipmap != null)
                    {
                        DestroyImmediate(mipmap);
                    }
                }
                Mipmaps.Clear();
            }
        }

        private MipmapsContainer _mipmapsContainer;
        private SerializedObject _mipmapsContainerSerializedObject;
        private SerializedProperty _mipmapsListSerializedProperty;
        private ReorderableList _mipmapReorderableList;

        private void Awake()
        {
            _mipmapsContainer = CreateInstance<MipmapsContainer>();
            _mipmapsContainerSerializedObject = new SerializedObject(_mipmapsContainer);
            _mipmapsListSerializedProperty = _mipmapsContainerSerializedObject.FindProperty("Mipmaps");
            _mipmapReorderableList = new ReorderableList(_mipmapsContainerSerializedObject, _mipmapsListSerializedProperty, true, false, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, new GUIContent("Mipmap Lists"));
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    var item = _mipmapsListSerializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, item, new GUIContent("Mipmap " + index));
                },
            };
        }
        
        private void OnDestroy()
        {
            DestroyImmediate(_mipmapsContainer);
        }

        private void OnGUI()
        {
            _mipmapsContainerSerializedObject.Update();
            _mipmapReorderableList.DoLayoutList();
            _mipmapsContainerSerializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Create and Save Texture"))
            {
                SaveTexture();
            }
        }

        private void SaveTexture()
        {
            if (_mipmapsContainer.Mipmaps.Count < 1)
            {
                EditorUtility.DisplayDialog("TextureCustomMipmap", "can not found any mipmap", "Confirm");
                return;
            }
            
            var level0 = _mipmapsContainer.Mipmaps[0];
            if (level0 == null)
            {
                EditorUtility.DisplayDialog("TextureCustomMipmap", "mipmap level 0 is null", "Confirm");
                return;
            }

            var mipampCounts = _mipmapsContainer.Mipmaps.Count > 12 ? 12 : _mipmapsContainer.Mipmaps.Count;
            var larget = level0.width;
            for (var i = 1; i < mipampCounts; i++)
            {
                var mipmap = _mipmapsContainer.Mipmaps[i];
                if (mipmap != null)
                {
                    if (mipmap.width * 2 == larget)
                    {
                        larget = mipmap.width;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("TextureCustomMipmap", $"mipmap level {i} size is not right, current mipmap size is {larget / 2}, but the selected mipmap texture size is {mipmap.width}", "Confirm");
                        return;
                    }
                }
            }

            var texture = new Texture2D(level0.width, level0.height, TextureFormat.ARGB32, true);
            for (var i = 0; i < mipampCounts; i++)
            {
                var mipmap = _mipmapsContainer.Mipmaps[i];
                if (mipmap == null)
                {
                    continue;
                }
                texture.SetPixels(mipmap.GetPixels(), i);
            }
            
            var path = EditorUtility.SaveFilePanelInProject("Save Texture", _mipmapsContainer.Mipmaps[0].name, "asset", "");
            if (path.Length != 0)
            {
                AssetDatabase.CreateAsset(texture, path);
                AssetDatabase.Refresh();
            }
        }
    }
}