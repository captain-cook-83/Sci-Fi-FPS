using System;
using Cc83.HandPose;
using EZCameraShake;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Character
{
    public class HandController : MonoBehaviour
    {
        private static readonly int ShootingShake = Animator.StringToHash("ShootingShake");
        private static readonly int StopShaking = Animator.StringToHash("StopShake");
        
        public HandSide side;

        public float activateAnimateSpeed = 20;

        [Tooltip("主要手掌虎口上方向")]
        public Vector3 primaryRotationAxis;
        
        [Tooltip("辅助手掌手心上方向")]
        public Vector3 secondaryRotationAxis;
        
        public Transform interactableBindableShell;

        public Transform interactableFixShell;

        public Action Interrupted;
        
        [SerializeField]
        private HandSkeleton skeleton;
        
        [SerializeField]
        private Animator shootingShakeAnimator;
        
        [SerializeField]
        private CameraShaker waggleShaker;

        private bool _isPrimaryFromMultiGrab;

        private void OnValidate()
        {
            if (skeleton != null)
            {
                foreach (var handSkeleton in skeleton.transform.GetComponents<HandSkeleton>())
                {
                    if (handSkeleton.handSide == side)
                    {
                        skeleton = handSkeleton;
                        Debug.Log($"Auto select component for HandSide({side})");
                        return;
                    }
                }

                skeleton = null;
            }
        }

        public void WaggleShake()
        {
            if (!_isPrimaryFromMultiGrab && waggleShaker)
            {
                waggleShaker.ShakeOnce(0.1f, 5, 0.1f, 1);
            }
        }

        public void OnCatchInteractable()
        {
            AkSoundEngine.PostEvent(AK.EVENTS.CATCH_PISTOL_PLAYER, gameObject);
        }

        public void SetPoseData(HandPoseData selectPoseData, HandPoseData activatePoseData)
        {
            skeleton.SetPoseData(selectPoseData, activatePoseData, activateAnimateSpeed);
            
            OnReleaseFromMultiGrab();
        }

        public void ClearPoseData()
        {
            skeleton.ClearPoseData();
            
            OnReleaseFromMultiGrab();
            
            if (shootingShakeAnimator)
            {
                shootingShakeAnimator.runtimeAnimatorController = null;
            }
        }

        public void SetAnimatorController(AnimatorController animatorController)
        {
            if (shootingShakeAnimator)
            {
                shootingShakeAnimator.runtimeAnimatorController = animatorController;
            }
        }

        public void Shake()
        {
            if (shootingShakeAnimator)
            {
                shootingShakeAnimator.SetTrigger(ShootingShake);
            } else
            {
                Debug.LogWarning($"Missing hand shaking animation for {name}");
            }
        }

        public void StopShake()
        {
            if (shootingShakeAnimator)
            {
                shootingShakeAnimator.SetTrigger(StopShaking);
            } else
            {
                Debug.LogWarning($"Missing hand shaking animation for {name}");
            }
        }

        public void OnPrimaryFromMultiGrab()
        {
            _isPrimaryFromMultiGrab = true;
        }

        public void OnReleaseFromMultiGrab()
        {
            interactableBindableShell.localRotation = Quaternion.identity;
            interactableBindableShell.localPosition = Vector3.zero;

            _isPrimaryFromMultiGrab = false;
        }

#if UNITY_EDITOR
        [Button("Set Grab Anchor from VRIK", ButtonSizes.Large)]
        public void AutoDetectHandGrabAnchor()
        {
            var ik = skeleton.GetComponentInParent<VRIK>();
            var handTransform = side == HandSide.Left ? ik.references.leftHand : ik.references.rightHand;
            
            var directInteractor = GetComponentInChildren<XRDirectInteractor>();
            if (directInteractor)
            {
                directInteractor.attachTransform = handTransform;
                Debug.Log($"Set '{directInteractor.name}'.attachTransform to '{handTransform.name}' via VRIK references.");
            }
            
            foreach (var interactor in GetComponentsInChildren<XRRayInteractor>())
            {
                if (interactor.useForceGrab)
                {
                    interactor.attachTransform = handTransform;
                    Debug.Log($"Set '{interactor.name}'.attachTransform to '{handTransform.name}' via VRIK references.");
                }
            }
        }
#endif
    }
}
