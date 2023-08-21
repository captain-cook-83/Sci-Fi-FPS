using Cc83.Interactable;
using UnityEngine;

namespace Cc83.HandPose
{
    public partial class EnemyHandSkeleton : MonoBehaviour
    {
        public HandSide handSide;
        
        public InteractablePoseData defaultPoseData;
        
        public Transform[] fingerNodes;

        public WeaponRifle weaponRifle;

        private void Start()
        {
#if UNITY_EDITOR
            if (defaultPoseData == null) return;
#endif
            
            switch (handSide)
            {
                case HandSide.Left:
                    weaponRifle.RefreshSecondaryAnchor(defaultPoseData.handLocalPosition, defaultPoseData.handLocalRotation);
                    break;
                case HandSide.Right:
                    weaponRifle.RefreshPrimaryAnchor(defaultPoseData.handLocalPosition, defaultPoseData.handLocalRotation);
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

        private void SetFingerNodes(HandPoseData data)
        {
            for (var i = 0; i < fingerNodes.Length; i++)
            {
                fingerNodes[i].localRotation = data.rotations[i];
            }
        }
    }
}
