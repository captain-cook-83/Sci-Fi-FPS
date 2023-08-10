using System.Collections;
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

        public Transform shootPoint;

        public GameObject trajectoryPrefab;

        public AudioClip[] shellFallAudios;
        
        private Animator animator;
        
        private AudioSource audioSource;

        private AudioClip defaultAudio;
        
        private XRGrabInteractable interactable;

        private float triggerTime;

        private Coroutine delayedAudioHandler;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            interactable = GetComponent<XRGrabInteractable>();
            
            interactable.activated.AddListener(OnShootActive);
            interactable.deactivated.AddListener(OnShootDeactivate);

            defaultAudio = audioSource.clip;
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
            
            audioSource.clip = defaultAudio;
            audioSource.Play();
            animator.SetTrigger(Shoot);
            if (triggerAnimator)
            {
                triggerAnimator.SetBool(TriggerHold, true);
            }

            Instantiate(trajectoryPrefab, shootPoint.position, shootPoint.rotation);
            
            if (delayedAudioHandler != null)
            {
                StopCoroutine(delayedAudioHandler);
                delayedAudioHandler = null;
            }
            
            if (shellFallAudios.Length > 0)
            {
                var audioClip = shellFallAudios[Random.Range(0, shellFallAudios.Length)];
                delayedAudioHandler = StartCoroutine(PlayShellFallAudio(audioClip, 1));
            }
        }

        private void OnShootDeactivate(DeactivateEventArgs args)
        {
            if (triggerAnimator)
            {
                triggerAnimator.SetBool(TriggerHold, false);
            }
        }

        private IEnumerator PlayShellFallAudio(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            delayedAudioHandler = null;

            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
