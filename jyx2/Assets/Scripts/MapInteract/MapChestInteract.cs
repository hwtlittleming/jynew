
using System;
using System.Collections;
using System.Collections.Generic;
using Animancer;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Jyx2
{
    [RequireComponent(typeof(Animator))]
    public class MapChestInteract : MonoBehaviour
    {

        public AnimationClip clip;

        private AnimancerComponent _animancerComponent;

        private void Awake()
        {
            var animator=this.GetComponent<Animator>() ?? this.gameObject.AddComponent<Animator>();
            _animancerComponent = this.GetComponent<AnimancerComponent>() ??
                                  this.gameObject.AddComponent<AnimancerComponent>();
            _animancerComponent.Animator = animator;
        }

        /// <summary>
        /// 设置为打开状态
        /// </summary>
        public void SetOpened()
        {
            var state = _animancerComponent.Play(clip);
            state.Events.OnEnd = _animancerComponent.Playable.PauseGraph;
            state.NormalizedTime = 1;
        }
        public UniTask Open()
        {
            _animancerComponent.Playable.UnpauseGraph();
            var state = _animancerComponent.Play(clip);
            state.Speed = 1;
            state.Events.OnEnd = () =>
            {
                _animancerComponent.Playable.PauseGraph();
            };
            return UniTask.Delay(TimeSpan.FromSeconds(clip.length));
        }

        public void Close(Action action = null)
        {
            _animancerComponent.Playable.UnpauseGraph();
            var state = _animancerComponent.Play(clip);
            state.Speed = -1;
            state.Events.OnEnd = () =>
            {
                action?.Invoke();
                _animancerComponent.Playable.PauseGraph();
            };
        }
    }
}