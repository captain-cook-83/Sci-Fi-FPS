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
            return _attackController.IsActive ? TaskStatus.Running : TaskStatus.Failure;
        }

        public override void OnEnd()
        {
            _attackController.Deactive();
        }
    }
}
