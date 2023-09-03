using Cc83.Character;
using Cc83.HandPose;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

#if BURST_PRESENT
using Unity.Burst;
#endif

namespace Cc83.Interactable
{
#if BURST_PRESENT
    [BurstCompile]
#endif
    public class InteractablePoseGrabTransformer : XRBaseGrabTransformer
    {
        public AudioClip readyAudio;

        private HandController primaryHandController;

        private HandController secondaryHandController;

        private InteractablePoseData primaryPoseData;
        
        private InteractablePoseData secondaryPoseData;
        
        public override void OnLink(XRGrabInteractable grabInteractable)
        {
            var attachTransform = grabInteractable.attachTransform;
            if (!ReferenceEquals(attachTransform.parent, grabInteractable.transform))
            {
                Debug.LogError($"AttachTransform({attachTransform.name}) must be a direct child of the grabInteractable({grabInteractable.name})");
            }
        }
        
        public override void OnGrab(XRGrabInteractable grabInteractable)
        {
            SetPrimaryInteractor(grabInteractable);
        }

        public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
        {
            switch (grabInteractable.interactorsSelecting.Count)
            {
                case 1:
                    var primaryInteractor = grabInteractable.interactorsSelecting[0];
                    var handController = primaryInteractor.transform.GetComponentInParent<HandController>();
                    
                    if (primaryHandController && !ReferenceEquals(primaryHandController, handController))
                    {
                        primaryHandController.ResetBindableShell();
                        primaryHandController = null;
                        primaryPoseData = default;
                    }
                    
                    if (secondaryHandController)                // means from 2 to 1
                    {
                        secondaryHandController.ResetBindableShell();
                        secondaryHandController = null;
                        secondaryPoseData = default;
                        
                        SetPrimaryInteractor(grabInteractable, false);
                    }
                    break;
                case 2:
                    var secondaryInteractor = grabInteractable.interactorsSelecting[1];
                    var interactablePose = grabInteractable.GetComponent<InteractablePose>();
                    
                    secondaryHandController = secondaryInteractor.transform.GetComponentInParent<HandController>();
                    secondaryPoseData = secondaryHandController.side == HandSide.Left ? interactablePose.secondaryLeftPose : interactablePose.secondaryRightPose;
                    secondaryHandController.SetPoseData(secondaryPoseData, secondaryPoseData);
                    break;
            }
        }
        
        public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
        {
            if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic && updatePhase != XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender) return;
            
            switch (grabInteractable.interactorsSelecting.Count)
            {
                case 1:
                    var interactor = grabInteractable.interactorsSelecting[0];
                    var interactorAttachTransform = interactor.GetAttachTransform(grabInteractable);

                    if (grabInteractable.trackRotation)
                    {
                        targetPose.rotation = interactorAttachTransform.rotation * primaryPoseData.handLocalRotation;
                    }
                    
                    if (grabInteractable.trackPosition)
                    {
                        targetPose.position = interactorAttachTransform.TransformPoint(primaryPoseData.handLocalPosition);
                    }
                    break;
                case 2:         // TODO 此处假设 Primary Interactor 为数组中子一个元素，但尚未确认是否与底层 API 承诺一致
                    var primaryInteractor = grabInteractable.interactorsSelecting[0];
                    var primaryAttachTransform = primaryInteractor.GetAttachTransform(grabInteractable);
                    var primaryProjection = primaryAttachTransform.TransformPoint(primaryPoseData.handProjection);
                    
                    var secondaryBindableShell = secondaryHandController.interactableBindableShell;

                    if (grabInteractable.trackRotation)         // 首先计算旋转
                    {
                        var primaryFixShell = primaryHandController.interactableFixShell;
                        var primaryBindableShell = primaryHandController.interactableBindableShell;
                        var primaryRotationAxis = primaryFixShell.TransformVector(primaryHandController.primaryRotationAxis);
                        
                        var secondaryFixShell = secondaryHandController.interactableFixShell;
                        var secondaryProjection = secondaryFixShell.TransformPoint(secondaryPoseData.handProjection);
                        var secondaryRotationAxis = secondaryFixShell.TransformVector(secondaryHandController.secondaryRotationAxis);
                        
                        targetPose.rotation = Quaternion.LookRotation(secondaryProjection - primaryProjection, (primaryRotationAxis + secondaryRotationAxis) * 0.5f);   // 双手同时控制物体翻转方向
                        
                        primaryBindableShell.rotation = targetPose.rotation * Quaternion.Inverse(primaryPoseData.handLocalRotation);
                        secondaryBindableShell.rotation = targetPose.rotation * Quaternion.Inverse(secondaryPoseData.handLocalRotation);
                    }

                    if (grabInteractable.trackPosition)
                    {
                        targetPose.position = primaryProjection - primaryPoseData.handProjectionLength * targetPose.forward;
                    }
                    else
                    {
                        targetPose.position = grabInteractable.transform.position;
                    }
                    
                    secondaryBindableShell.position = targetPose.position - secondaryBindableShell.rotation * secondaryPoseData.handLocalPosition;
                    break;
            }
        }
        
        private void SetPrimaryInteractor(XRGrabInteractable grabInteractable, bool firstGrab = true)
        {
            var interactor = grabInteractable.interactorsSelecting[0];
            var interactablePose = grabInteractable.GetComponent<InteractablePose>();

            primaryHandController = interactor.transform.GetComponentInParent<HandController>();
            primaryPoseData = primaryHandController.side == HandSide.Left ? interactablePose.primaryLeftPose : interactablePose.primaryRightPose;
            
            var primaryActivatePose = primaryHandController.side == HandSide.Left ? interactablePose.primaryLeftActivatePose : interactablePose.primaryRightActivatePose;
            primaryHandController.SetPoseData(primaryPoseData, primaryActivatePose);
            primaryHandController.PlayCatchSound();
        }
    }
}
