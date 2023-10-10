using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class TurningToTarget : Action
    {
        [Range(1, 2)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float TimeOut = 2;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3List PathPoints;

        private Animator _animator;

        private TaskStatus _status;

        private float _timeout;

        public override void OnAwake()
        {
            _animator = GetComponent<Animator>();
        }

        public override void OnStart()
        {
            Owner.RegisterEvent(BehaviorDefinitions.EventTurningStopped, OnTurningStopped);         // 目前偶尔情况：无法受到动画系统发出的事件；所以采用超时机制来预防这种错误
            
            _status = TaskStatus.Running;
            _timeout = Time.time + TimeOut;
            
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
            }
        }

        public override TaskStatus OnUpdate()
        {
            if (Time.time < _timeout) return _status;
            
            Debug.LogWarning($"Missing Behavior Event: {BehaviorDefinitions.EventTurningStopped}");
            return TaskStatus.Failure;
        }

        public override void OnEnd()
        {
            Owner.UnregisterEvent(BehaviorDefinitions.EventTurningStopped, OnTurningStopped);
        }

        private void OnTurningStopped()
        {
            _status = TaskStatus.Success;
        }
    }
}