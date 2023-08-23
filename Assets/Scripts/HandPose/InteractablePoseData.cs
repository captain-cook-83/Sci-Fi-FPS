using UnityEngine;

namespace Cc83.HandPose
{
    public class InteractablePoseData : HandPoseData
    {
        public Vector3 handProjection;

        public float handProjectionLength;
        
        public Vector3 handLocalPosition;

        public Quaternion handLocalRotation;
    }
}
