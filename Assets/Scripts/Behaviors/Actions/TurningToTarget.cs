using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using Cc83.Utils;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TurningToTarget : Action
    {
        public const float MinAngle = 45;
        
        [Range(1, 2)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float TimeOut = 2;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetTurn;

        // ReSharper disable once UnassignedField.Global
        public bool RandomAngle;
        
        // ReSharper disable once UnassignedField.Global
        public bool FastTurn;

        private Animator _animator;

        private TaskStatus _status;

        private Quaternion _targetRotation;

        private float _timeout;

        public override void OnAwake()
        {
            _animator = GetComponent<Animator>();
        }
        
        public override void OnStart()
        {
            Owner.RegisterEvent(BehaviorDefinitions.EventTurningStopped, OnTurningStopped);         // 目前偶尔情况：无法受到动画系统发出的事件；所以采用超时机制来预防这种错误
            
            _timeout = Time.time + TimeOut;

            var angle = 0f;

            if (RandomAngle)
            {
                angle = Random.Range(MinAngle, 180);
                angle *= Random.value > 0.5 ? 1 : -1;
            }
            else
            {
                var direction = TargetTurn.Value - transform.position;         // 不能使用 pathPoints[0]，因为路径点计算时位置可能已经与当下不一致（Walk To Stop）；或许也不是，需要时再考虑？
                angle = VectorUtils.DotDirectionalAngle2D(transform.forward, direction);
                // Debug.LogWarning($"TurningToTarget: {angle}");
            }
            
            if (Mathf.Abs(angle) + 0.1f >= MinAngle)               // +0.1f, 保证精度错误后依然满足条件
            {
                Debug.Log($"Turn: {angle}");
                _animator.SetFloat(AnimatorConstants.AnimatorTurn, angle);
                _animator.SetTrigger(FastTurn ? AnimatorConstants.AnimatorFastTurn : AnimatorConstants.AnimatorStartTurn);
                _status = TaskStatus.Running;
            }
            else
            {
                Debug.LogWarning($"Turning Ignored {angle}");
                _status = TaskStatus.Success;
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
            var prevRotationAngle = 0f;
            do
            {
                rotation = Quaternion.Lerp(rotation, targetRotation, Time.deltaTime * 10f);
                prevRotationAngle = rotationAngle;
                rotationAngle = Quaternion.Angle(rotation, targetRotation);
                transform.rotation = rotation;
            } while (prevRotationAngle > rotationAngle);
            
            _status = TaskStatus.Success;
        }
    }
}