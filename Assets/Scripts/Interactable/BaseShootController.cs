using System.Collections;
using UnityEngine;
using Event = AK.Wwise.Event;

namespace Cc83.Interactable
{
    public abstract class BaseShootController : MonoBehaviour
    {
        [Range(0.01f, 0.5f)]
        public float cdTime = 0.17f;
        
        public FireEffectManager fireEffectManager;
        
        public Event akEvent;
        
        [Range(10, 100)]
        public int maxAmmunitionCapacity = 40;

        public virtual bool IsEnabled => isActiveAndEnabled;
        
        protected bool PendingShoot;
        
        protected Animator Animator;
        
        protected int AmmunitionQuantity;
        
        private float _triggerTime;
        
        protected virtual void Awake()
        {
            Animator = GetComponent<Animator>();
            AmmunitionQuantity = maxAmmunitionCapacity;
        }
        
        protected virtual void OnDestroy() {}

        private void LateUpdate()
        {
            if (PendingShoot)
            {
                PendingShoot = false;
// #if !UNITY_EDITOR
//                 AmmunitionQuantity--;
// #endif
                
                // 首先获取位置和旋转等数据，避免接下来的动画逻辑改变相关信息后计算出现偏差
                var shootInfo = fireEffectManager.transform;
                var shootPosition = shootInfo.position;
                var shootRotation = shootInfo.rotation;
                var shootDirection = shootInfo.forward;
                
                OnShootAnimation();
            
                fireEffectManager.Shoot(shootPosition, shootRotation, shootDirection);
            }
        }

        public bool Shoot(int times = 1)
        {
            var currentTime = Time.time;
            if (currentTime < _triggerTime) return false;
            
            _triggerTime = currentTime + cdTime;
            StartCoroutine(AsyncShoot(times));
            return true;
        }

        protected abstract IEnumerator AsyncShoot(int times);

        protected virtual void OnShootAnimation() { }
    }
}