using UnityEngine;

namespace Cc83
{
    public static class AnimatorConstants
    {
        public static readonly int AnimatorTensity = Animator.StringToHash("Tensity");
        public static readonly int AnimatorSpeed = Animator.StringToHash("Speed");
        public static readonly int AnimatorMoving = Animator.StringToHash("Moving");
        public static readonly int AnimatorCrouching = Animator.StringToHash("Crouching");
        public static readonly int AnimatorDeath = Animator.StringToHash("Death");
        public static readonly int AnimatorTurn = Animator.StringToHash("Turn");
        public static readonly int AnimatorStartTurn = Animator.StringToHash("StartTurn");          // Trigger
        public static readonly int AnimatorFastTurn = Animator.StringToHash("FastTurn");          // Trigger
        public static readonly int AnimatorStop = Animator.StringToHash("Stop");                    // Trigger
        
        public const float MinimumTensity = -2;
        public const float WalkTensity = -1;
        public const float AimingTensity = 0.5f;
        public const float MaximumTensity = 1f;

        public const float InvalidTurn = 0;
    }
}
