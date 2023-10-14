using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;
using Cc83.Utils;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class Attack : Action
    {
        // 左右两侧夹角之和，必须大于 TurningToTarget.MinAngle，否则会出现转向某一侧之后因不满足新的条件而立即转向另一侧的尴尬情况
        private const float LeftRetargetAngle = 35;
        private const float RightRetargetAngle = 15;
        
        [Range(0.5f, 5)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float MaxRepeatShootDelay = 3;
        
        // ReSharper disable once UnassignedField.Global
        public SharedFloat AttackFarDistance;
        
        // ReSharper disable once UnassignedField.Global
        public SharedFloat AttackNearDistance;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTarget Enemy;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetTurn;
        
        private EnemyAttackController _attackController;

        private SensorAgent.SensorTarget _sensorTarget;
        
        private Vector3 _currentDirection;

        private float _attackFarSqrDistance;

        private float _attackNearSqrDistance;

        public override void OnAwake()
        {
            _attackController = GetComponent<EnemyAttackController>();
            _attackFarSqrDistance = Mathf.Pow(AttackFarDistance.Value, 2);
            _attackNearSqrDistance = Mathf.Pow(AttackNearDistance.Value, 2);
        }

        public override void OnStart()
        {
            _sensorTarget = Enemy.Value;
            _currentDirection = _sensorTarget.direction;
            _attackController.Active(_sensorTarget, MaxRepeatShootDelay);
        }

        public override TaskStatus OnUpdate()
        {
            var sqrDistance = Vector3.SqrMagnitude(_sensorTarget.targetAgent.transform.position - transform.position);
            if (sqrDistance > _attackFarSqrDistance)
            {
                _attackController.Reset();
                return TaskStatus.Failure;
            }
            
            var targetDirection = _sensorTarget.direction;
            if (!targetDirection.Equals(_currentDirection))     // 在目标未移动的情况下，_currentDirection 可以做到精准 Equals；而当前 NPC 的 forward 做不到这一点，从而无法进行当前检测优化
            {
                var directionalAngle = VectorUtils.DotDirectionalAngle2D(transform.forward, targetDirection);
                if (Mathf.Abs(directionalAngle) > (directionalAngle < 0 ? LeftRetargetAngle : RightRetargetAngle))
                {
                    const float angle = TurningToTarget.MinAngle;
                    var rotation = Quaternion.AngleAxis(directionalAngle < 0 ? -angle : angle, Vector3.up);
                    var targetPosition = transform.position + rotation * transform.forward;
                    TargetTurn.SetValue(targetPosition);
                    return TaskStatus.Success;
                }
            }
            
            _attackController.Tick();
            return TaskStatus.Running;
        }

        public override void OnConditionalAbort()
        {
            _attackController.Reset();
        }
    }
}
