using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Interactable
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ShootController : MonoBehaviour
    {
        private static readonly int Shoot = Animator.StringToHash("Shoot");
        private static readonly int TriggerHold = Animator.StringToHash("TriggerHold");

        [Range(0.01f, 0.5f)]
        public float cdTime = 0.17f;

        public Animator triggerAnimator;
        
        private Animator animator;
        
        private AudioSource audioSource;
        
        private XRGrabInteractable interactable;

        private float triggerTime;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
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
            
            audioSource.Play();
            animator.SetTrigger(Shoot);

            if (triggerAnimator)
            {
                triggerAnimator.SetBool(TriggerHold, true);
            }
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
