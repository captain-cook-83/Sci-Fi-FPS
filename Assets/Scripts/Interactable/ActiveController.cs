using Cc83.Character;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Interactable
{
    public class ActiveController : MonoBehaviour
    {
        private XRGrabInteractable _interactable;
        
        private HandController _handController;
        
        private void Awake()
        {
            _interactable = GetComponent<XRGrabInteractable>();
            
            _interactable.activated.AddListener(OnShootActive);
            _interactable.deactivated.AddListener(OnShootDeactivate);
        }

        private void OnDestroy()
        {
            _interactable.activated.RemoveListener(OnShootActive);
            _interactable.deactivated.RemoveListener(OnShootDeactivate);
        }

        private void OnShootActive(ActivateEventArgs args)
        {
            _handController = args.interactorObject.transform.GetComponentInParent<HandController>();
        }

        private void OnShootDeactivate(DeactivateEventArgs args)
        {
            if (_handController)
            {
                _handController = null;
            }
        }
    }
}