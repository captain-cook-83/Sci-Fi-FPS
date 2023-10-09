using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class DetectForwardWall : Action
    {
        [Range(0.5f, 5)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float RaycastDistance = 2;
        
        [Range(0.1f, 1)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float RaycastInterval = 0.5f;

        private Animator _animator;

        private float _tickTime;

        public override void OnStart()
        {
            _animator = GetComponent<Animator>();
        }

        public override TaskStatus OnUpdate()
        {
            var currentTime = Time.time;
            if (currentTime > _tickTime)
            {
                _tickTime = currentTime + RaycastInterval;

                if (Physics.Raycast(transform.position + Vector3.up, transform.forward, RaycastDistance, Definitions.MovingObstacleLayerMask))
                {
                    _animator.SetTrigger(AnimatorConstants.AnimatorStop);
                    _animator.SetBool(AnimatorConstants.AnimatorMoving, false);
                    Debug.LogWarning("Walking interrupted via raycast detection");

                    return TaskStatus.Success;
                }
            }

            return TaskStatus.Running;
        }
    }
}
