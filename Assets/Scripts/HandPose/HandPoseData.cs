using UnityEngine;

namespace Cc83.HandPose
{
    public class HandPoseData : ScriptableObject
    {
        public HandSide side;
        
        public Quaternion[] rotations;
    }

    public enum HandSide
    {
        Left, Right
    }
}
