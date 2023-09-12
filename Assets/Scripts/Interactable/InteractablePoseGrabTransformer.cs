using Cc83.Character;
using Cc83.HandPose;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using Unity.Burst;

namespace Cc83.Interactable
{
    [BurstCompile]
    public class InteractablePoseGrabTransformer : XRBaseGrabTransformer
    {
        [Tooltip("grab more stable, but slight performance loss.")]
        public bool stableMode = true;
        
        private LocomotionProvider _moveProvider;
        
        private HandController _primaryHandController;

        private HandController _secondaryHandController;

        private InteractablePoseData _primaryPoseData;
        
        private InteractablePoseData _secondaryPoseData;
        
        public override void OnLink(XRGrabInteractable grabInteractable)
        {
            var attachTransform = grabInteractable.attachTransform;
            if (attachTransform && !ReferenceEquals(attachTransform.parent, grabInteractable.transform))
            {
                Debug.LogError($"AttachTransform({attachTransform.name}) must be a direct child of the grabInteractable({grabInteractable.name})");
            }
        }
        
        public override void OnGrab(XRGrabInteractable grabInteractable)
        {
            var interactor = grabInteractable.interactorsSelecting[0];
            _moveProvider = interactor.transform.GetComponentInParent<ContinuousMoveProviderBase>();
            
            SetPrimaryInteractor(grabInteractable);
        }

        public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
        {
            var primaryInteractor = grabInteractable.interactorsSelecting[0];
            var handController = primaryInteractor.transform.GetComponentInParent<HandController>();
            
            switch (grabInteractable.interactorsSelecting.Count)
            {
                case 1:
                    if (_primaryHandController && !ReferenceEquals(_primaryHandController, handController))
                    {
                        _primaryHandController.OnReleaseFromMultiGrab();
                        _primaryHandController = null;
                        _primaryPoseData = default;
                    }
                    
                    if (_secondaryHandController)                // means from 2 to 1
                    {
                        _secondaryHandController.OnReleaseFromMultiGrab();
                        _secondaryHandController = null;
                        _secondaryPoseData = default;
                        
                        SetPrimaryInteractor(grabInteractable, false);
                    }
                    break;
                case 2:
                    var secondaryInteractor = grabInteractable.interactorsSelecting[1];
                    var interactablePose = grabInteractable.GetComponent<InteractablePose>();
                    var interactableAnimatorControllers = grabInteractable.GetComponent<InteractableAnimatorController>();
                    
                    _secondaryHandController = secondaryInteractor.transform.GetComponentInParent<HandController>();
                    _secondaryPoseData = _secondaryHandController.side == HandSide.Left ? interactablePose.secondaryLeftPose : interactablePose.secondaryRightPose;
                    _secondaryHandController.SetPoseData(_secondaryPoseData, _secondaryPoseData);
                    _secondaryHandController.SetAnimatorController(_secondaryHandController.side == HandSide.Left ? interactableAnimatorControllers.leftController : interactableAnimatorControllers.rightController);
                    
                    handController.OnPrimaryFromMultiGrab();
                    break;
            }
        }
        
        public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
        {
            if (stableMode)
            {
                if (!(updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender ||
                      updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic && _moveProvider && _moveProvider.locomotionPhase != LocomotionPhase.Moving)) return;                  // 稳定性更好，但是非移动状态下计算两次   // // _moveProvider is null while XR Socket Interactor
            }
            else
            {
                if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender) return;                                                                          // 手部始终有局部微小的延迟对齐
            }
            
            switch (grabInteractable.interactorsSelecting.Count)
            {
                case 1:
                    var interactor = grabInteractable.interactorsSelecting[0];
                    var interactorAttachTransform = interactor.GetAttachTransform(grabInteractable);

                    if (grabInteractable.trackRotation)
                    {
                        targetPose.rotation = interactorAttachTransform.rotation * _primaryPoseData.handLocalRotation;
                    }
                    
                    if (grabInteractable.trackPosition)
                    {
                        targetPose.position = interactorAttachTransform.TransformPoint(_primaryPoseData.handLocalPosition);
                    }
                    break;
                case 2:         // TODO 此处假设 Primary Interactor 为数组中子一个元素，但尚未确认是否与底层 API 承诺一致
                    if (_primaryHandController == null) return;  // null for XR Socket Interactor
                    
                    var primaryInteractor = grabInteractable.interactorsSelecting[0];
                    var primaryAttachTransform = primaryInteractor.GetAttachTransform(grabInteractable);
                    var primaryProjection = primaryAttachTransform.TransformPoint(_primaryPoseData.handProjection);
                    
                    var secondaryBindableShell = _secondaryHandController.interactableBindableShell;

                    if (grabInteractable.trackRotation)         // 首先计算旋转
                    {
                        var primaryFixShell = _primaryHandController.interactableFixShell;
                        var primaryBindableShell = _primaryHandController.interactableBindableShell;
                        var primaryRotationAxis = primaryFixShell.TransformVector(_primaryHandController.primaryRotationAxis);
                        
                        var secondaryFixShell = _secondaryHandController.interactableFixShell;
                        var secondaryProjection = secondaryFixShell.TransformPoint(_secondaryPoseData.handProjection);
                        var secondaryRotationAxis = secondaryFixShell.TransformVector(_secondaryHandController.secondaryRotationAxis);
                        
                        targetPose.rotation = Quaternion.LookRotation(secondaryProjection - primaryProjection, (primaryRotationAxis + secondaryRotationAxis) * 0.5f);   // 双手同时控制物体翻转方向
                        
                        primaryBindableShell.rotation = targetPose.rotation * Quaternion.Inverse(_primaryPoseData.handLocalRotation);
                        secondaryBindableShell.rotation = targetPose.rotation * Quaternion.Inverse(_secondaryPoseData.handLocalRotation);
                    }

                    if (grabInteractable.trackPosition)
                    {
                        targetPose.position = primaryProjection - _primaryPoseData.handProjectionLength * targetPose.forward;
                    }
                    else
                    {
                        targetPose.position = grabInteractable.transform.position;
                    }
                    
                    secondaryBindableShell.position = targetPose.position - secondaryBindableShell.rotation * _secondaryPoseData.handLocalPosition;
                    break;
            }
        }
        
        private void SetPrimaryInteractor(XRGrabInteractable grabInteractable, bool firstGrab = true)
        {
            var interactor = grabInteractable.interactorsSelecting[0];
            var interactablePose = grabInteractable.GetComponent<InteractablePose>();
            var interactableAnimatorControllers = grabInteractable.GetComponent<InteractableAnimatorController>();

            _primaryHandController = interactor.transform.GetComponentInParent<HandController>();
            if (_primaryHandController == null) return;  // null for XR Socket Interactor
            
            _primaryPoseData = _primaryHandController.side == HandSide.Left ? interactablePose.primaryLeftPose : interactablePose.primaryRightPose;
            
            var primaryActivatePose = _primaryHandController.side == HandSide.Left ? interactablePose.primaryLeftActivatePose : interactablePose.primaryRightActivatePose;
            _primaryHandController.SetPoseData(_primaryPoseData, primaryActivatePose);
            _primaryHandController.SetAnimatorController(_primaryHandController.side == HandSide.Left ? interactableAnimatorControllers.leftController : interactableAnimatorControllers.rightController);
            _primaryHandController.OnCatchInteractable();
        }
    }
}
