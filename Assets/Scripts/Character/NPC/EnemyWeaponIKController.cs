using UnityEngine;

namespace Cc83.Character
{
    [RequireComponent(typeof(WeaponReference))]
    public class EnemyWeaponIKController : MonoBehaviour
    {
        private const float StopFactor = 0.1f;
        
        public Transform aimingAxis;

        public Transform aimTowards;
        
        public bool aimingActive;

        private WeaponReference weaponReference;
        
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
        public float lerpSpeed = 8;
        
        [SerializeField]
        private bool primaryIkValue = true;
        [SerializeField]
        private bool secondaryIkValue = true;

        private float primaryLerpWeight;
        private float secondaryLerpWeight;
        
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            weaponReference = GetComponent<WeaponReference>();
            
            primaryLerpWeight = primaryIkValue ? 1 : 0;
            secondaryLerpWeight = secondaryIkValue ? 1 : 0;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (aimingActive)
            {
                aimingAxis.LookAt(aimTowards.position);
            }
            
            if (layerIndex == activeLayerIndex)
            {
                var weight = animator.GetLayerWeight(layerIndex);
                if (weight > 0)
                {
                    if (primaryIkValue)
                    {
                        if (weight > primaryLerpWeight + StopFactor)
                        {
                            primaryLerpWeight = Mathf.Lerp(primaryLerpWeight, weight, Time.deltaTime * lerpSpeed);
                            weight = primaryLerpWeight;
                        }
                        
                        SetHandIK(AvatarIKGoal.RightHand, weaponReference.weapon.primaryAnchor, weight);
                    }

                    if (secondaryIkValue)
                    {
                        if (weight > secondaryLerpWeight + StopFactor)
                        {
                            secondaryLerpWeight = Mathf.Lerp(secondaryLerpWeight, weight, Time.deltaTime * lerpSpeed);
                            weight = secondaryLerpWeight;
                        }
                        
                        SetHandIK(AvatarIKGoal.LeftHand, weaponReference.weapon.secondaryAnchor, weight);
                    }
                }
            }
        }
        
        private void SetHandIK(AvatarIKGoal ikGoal, Transform anchor, float weight)
        {
            animator.SetIKPositionWeight(ikGoal,weight);
            animator.SetIKRotationWeight(ikGoal,weight);
            animator.SetIKPosition(ikGoal, anchor.position);
            animator.SetIKRotation(ikGoal, anchor.rotation);
        }
    }
}
