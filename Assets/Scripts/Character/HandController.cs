using System;
using Cc83.HandPose;
using EZCameraShake;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;
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

        public Transform AttachTransform => directInteractor.attachTransform;
        
        [SerializeField]
        private HandSkeleton skeleton;
        
        [SerializeField]
        private Animator shootingShakeAnimator;
        
        [SerializeField]
        private CameraShaker waggleShaker;

        [SerializeField]
        private XRDirectInteractor directInteractor;

        private bool _isPrimaryFromMultiGrab;

        private XRBaseControllerInteractor _activeInteractor;

        private bool _activeInteractorActiveAllowed = true;

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
            
            OnReleased();
        }

        public void ClearPoseData()
        {
            skeleton.ClearPoseData();
            
            OnReleased();
            
            if (shootingShakeAnimator)
            {
                shootingShakeAnimator.runtimeAnimatorController = null;
            }
        }

        public void SetAnimatorController(RuntimeAnimatorController animatorController)
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

        public void OnPrimaryFromMultiGrab(XRBaseControllerInteractor interactor)
        {
            _activeInteractor = interactor;
            _isPrimaryFromMultiGrab = true;
        }

        public void OnSecondaryFromMultiGrab(XRBaseControllerInteractor interactor)
        {
            _activeInteractor = interactor;
            _activeInteractorActiveAllowed = _activeInteractor.allowActivate;
            _activeInteractor.allowActivate = false;
        }

        public void OnReleaseFromMultiGrab()
        {
            OnReleased();

            if (!_isPrimaryFromMultiGrab)
            {
                _activeInteractor.allowActivate = _activeInteractorActiveAllowed;
                _activeInteractorActiveAllowed = true;
            }
            
            _activeInteractor = null;
            _isPrimaryFromMultiGrab = false;
        }

        private void OnReleased()
        {
            interactableBindableShell.localRotation = Quaternion.identity;
            interactableBindableShell.localPosition = Vector3.zero;
        }

#if UNITY_EDITOR
        [Button("Set Grab Anchor from VRIK", ButtonSizes.Large)]
        public void AutoDetectHandGrabAnchor()
        {
            var ik = skeleton.GetComponentInParent<VRIK>();
            var handTransform = side == HandSide.Left ? ik.references.leftHand : ik.references.rightHand;
            
            directInteractor = GetComponentInChildren<XRDirectInteractor>();
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
