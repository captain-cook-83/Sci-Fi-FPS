using System.Collections;
using Cc83.Character;
using Cc83.HandPose;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace Cc83.Interactable
{
    public class InteractablePoseGrabTransformer : XRBaseGrabTransformer
    {
        public AudioClip readyAudio;

        private AudioSource audioSource;
        
        public override void OnLink(XRGrabInteractable grabInteractable)
        {
            base.OnLink(grabInteractable);
            
            var attachTransform = grabInteractable.attachTransform;
            if (!ReferenceEquals(attachTransform.parent, grabInteractable.transform))
            {
                Debug.LogError($"AttachTransform({attachTransform.name}) must be a direct child of the grabInteractable({grabInteractable.name})");
            }

            audioSource = GetComponent<AudioSource>();
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

            StartCoroutine(ResetAudioSource(audioSource.clip, readyAudio.length));
            audioSource.clip = readyAudio;
            audioSource.Play();
        }
        
        public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
        {
            if (grabInteractable.interactorsSelecting.Count != 2) return;
            
            var interactor = grabInteractable.interactorsSelecting[1];
            var handController = interactor.transform.GetComponentInParent<HandController>();
            
            var interactablePose = grabInteractable.GetComponent<InteractablePose>();
            var secondaryPose = handController.side == HandSide.Left ? interactablePose.secondaryLeftPose : interactablePose.secondaryRightPose;
            var attachTransform = grabInteractable.secondaryAttachTransform;
            
            handController.SetPoseData(secondaryPose, secondaryPose);
            attachTransform.SetLocalPositionAndRotation(-secondaryPose.handLocalPosition, Quaternion.Inverse(secondaryPose.handLocalRotation));
            
            // TODO 切换震动动画
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

        private IEnumerator ResetAudioSource(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);

            audioSource.clip = clip;
        }
    }
}
