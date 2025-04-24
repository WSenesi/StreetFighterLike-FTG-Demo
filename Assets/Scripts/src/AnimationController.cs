using UnityEngine;

namespace src
{
    public class AnimationController : MonoBehaviour
    {
        public Animator animator;

        public void PlayAnimation(string animationName, float transitionDuration)
        {
            if (string.IsNullOrEmpty(animationName)) return;
            
            animator.CrossFade(animationName, transitionDuration);
        }
    }
}