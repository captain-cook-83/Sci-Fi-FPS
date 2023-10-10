using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class AsyncTurningToTarget : Action
    {
        // ReSharper disable once UnassignedField.Global
        public SharedVector3List PathPoints;

        private Animator _animator;

        private TaskStatus _status;

        public override void OnAwake()
        {
            _animator = GetComponent<Animator>();
        }

        public override void OnStart()
        {
            var pathPoints = PathPoints.Value;
            
            var forward = transform.forward;
            var direction = pathPoints[1] - transform.position;         // 不能使用 pathPoints[0]，因为路径点计算时位置可能已经与当下不一致（Walk To Stop）；或许也不是，需要时再考虑？
            var forward2 = new Vector2(forward.x, forward.z);
            var direction2 = new Vector2(direction.x, direction.z);
            
            var angle = Vector3.Angle(forward2, direction2);
            if (angle > 20)
            {
                var dot = Vector2.Dot(new Vector2(forward2.y, -forward2.x), direction2);       // Left(-) or Right(+)
                _animator.SetFloat(AnimatorConstants.AnimatorTurn, dot > 0 ? angle : -angle);
                _status = TaskStatus.Success;
            }
            else
            {
                _status = TaskStatus.Failure;
            }
        }

        public override TaskStatus OnUpdate()
        {
            return _status;
        }
    }
}
