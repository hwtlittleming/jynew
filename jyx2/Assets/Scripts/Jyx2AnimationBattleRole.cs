
using System;
using Animancer;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Jyx2
{
    /// <summary>
    /// 动画播放者
    /// </summary>
    public abstract class AnimationBattleRole : MonoBehaviour
    {
        public abstract Animator GetAnimator();

        private HybridAnimancerComponent _animancer;
        public HybridAnimancerComponent GetAnimancer()
        {
            //TODO:判断是否销毁了_animacer，替换模型_animacer不会立即消失……,所以直接每次都get一次
            var animator = GetAnimator();
            _animancer = GameUtil.GetOrAddComponent<HybridAnimancerComponent>(animator.transform);
            
            if(_animancer.Animator == null)
                _animancer.Animator = animator;
            
            if(_animancer.Controller == null)
                _animancer.Controller = animator.runtimeAnimatorController;
            
            return _animancer;
        }

        protected void InitAnimantionSystem()
        {
            GetAnimator();
            GetAnimancer();
        }
        
        /// <summary>
        /// 当前的技能播放
        /// </summary>
        public SkillDisplayAsset CurDisplay { get; set; }

        bool IsStandardModelAvata()
        {
            var animator = GetAnimator();
            var controller = animator.runtimeAnimatorController;
            return controller.name == "jyx2humanoidController.controller";
        }
        
        public virtual void Idle()
        {
            if (this == null || CurDisplay == null)
                return;

            PlayAnimation(CurDisplay.LoadAnimation(SkillDisplayAsset.Jyx2RoleAnimationType.Idle));
        }

        public virtual void DeadOrIdle()
        {
            Idle();
        }
        
        public virtual void BeHit()
        {
            if (this == null)
                return;

            AnimationClip clip = null;
            if (CurDisplay == null)
            {
                clip = GlobalAssetConfig.Instance.defaultBeHitClip;
            }
            else
            {
                clip = CurDisplay.LoadAnimation(SkillDisplayAsset.Jyx2RoleAnimationType.Behit);
            }
            
            PlayAnimation(clip, DeadOrIdle, 0.25f);
        }

        public virtual void Attack()
        {
            if (this == null)
                return;

            PlayAnimation(CurDisplay.LoadAnimation(SkillDisplayAsset.Jyx2RoleAnimationType.Attack), 
                Idle, 0.25f);
        }

        public virtual void Run()
        {
            if (this == null)
                return;
            
            AnimationClip clip = null;
            if (CurDisplay == null)
            {
                clip = GlobalAssetConfig.Instance.defaultMoveClip;
            }
            else
            {
                clip = CurDisplay.LoadAnimation(SkillDisplayAsset.Jyx2RoleAnimationType.Move);
            }

            PlayAnimation(clip);
        }
        
        
        public virtual void   ShowDamage()
        {
            //DONOTHING
        }

        public virtual void MarkHpBarIsDirty()
        {
            //DONOTHING
        }

        public virtual void UnmarkHpBarIsDirty()
        {
            //DONOTHING
        }

        public UniTask PlayAnimationAsync(AnimationClip clip, float fadeDuration = 0f)
        {
            UniTaskCompletionSource source = new UniTaskCompletionSource();
            PlayAnimation(clip, () =>
            {
                source.TrySetResult();
            }, fadeDuration);
            return source.Task;
        }
        
        public void PlayAnimation(AnimationClip clip, Action callback = null, float fadeDuration = 0f)
        {
            if (clip == null)
            {
                Debug.LogError("调用了空的动作!");
                callback ? .Invoke();
                return;
            }
            
            var animancer = GetAnimancer();
            animancer.Stop();

            //检查动作配置是否正确
            if (clip.isLooping && callback != null)
            {
                Debug.LogError($"动作设置了LOOP但是会有回调！请检查{clip.name}");
            }
            else if (!clip.isLooping && callback == null)
            {
                Debug.LogError($"动作没设置LOOP但是没有回调！请检查{clip.name}");
            }
            
            var state = animancer.Play(clip, 0.25f);

            if (callback != null)
            {
                if (fadeDuration > 0)
                {
                    GameUtil.CallWithDelay(state.Duration - fadeDuration, callback);
                }
                else
                {
                    state.Events.OnEnd = () =>
                    {
                        state.Stop();
                        callback();
                    };
                }
            }
        }
    }
}
    
