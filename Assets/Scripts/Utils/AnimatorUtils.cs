using System.Collections;
using UnityEngine;

namespace Cc83.Utils
{
    public static class AnimatorUtils
    {
        public delegate bool CoroutineValidator();
        
        public static IEnumerator ChangeFloat(Animator animator, int key, float value, CoroutineValidator validator, float speed = 1, System.Action onComplete = null)
        {
            var tensity = animator.GetFloat(key);
            var currentTensity = tensity;

            while (Mathf.Abs(value - currentTensity) > 0.001f && validator())
            {
                currentTensity = Mathf.Lerp(currentTensity, value, Time.deltaTime * speed * 100);
                animator.SetFloat(key, currentTensity);
                yield return null;
            }
            animator.SetFloat(key, value);
            
            onComplete?.Invoke();
        }
    }
}