using UnityEngine;

namespace Cc83.Character
{
    [RequireComponent(typeof(WeaponReference))]
    public class EnemyWeaponIKController : MonoBehaviour
    {
        private const float StopFactor = 0.01f;
        
        public Transform aimingAxis;

        public Transform aimTowards;
        
        public bool aimingActive;

        private WeaponReference _weaponReference;
        
        public bool primaryIk
        {
            get => primaryIkValue;
            set
            {
                primaryIkValue = value;

                if (value)
                {
                    primaryLerpWeight = 0;
                }
            }
        }
        
        public bool secondaryIk
        {
            get => secondaryIkValue;
            set
            {
                secondaryIkValue = value;

                if (value)
                {
                    secondaryLerpWeight = 0;
                }
            }
        }
        
        public int activeLayerIndex;

        [Range(4, 16)]
        public float lerpSpeed = 14;
        
        [SerializeField]
        private bool primaryIkValue = true;
        [SerializeField]
        private bool secondaryIkValue = true;

        private float primaryLerpWeight;
        private float secondaryLerpWeight;
        private float aimLerpWeight;
        
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            _weaponReference = GetComponent<WeaponReference>();
            
            primaryLerpWeight = primaryIkValue ? 1 : 0;
            secondaryLerpWeight = secondaryIkValue ? 1 : 0;
        }
        
        private void OnAnimatorIK(int layerIndex)
        {
            if (layerIndex == activeLayerIndex)
            {
                var weight = animator.GetLayerWeight(layerIndex);
                
                var primaryWeight = primaryIkValue ? weight : 0;
                if (Mathf.Abs(primaryWeight - primaryLerpWeight) > StopFactor)
                {
                    primaryLerpWeight = Mathf.Lerp(primaryLerpWeight, primaryWeight, Time.deltaTime * lerpSpeed);
                    primaryWeight = primaryLerpWeight;
                }

                if (primaryWeight > 0)
                {
                    SetHandIK(AvatarIKGoal.RightHand, _weaponReference.weapon.primaryAnchor, primaryWeight);
                }
                
                var secondaryWeight = secondaryIkValue ? weight : 0;
                if (Mathf.Abs(secondaryWeight - secondaryLerpWeight) > StopFactor)
                {
                    secondaryLerpWeight = Mathf.Lerp(secondaryLerpWeight, secondaryWeight, Time.deltaTime * lerpSpeed);
                    secondaryWeight = secondaryLerpWeight;
                }

                if (secondaryWeight > 0)
                {
                    SetHandIK(AvatarIKGoal.LeftHand, _weaponReference.weapon.secondaryAnchor, secondaryWeight);
                }
                
                var aimingWeight = aimingActive ? weight : 0;
                if (Mathf.Abs(aimingWeight - aimLerpWeight) > StopFactor)
                {
                    aimLerpWeight = Mathf.Lerp(aimLerpWeight, aimingWeight, Time.deltaTime * lerpSpeed);
                    aimingWeight = aimLerpWeight;
                }

                if (aimingWeight > 0)
                {
                    var targetRotation = Quaternion.LookRotation(aimingAxis.parent.InverseTransformPoint(aimTowards.position));
                    aimingAxis.localRotation = Quaternion.Lerp(Quaternion.identity, targetRotation, aimingWeight);
                }
                else
                {
                    aimingAxis.localRotation = Quaternion.identity;
                }
            }
        }

        // private void OnAnimatorIK(int layerIndex)
        // {
        //     if (layerIndex == activeLayerIndex)
        //     {
        //         var weight = animator.GetLayerWeight(layerIndex);
        //         if (weight > 0)
        //         {
        //             if (primaryIkValue)
        //             {
        //                 var primaryWeight = weight;
        //                 if (primaryWeight > primaryLerpWeight + StopFactor)
        //                 {
        //                     primaryLerpWeight = Mathf.Lerp(primaryLerpWeight, primaryWeight, Time.deltaTime * lerpSpeed);
        //                     primaryWeight = primaryLerpWeight;
        //                 }
        //                 
        //                 SetHandIK(AvatarIKGoal.RightHand, weaponReference.weapon.primaryAnchor, primaryWeight);
        //             }
        //
        //             if (secondaryIkValue)
        //             {
        //                 var secondaryWeight = weight;
        //                 if (secondaryWeight > secondaryLerpWeight + StopFactor)
        //                 {
        //                     secondaryLerpWeight = Mathf.Lerp(secondaryLerpWeight, secondaryWeight, Time.deltaTime * lerpSpeed);
        //                     secondaryWeight = secondaryLerpWeight;
        //                 }
        //                 
        //                 SetHandIK(AvatarIKGoal.LeftHand, weaponReference.weapon.secondaryAnchor, secondaryWeight);
        //             }
        //             
        //             if (aimingActive)
        //             {
        //                 var aimingWeight = weight;
        //                 if (aimingWeight > aimLerpWeight + StopFactor)
        //                 {
        //                     aimLerpWeight = Mathf.Lerp(aimLerpWeight, aimingWeight, Time.deltaTime * lerpSpeed);
        //                     aimingWeight = aimLerpWeight;
        //                 }
        //                 
        //                 var targetRotation = Quaternion.LookRotation(aimTowards.position - aimingAxis.position);
        //                 aimingAxis.rotation = Quaternion.Lerp(Quaternion.identity, targetRotation, aimingWeight);
        //                 // aimingAxis.LookAt(aimTowards.position);
        //             }
        //         }
        //     }
        // }
        
        private void SetHandIK(AvatarIKGoal ikGoal, Transform anchor, float weight)
        {
            animator.SetIKPositionWeight(ikGoal,weight);
            animator.SetIKRotationWeight(ikGoal,weight);
            animator.SetIKPosition(ikGoal, anchor.position);
            animator.SetIKRotation(ikGoal, anchor.rotation);
        }
    }
}
