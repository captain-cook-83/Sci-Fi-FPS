using Cc83.Character;
using Cc83.HandPose;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace Cc83.Interactable
{
    public class InteractablePoseGrabTransformer : XRBaseGrabTransformer
    {
        public override void OnLink(XRGrabInteractable grabInteractable)
        {
            base.OnLink(grabInteractable);
            
            var attachTransform = grabInteractable.attachTransform;
            if (!ReferenceEquals(attachTransform.parent, grabInteractable.transform))
            {
                Debug.LogError($"AttachTransform({attachTransform.name}) must be a direct child of the grabInteractable({grabInteractable.name})");
            }
        }
        
        public override void OnGrab(XRGrabInteractable grabInteractable)
        {
            base.OnGrab(grabInteractable);
            
            var interactor = grabInteractable.interactorsSelecting[0];
            var handController = interactor.transform.GetComponentInParent<HandController>();
            
            var interactablePose = grabInteractable.GetComponent<InteractablePose>();
            var primaryPose = handController.side == HandSide.Left ? interactablePose.primaryLeftPose : interactablePose.primaryRightPose;
            var primaryActivatePose = handController.side == HandSide.Left ? interactablePose.primaryLeftActivatePose : interactablePose.primaryRightActivatePose;
            var attachTransform = grabInteractable.attachTransform;
            
            handController.SetPoseData(primaryPose, primaryActivatePose);
            attachTransform.SetLocalPositionAndRotation(-primaryPose.handLocalPosition, Quaternion.Inverse(primaryPose.handLocalRotation));
        }
        
        public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
        {
            switch (updatePhase)
            {
                case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                {
                    var interactor = grabInteractable.interactorsSelecting[0];
                    var interactorAttachTransform = interactor.GetAttachTransform(grabInteractable);
                    var thisAttachTransform = grabInteractable.GetAttachTransform(interactor);

                    targetPose.position = interactorAttachTransform.TransformPoint(-thisAttachTransform.localPosition);
                    
                    if (grabInteractable.trackRotation)
                    {
                        var thisTransform = grabInteractable.transform;
                        var rotationOffset = Quaternion.Inverse(Quaternion.Inverse(thisTransform.rotation) * thisAttachTransform.rotation);
                        targetPose.rotation = interactorAttachTransform.rotation * rotationOffset;
                    }
                    
                    break;
                }
            }
        }
    }
}
