using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Interactable
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ShootController : MonoBehaviour
    {
        private AudioSource audioSource;
        
        private XRGrabInteractable interactable;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            interactable = GetComponent<XRGrabInteractable>();
            interactable.activated.AddListener(OnShootActive);
        }

        private void OnDestroy()
        {
            interactable.activated.RemoveListener(OnShootActive);
        }

        private void OnShootActive(ActivateEventArgs args)
        {
            audioSource.Play();
        }
    }
}
