using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Interactable
{
    public class RayAttachModifier : MonoBehaviour
    {
        private IXRSelectInteractable _selectInteractable;

        protected void OnEnable()
        {
            _selectInteractable = GetComponent<IXRSelectInteractable>();
            _selectInteractable.selectEntered.AddListener(OnSelectEntered);
        }

        protected void OnDisable()
        {
            _selectInteractable.selectEntered.RemoveListener(OnSelectEntered);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            var interactor = args.interactorObject;
            if (interactor is not XRRayInteractor) return;
            
            var attachTransform = interactor.GetAttachTransform(_selectInteractable);
            var originalAttachPose = interactor.GetLocalAttachPoseOnSelect(_selectInteractable);
            attachTransform.SetLocalPose(originalAttachPose);
        }
    }
}