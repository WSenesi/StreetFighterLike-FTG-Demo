using Mirror;
using src.Behavior_Layer;
using src.Behavior_Layer.EventConfig;
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
        
        private Character.Character _character;
        private CharacterEventManager _eventManager;
        private NetworkIdentity _networkIdentity;

        private void Awake()
        {
            _networkIdentity = GetComponent<NetworkIdentity>();

            if (animator is null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator is null)
            {
                Debug.LogError($"AnimationController requires an Animator component. Disabling script.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            _character = GetComponent<Character.Character>();
            if (_character is null) return;
            
            _eventManager = _character.EventManager;
            if (_eventManager is null) return;
            if (_networkIdentity.isClient)
            {
                _eventManager.OnAnimationTrigger += OnAnimationTriggered;
            }

        }

        private void OnDestroy()
        {
            if (_eventManager is not null)
            {
                _eventManager.OnAnimationTrigger -= OnAnimationTriggered;
            }
        }

        private void OnAnimationTriggered(AnimationEventConfig animationEventConfig, Character.Character character)
        {
            PlayAnimation(animationEventConfig.animationClip);
        }

        // public void PlayAnimation(string animationName, float transitionDuration)
        // {
        //     if (string.IsNullOrEmpty(animationName)) return;
        //     
        //     animator.CrossFade(animationName, transitionDuration);
        // }
        
        /// <summary>
        /// 立即播放指定的动画片段（硬切换）。
        /// 依赖于 Animator Controller 中存在与 AnimationClip 同名的状态。
        /// </summary>
        /// <param name="clip">要播放的动画片段</param>
        /// <param name="layerIndex">动画层级索引 (通常为 0)</param>
        /// <param name="normalizedTime">从动画的哪个时间点开始播放 (0 是开头, 1 是结尾)</param>
        public void PlayAnimation(AnimationClip clip, int layerIndex = 0, float normalizedTime = 0f)
        {
            if (!this.enabled || clip is null) return;
            
            // 确保只有客户端实例才执行动画播放
            if (_networkIdentity.isClient)
            {
                // 使用 AnimationClip 的名字作为 Animator State 的名字
                animator.Play(clip.name, layerIndex, normalizedTime);
                
                // 确保在 UI Text 组件被正确赋值的情况下才去更新
                if (currentPlayingAnimation is not null)
                    currentPlayingAnimation.text = clip.name;
            }
            
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
            if (!this.enabled || clip is null) return;
            
            // 确保只有客户端实例才执行动画播放
            if (_networkIdentity.isClient)
            {
                float duration = (crossFadeDuration >= 0) ? crossFadeDuration : defaultCrossFadeFrame;
                
                // 使用 AnimationClip 的名字作为 Animator State 的名字
                animator.CrossFade(clip.name, duration, layerIndex, normalizedTime);
                
                // 确保在 UI Text 组件被正确赋值的情况下才去更新
                if (currentPlayingAnimation is not null)
                    currentPlayingAnimation.text = clip.name;
            }
        }
    }
}