using System;
using Cc83.Utils;
using UnityEngine;

namespace Cc83.Character
{
    public class AnimatorStateController : MonoBehaviour
    {
        private Animator _animator;

        private Coroutine _speedCoroutine;

        private Action _speedOnComplete;
        
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

        public void ChangeSpeed(float value, Action onStart = null , Action onComplete = null, bool forceComplete = false)         // 方法体内代码顺序严格保证链式调用下的正常清理及回调
        {
            CancelCoroutine(_speedCoroutine);
            
            _speedCoroutine = StartCoroutine(AnimatorUtils.ChangeFloat(_animator, AnimatorConstants.AnimatorSpeed, value, 0.1f, () => 
            {
                _speedOnComplete = null;
                onComplete?.Invoke();
            }));
            
            var callback = _speedOnComplete;
            _speedOnComplete = forceComplete ? onComplete : null;
            callback?.Invoke();
            
            onStart?.Invoke();
        }

        public void ChangeTensity(float value, float speed = 1.0f)
        {
            CancelCoroutine(_tensityCoroutine);
            
            _tensityCoroutine = StartCoroutine(AnimatorUtils.ChangeFloat(_animator, AnimatorConstants.AnimatorTensity, value, speed));
        }
        
        public float Tensity => _animator.GetFloat(AnimatorConstants.AnimatorTensity);
        
        private void CancelCoroutine(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
    }
}
