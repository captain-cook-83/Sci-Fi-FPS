using Cc83.HandPose;
using UnityEngine;

namespace Cc83.Character
{
    public class HandController : MonoBehaviour
    {
        public HandSide side;

        public HandSkeleton skeleton;

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

        public void SetPoseData(HandPoseData data)
        {
            skeleton.SetPoseData(data);
            skeleton.enabled = false;
        }

        public void ClearPoseData()
        {
            skeleton.enabled = true;
            skeleton.ClearPoseData();
        }
    }
}
