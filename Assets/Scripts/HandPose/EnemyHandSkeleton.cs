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
            
            // 需要注意：如果 Animator 所使用 Avatar 中 LeftHand（或 RightHand）采用了默认关联的手指骨骼，则运行时 Animator 将采用 FBX 中的骨骼默认值覆盖此处设置的手指姿态。
            // 解决方法：1、使用 LateUpdate 生命周期回调来持续刷新手指姿态；2、删除 Avatar 配置中 LeftHand（和 RightHand）中的全部骨骼引用（推荐）
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
