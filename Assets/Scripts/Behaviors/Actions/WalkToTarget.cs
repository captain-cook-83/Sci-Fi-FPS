using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Cc83.Behaviors
{
    public class WalkToTarget : MoveToTarget
    {
        [Range(0.5f, 5)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float RaycastDistance = 1;

        [Range(0.1f, 1)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float RaycastInterval = 0.1f;

        private float _tickTime;
        
        protected override int Tensity => AnimatorConstants.MinimumTensity;
        
        protected override bool StopMoveImmediately => true;

        public override void OnStart()
        {
            base.OnStart();
            
            _tickTime = Time.time + 1;      // 1s 之后开始检测
        }
        
        public override TaskStatus OnUpdate()
        {
            var currentTime = Time.time;
            if (currentTime > _tickTime)
            {
                _tickTime = currentTime + RaycastInterval;

                if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out var hitInfo, RaycastDistance,
                        Definitions.MovingObstacleLayerMask))
                {
                    Animator.SetTrigger(AnimatorConstants.AnimatorStop);
                    Animator.SetBool(AnimatorConstants.AnimatorMoving, false);
                    Debug.LogWarning("Walking interrupted via raycast detection");

                    Status = TaskStatus.Success;
                }
            }
            
            return base.OnUpdate();
        }
    }
}
