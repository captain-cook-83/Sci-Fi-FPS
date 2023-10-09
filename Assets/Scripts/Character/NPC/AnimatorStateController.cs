using System;
using Cc83.Utils;
using UnityEngine;

namespace Cc83.Character
{
    public class AnimatorStateController : MonoBehaviour
    {
        private Animator _animator;

        private Coroutine _speedCoroutine;
        
        private Coroutine _tensityCoroutine;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnDestroy()
        {
            CancelCoroutine(_speedCoroutine);
            CancelCoroutine(_tensityCoroutine);
        }

        public void ChangeSpeed(float value, float speed = 1, Action onComplete = null)
        {
            CancelCoroutine(_speedCoroutine);
            
            _speedCoroutine = StartCoroutine(AnimatorUtils.ChangeFloat(_animator, AnimatorConstants.AnimatorSpeed, value, speed, onComplete));
        }

        public void ChangeTensity(float value, float speed = 1, Action onComplete = null)
        {
            CancelCoroutine(_tensityCoroutine);
            
            _tensityCoroutine = StartCoroutine(AnimatorUtils.ChangeFloat(_animator, AnimatorConstants.AnimatorTensity, value, speed, onComplete));
        }
        
        private void CancelCoroutine(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
    }
}
