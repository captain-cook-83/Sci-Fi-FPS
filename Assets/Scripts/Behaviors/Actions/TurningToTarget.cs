using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class TurningToTarget : Action
    {
        [Range(1, 2)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once ConvertToConstant.Global
        public float Duration = 2;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3List PathPoints;

        private Animator _animator;

        private float _successTime;

        public override void OnAwake()
        {
            _animator = GetComponent<Animator>();
        }

        public override void OnStart()
        {
            var pathPoints = PathPoints.Value;
            if (pathPoints.Count < 2) return;

            
            var forward = transform.forward;
            var direction = pathPoints[1] - transform.position;         // 不能使用 pathPoints[0]，因为路径点计算时位置可能已经与当下不一致（Walk To Stop）；或许也不是，需要时再考虑？
            var forward2 = new Vector2(forward.x, forward.z);
            var direction2 = new Vector2(direction.x, direction.z);
            
            var angle = Vector3.Angle(forward2, direction2);
            if (angle > 15)
            {
                var dot = Vector2.Dot(new Vector2(forward2.y, -forward2.x), direction2);       // Left(-) or Right(+)
                _animator.SetFloat(AnimatorConstants.AnimatorTurn, dot > 0 ? angle : -angle);
                _successTime = Time.time + Duration;
            }
        }

        public override TaskStatus OnUpdate()
        {
            return Time.time < _successTime ? TaskStatus.Running : TaskStatus.Success;
        }
    }
}
