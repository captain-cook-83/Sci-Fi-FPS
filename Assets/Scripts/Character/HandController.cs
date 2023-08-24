using Cc83.HandPose;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Character
{
    public class HandController : MonoBehaviour
    {
        private static readonly int ShootingShake = Animator.StringToHash("ShootingShake");
        
        public HandSide side;

        public HandSkeleton skeleton;

        public float activateAnimateSpeed = 20;

        public Animator shootingShakeAnimator;

        public Transform interactableBindableShell;

        public Transform interactableFixShell;

        [Tooltip("主要手掌虎口上方向")]
        public Vector3 primaryRotationAxis;
        
        [Tooltip("辅助手掌手心上方向")]
        public Vector3 secondaryRotationAxis;

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

        public void SetPoseData(HandPoseData selectPoseData, HandPoseData activatePoseData)
        {
            skeleton.SetPoseData(selectPoseData, activatePoseData, activateAnimateSpeed);
            
            ResetBindableShell();
        }

        public void ClearPoseData()
        {
            skeleton.ClearPoseData();
            
            ResetBindableShell();
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

        public void ResetBindableShell()
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
