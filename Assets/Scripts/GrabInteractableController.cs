using Cc83.HandPose;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Interactable
{
    [RequireComponent(typeof(InteractablePose))]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class GrabInteractableController : MonoBehaviour
    {
        private InteractablePose pose;
        
        private XRGrabInteractable interactable;

        private void Awake()
        {
            pose = GetComponent<InteractablePose>();
            interactable = GetComponent<XRGrabInteractable>();
            interactable.firstSelectEntered.AddListener(OnFirstSelectEntered);
            interactable.lastSelectExited.AddListener(OnLastSelectExited);
        }

        private void OnDestroy()
        {
            interactable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
            interactable.lastSelectExited.RemoveListener(OnLastSelectExited);
        }

        public void OnFirstSelectEntered(SelectEnterEventArgs eventArgs)
        {
            // eventArgs.interactorObject.
        }
        
        public void OnLastSelectExited(SelectExitEventArgs eventArgs)
        {
            
        }
    }
}
