using UnityEngine;

namespace Cc83.Behaviors
{
    public class PatrolToTarget : MoveToTarget
    {
        public override void OnStart()
        {
            base.OnStart();
            
            AnimatorStateController.ChangeTensity(Random.Range(AnimatorConstants.AimingTensity, AnimatorConstants.MaximumTensity));            // Tensity 直到移动之前才设置，避免之前的先举枪再转身的不自然表现
        }
    }
}