using System;
using System.Collections;
using Cc83.Behaviors;
using UnityEngine;

namespace Cc83.Utils
{
    public static class AnimatorUtils
    {
        public static float ConvertTensity(Tensity tensity)
        {
            return tensity switch
            {
                Tensity.LowestIdle => -2,
                Tensity.LowerIdle => -1,
                Tensity.Rifle => 0.01f,
                Tensity.HigherRifle => 0.5f,
                Tensity.HighestRifle => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(tensity), tensity, null)
            };
        }
        
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
            animator.SetFloat(key, value);
            
            onComplete?.Invoke();
        }
    }
}