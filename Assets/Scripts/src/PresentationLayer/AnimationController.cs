using src.Behavior_Layer;
using UnityEngine;
using UnityEngine.UI;

namespace src.PresentationLayer
{
    public class AnimationController : MonoBehaviour
    {
        [SerializeField] 
        private Animator animator;

        [SerializeField] 
        private Text currentPlayingAnimation;
        
        [SerializeField] 
        private float defaultCrossFadeFrame = 0.2f;
        
        private Character _character;
        private CharacterEventManager _eventManager;

        private void Start()
        {
            _character = GetComponent<Character>();
            _eventManager = _character.EventManager;
            _eventManager.OnAnimationTrigger += (animationEventConfig, character) =>
            {
                PlayAnimation(animationEventConfig.animationClip);
            };
        }

        public void PlayAnimation(string animationName, float transitionDuration)
        {
            if (string.IsNullOrEmpty(animationName)) return;
            
            animator.CrossFade(animationName, transitionDuration);
        }
        
        /// <summary>
        /// 立即播放指定的动画片段（硬切换）。
        /// 依赖于 Animator Controller 中存在与 AnimationClip 同名的状态。
        /// </summary>
        /// <param name="clip">要播放的动画片段</param>
        /// <param name="layerIndex">动画层级索引 (通常为 0)</param>
        /// <param name="normalizedTime">从动画的哪个时间点开始播放 (0 是开头, 1 是结尾)</param>
        public void PlayAnimation(AnimationClip clip, int layerIndex = 0, float normalizedTime = 0f)
        {
            if (!clip || !animator)
            {
                // Debug.LogWarning($"AnimationClip is null or Animator not found on {gameObject.name}. Cannot play animation.");
                return;
            }
            // 使用 AnimationClip 的名字作为 Animator State 的名字
            animator.Play(clip.name, layerIndex, normalizedTime);
            
            currentPlayingAnimation.text = clip.name;
        }
        
        /// <summary>
        /// 平滑过渡到指定的动画片段。
        /// 依赖于 Animator Controller 中存在与 AnimationClip 同名的状态。
        /// </summary>
        /// <param name="clip">要过渡到的动画片段</param>
        /// <param name="crossFadeDuration">过渡时间 (秒)。如果小于0，则使用默认值。</param>
        /// <param name="layerIndex">动画层级索引 (通常为 0)</param>
        /// <param name="normalizedTime">从目标动画的哪个时间点开始播放 (0 是开头)</param>
        public void CrossFadeAnimation(AnimationClip clip, float crossFadeDuration = -1f, int layerIndex = 0, float normalizedTime = 0f)
        {
            if (!clip || !animator)
            {
                // Debug.LogWarning($"AnimationClip is null or Animator not found on {gameObject.name}. Cannot crossfade.");
                return;
            }

            float duration = (crossFadeDuration >= 0) ? crossFadeDuration : defaultCrossFadeFrame;

            // 使用 AnimationClip 的名字作为 Animator State 的名字
            animator.CrossFade(clip.name, duration, layerIndex, normalizedTime);
            
            currentPlayingAnimation.text = clip.name;
        }
    }
}