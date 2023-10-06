using System.Collections;
using UnityEngine;

namespace Cc83.Utils
{
    public static class AnimatorUtils
    {
        public static IEnumerator ChangeFloat(Animator animator, int key, float value, float speed = 1, System.Action onComplete = null)
        {
            var tensity = animator.GetFloat(key);
            var currentTensity = tensity;

            while (Mathf.Abs(value - currentTensity) > 0.001f)
            {
                currentTensity = Mathf.Lerp(currentTensity, value, Time.deltaTime * speed * 100);
                animator.SetFloat(key, currentTensity);
                yield return null;
            }
            
            onComplete?.Invoke();
        }
    }
}