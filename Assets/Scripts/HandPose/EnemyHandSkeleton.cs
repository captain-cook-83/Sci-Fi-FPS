using System.Runtime.CompilerServices;
using Cc83.Character;
using UnityEngine;

namespace Cc83.HandPose
{
    [RequireComponent(typeof(WeaponReference))]
    public partial class EnemyHandSkeleton : MonoBehaviour
    {
        public HandSide handSide;
        
        public Transform[] fingerNodes;

        public InteractablePoseData defaultPoseData;
        
        private WeaponReference weaponReference;

        private void Awake()
        {
            weaponReference = GetComponent<WeaponReference>();
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (defaultPoseData == null) return;
#endif
            
            switch (handSide)
            {
                case HandSide.Left:
                    weaponReference.weapon.RefreshSecondaryAnchor(defaultPoseData.handLocalPosition, defaultPoseData.handLocalRotation);
                    break;
                case HandSide.Right:
                    weaponReference.weapon.RefreshPrimaryAnchor(defaultPoseData.handLocalPosition, defaultPoseData.handLocalRotation);
                    break;
            }
        }

        private void LateUpdate()           // TODO 寻找一次性设置的方式 或 DOTS
        {
#if UNITY_EDITOR
            if (defaultPoseData == null) return;
#endif
            
            SetFingerNodes(defaultPoseData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetFingerNodes(HandPoseData data)
        {
            for (var i = 0; i < fingerNodes.Length; i++)
            {
                fingerNodes[i].localRotation = data.rotations[i];
            }
        }
    }
}
