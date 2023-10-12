using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class Attack : Action
    {
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
        public SharedSensorTargetList Enemies;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetTurn;

        private EnemyAttackController _attackController;

        public override void OnAwake()
        {
            _attackController = GetComponent<EnemyAttackController>();
        }

        public override void OnStart()
        {
            _attackController.Active(Enemies.Value[0], AttackNearDistance.Value, AttackFarDistance.Value, MaxRepeatShootDelay);
        }

        public override TaskStatus OnUpdate()
        {
            var dotDirection = _attackController.Tick();
            if (dotDirection == 0) return TaskStatus.Running;

            const float angle = TurningToTarget.MinAngle;
            var rotation = Quaternion.AngleAxis(dotDirection < 0 ? -angle : angle, Vector3.up);
            var targetPosition = transform.position + rotation * transform.forward;
            TargetTurn.SetValue(targetPosition);
            return TaskStatus.Success;
        }

        public override void OnEnd()
        {
            _attackController.Deactive();
        }
    }
}
