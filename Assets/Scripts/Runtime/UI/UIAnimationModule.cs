using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Game.Runtime
{
    [RequireComponent(typeof(Animation))]
    public class UIAnimationModule : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        private Animation _player;
        public Animation Player => _player;
        [SerializeField]
        private AnimationClip _openAnim;
        [SerializeField]
        private AnimationClip _closeAnim;

        private void OnValidate()
        {
            if (_player != null)
            {
                _player = GetComponent<Animation>();
            }
            _player.playAutomatically = false;
            _player.wrapMode = WrapMode.Once;
            _player.cullingType = AnimationCullingType.AlwaysAnimate;
            _player.hideFlags = HideFlags.HideInInspector;
            if (_openAnim != null)
            {
                _openAnim.legacy = true;
                _player.AddClip(_openAnim, "Open");    
            }

            if (_closeAnim != null)
            {
                _closeAnim.legacy = true;
                _player.AddClip(_closeAnim, "Close");
            }
        }

        public UniTask PlayOpenAnimation()
        {
            if (_openAnim != null)
            {
                _player.Play("Open");
            }
            return UniTask.Delay(TimeSpan.FromSeconds(_openAnim.length));
        }

        public UniTask PlayCloseAnimation()
        {
            if (_closeAnim != null)
            {
                _player.Play("Close");
            }
            return UniTask.Delay(TimeSpan.FromSeconds(_openAnim.length));
        }
    }
}