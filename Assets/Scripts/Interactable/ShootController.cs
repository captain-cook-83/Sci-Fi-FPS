using Cc83.Character;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Interactable
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ShootController : MonoBehaviour
    {
        private static readonly int Shoot = Animator.StringToHash("Shoot");
        private static readonly int TriggerHold = Animator.StringToHash("TriggerHold");

        [Range(0.01f, 0.5f)]
        public float cdTime = 0.17f;

        public Animator triggerAnimator;

        public Transform shootPoint;

        public GameObject trajectoryPrefab;
        
        public FireEffectManager fireEffectManager;
        
        private Animator animator;
        
        private AkAmbient akAmbient;
        
        private XRGrabInteractable interactable;

        private float triggerTime;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            akAmbient = GetComponent<AkAmbient>();
            interactable = GetComponent<XRGrabInteractable>();
            
            interactable.activated.AddListener(OnShootActive);
            interactable.deactivated.AddListener(OnShootDeactivate);
        }

        private void OnDestroy()
        {
            interactable.activated.RemoveListener(OnShootActive);
            interactable.deactivated.RemoveListener(OnShootDeactivate);
        }

        private void OnShootActive(ActivateEventArgs args)
        {
            var currentTime = Time.time;
            if (currentTime < triggerTime)
            {
                return;
            }

            triggerTime = currentTime + cdTime;
    
            // 首先获取位置和旋转等数据，避免接下来的动画逻辑改变相关信息后计算出现偏差
            var shootInfo = fireEffectManager.transform;
            var shootPosition = shootInfo.position;
            var shootRotation = shootInfo.rotation;
            var shootDirection = shootInfo.forward;

            akAmbient.data?.Post(gameObject);
            animator.SetTrigger(Shoot);
            if (triggerAnimator)
            {
                triggerAnimator.SetBool(TriggerHold, true);
            }
            
            Instantiate(trajectoryPrefab, shootPoint.position, shootPoint.rotation);
            
            var handController = args.interactorObject.transform.GetComponentInParent<HandController>();
            handController.Shake();
            
            fireEffectManager.Shoot(shootPosition, shootRotation, shootDirection);
        }

        private void OnShootDeactivate(DeactivateEventArgs args)
        {
            if (triggerAnimator)
            {
                triggerAnimator.SetBool(TriggerHold, false);
            }
        }
    }
}
