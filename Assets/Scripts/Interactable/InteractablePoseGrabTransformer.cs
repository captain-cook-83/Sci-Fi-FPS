using System.Collections;
using Cc83.Character;
using Cc83.HandPose;
using Unity.Mathematics;
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
                    }
                    
                    if (secondaryHandController)                // means from 2 to 1
                    {
                        secondaryHandController.ResetBindableShell();
                        secondaryHandController = null;
                        
                        SetPrimaryInteractor(grabInteractable, false);
                    }
                    break;
                case 2:
                    var secondaryInteractor = grabInteractable.interactorsSelecting[1];
                    secondaryHandController = secondaryInteractor.transform.GetComponentInParent<HandController>();
            
                    var interactablePose = grabInteractable.GetComponent<InteractablePose>();
                    var secondaryPose = secondaryHandController.side == HandSide.Left ? interactablePose.secondaryLeftPose : interactablePose.secondaryRightPose;
                    var secondaryAttachTransform = grabInteractable.secondaryAttachTransform;
            
                    secondaryHandController.SetPoseData(secondaryPose, secondaryPose);
                    secondaryAttachTransform.SetLocalPositionAndRotation(-secondaryPose.handLocalPosition, Quaternion.Inverse(secondaryPose.handLocalRotation));
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
                    var interactableAttachTransform = grabInteractable.GetAttachTransform(interactor);
                    
                    if (grabInteractable.trackRotation)
                    {
                        var interactableTransform = grabInteractable.transform;
                        var rotationOffset = Quaternion.Inverse(Quaternion.Inverse(interactableTransform.rotation) * interactableAttachTransform.rotation);
                        targetPose.rotation = interactorAttachTransform.rotation * rotationOffset;
                    }
                    
                    if (grabInteractable.trackPosition)
                    {
                        targetPose.position = interactorAttachTransform.TransformPoint(-interactableAttachTransform.localPosition);
                    }
                    break;
                case 2:         // TODO 此处假设 Primary Interactor 为数组中子一个元素，但尚未确认是否与底层 API 承诺一致
                    var primaryInteractor = grabInteractable.interactorsSelecting[0];
                    var primaryAttachTransform = primaryInteractor.GetAttachTransform(grabInteractable);
                    var primaryInteractableAttachTransform = grabInteractable.GetAttachTransform(primaryInteractor);
                    var primaryHandTransform = primaryHandController.interactableBindableShell;
                    
                    var secondaryInteractor = grabInteractable.interactorsSelecting[1];
                    var secondaryAttachTransform = secondaryInteractor.GetAttachTransform(grabInteractable);
                    var secondaryInteractableAttachTransform = grabInteractable.GetAttachTransform(secondaryInteractor);
                    var secondaryHandTransform = secondaryHandController.interactableBindableShell;

                    if (grabInteractable.trackRotation)         // 首先计算旋转
                    {
                        var direction = secondaryAttachTransform.TransformPoint(new Vector3(-0.08f, 0.06f, 0)) - primaryAttachTransform.position;
                        targetPose.rotation = Quaternion.LookRotation(direction, (secondaryAttachTransform.up /* TODO 辅助手掌手心上方向 */ + primaryAttachTransform.forward /* TODO 主要手掌虎口上方向 */) * 0.5f);      // 双手同时控制物体翻转方向
                    }

                    if (grabInteractable.trackPosition)
                    {
                        targetPose.position = primaryAttachTransform.position +
                                              primaryInteractableAttachTransform.localPosition.magnitude * targetPose.forward;
                    }
                    break;
            }
        }
        
        private void SetPrimaryInteractor(XRGrabInteractable grabInteractable, bool firstGrab = true)
        {
            var interactor = grabInteractable.interactorsSelecting[0];
            primaryHandController = interactor.transform.GetComponentInParent<HandController>();
            
            var interactablePose = grabInteractable.GetComponent<InteractablePose>();
            var primaryPose = primaryHandController.side == HandSide.Left ? interactablePose.primaryLeftPose : interactablePose.primaryRightPose;
            var primaryActivatePose = primaryHandController.side == HandSide.Left ? interactablePose.primaryLeftActivatePose : interactablePose.primaryRightActivatePose;
            var attachTransform = grabInteractable.attachTransform;
            
            primaryHandController.SetPoseData(primaryPose, primaryActivatePose);
            attachTransform.SetLocalPositionAndRotation(-primaryPose.handLocalPosition, Quaternion.Inverse(primaryPose.handLocalRotation));

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
