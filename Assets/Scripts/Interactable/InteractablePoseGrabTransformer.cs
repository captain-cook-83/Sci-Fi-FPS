using System.Collections;
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

        private AudioSource audioSource;

        private HandController primaryHandController;

        private HandController secondaryHandController;

        private InteractablePoseData primaryPoseData;
        
        private InteractablePoseData secondaryPoseData;

        private Quaternion primaryRotationOffset;
        
        public override void OnLink(XRGrabInteractable grabInteractable)
        {
            var attachTransform = grabInteractable.attachTransform;
            if (!ReferenceEquals(attachTransform.parent, grabInteractable.transform))
            {
                Debug.LogError($"AttachTransform({attachTransform.name}) must be a direct child of the grabInteractable({grabInteractable.name})");
            }

            audioSource = GetComponent<AudioSource>();
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
                    var secondaryAttachTransform = grabInteractable.secondaryAttachTransform;
                    
                    secondaryHandController = secondaryInteractor.transform.GetComponentInParent<HandController>();
                    secondaryPoseData = secondaryHandController.side == HandSide.Left ? interactablePose.secondaryLeftPose : interactablePose.secondaryRightPose;
                    secondaryHandController.SetPoseData(secondaryPoseData, secondaryPoseData);
                    secondaryAttachTransform.SetLocalPositionAndRotation(-secondaryPoseData.handLocalPosition, Quaternion.Inverse(secondaryPoseData.handLocalRotation));
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
                        targetPose.rotation = interactorAttachTransform.rotation * primaryRotationOffset;
                    }
                    
                    if (grabInteractable.trackPosition)
                    {
                        var interactableAttachTransform = grabInteractable.GetAttachTransform(interactor);
                        targetPose.position = interactorAttachTransform.TransformPoint(-interactableAttachTransform.localPosition);
                    }
                    break;
                case 2:         // TODO 此处假设 Primary Interactor 为数组中子一个元素，但尚未确认是否与底层 API 承诺一致
                    var primaryInteractor = grabInteractable.interactorsSelecting[0];
                    var primaryAttachTransform = primaryInteractor.GetAttachTransform(grabInteractable);
                    
                    var secondaryInteractor = grabInteractable.interactorsSelecting[1];
                    var secondaryAttachTransform = secondaryInteractor.GetAttachTransform(grabInteractable);
                    
                    var primaryProjection = primaryAttachTransform.TransformPoint(primaryPoseData.handProjection);
                    var secondaryProjection = secondaryAttachTransform.TransformPoint(secondaryPoseData.handProjection);
                    
                    if (grabInteractable.trackRotation)         // 首先计算旋转
                    {
                        var primaryFixShell = primaryHandController.interactableFixShell;
                        var secondaryFixShell = secondaryHandController.interactableFixShell;
                        
                        targetPose.rotation = Quaternion.LookRotation(secondaryProjection - primaryProjection, 
                            (primaryFixShell.forward /* TODO 主要手掌虎口上方向 */ + secondaryFixShell.up /* TODO 辅助手掌手心上方向 */) * 0.5f);      // 双手同时控制物体翻转方向

                        primaryHandController.interactableBindableShell.rotation = targetPose.rotation * Quaternion.Inverse(primaryRotationOffset);
                    }

                    if (grabInteractable.trackPosition)
                    {
                        targetPose.position = primaryProjection - primaryPoseData.handProjectionLength * targetPose.forward;
                    }
                    break;
            }
        }
        
        private void SetPrimaryInteractor(XRGrabInteractable grabInteractable, bool firstGrab = true)
        {
            var interactor = grabInteractable.interactorsSelecting[0];
            var interactablePose = grabInteractable.GetComponent<InteractablePose>();
            var attachTransform = grabInteractable.attachTransform;

            primaryHandController = interactor.transform.GetComponentInParent<HandController>();
            primaryPoseData = primaryHandController.side == HandSide.Left ? interactablePose.primaryLeftPose : interactablePose.primaryRightPose;
            
            var primaryActivatePose = primaryHandController.side == HandSide.Left ? interactablePose.primaryLeftActivatePose : interactablePose.primaryRightActivatePose;
            primaryHandController.SetPoseData(primaryPoseData, primaryActivatePose);
            attachTransform.SetLocalPositionAndRotation(-primaryPoseData.handLocalPosition, Quaternion.Inverse(primaryPoseData.handLocalRotation));
            
            var interactableAttachTransform = grabInteractable.GetAttachTransform(interactor);
            primaryRotationOffset = Quaternion.Inverse(Quaternion.Inverse(grabInteractable.transform.rotation) * interactableAttachTransform.rotation);

            if (firstGrab)
            {
                StartCoroutine(ResetAudioSource(audioSource.clip, readyAudio.length));
                audioSource.clip = readyAudio;
                audioSource.Play();
            }
        }

        private IEnumerator ResetAudioSource(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);

            audioSource.clip = clip;
        }
    }
}
