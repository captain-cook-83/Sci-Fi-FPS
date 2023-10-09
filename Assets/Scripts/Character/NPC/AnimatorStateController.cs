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
            CancelChangeSpeed();
            CancelChangeTensity();
        }

        public void ChangeSpeed(float value, float speed = 1, Action onComplete = null)
        {
            CancelChangeSpeed();
            
            _speedCoroutine = StartCoroutine(AnimatorUtils.ChangeFloat(_animator, AnimatorConstants.AnimatorSpeed, value, speed, onComplete));
        }

        public void ChangeTensity(float value, float speed = 1, Action onComplete = null)
        {
            CancelChangeTensity();
            
            _tensityCoroutine = StartCoroutine(AnimatorUtils.ChangeFloat(_animator, AnimatorConstants.AnimatorTensity, value, speed, onComplete));
        }
        
        private void CancelChangeSpeed()
        {
            if (_speedCoroutine != null)
            {
                StopCoroutine(_speedCoroutine);
            }
        }
        
        private void CancelChangeTensity()
        {
            if (_tensityCoroutine != null)
            {
                StopCoroutine(_tensityCoroutine);
            }
        }
    }
}
