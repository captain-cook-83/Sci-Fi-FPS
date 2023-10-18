using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class KeepAimingToTarget : Action
    {
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTarget Enemy;
        
        // ReSharper disable once UnassignedField.Global
        public SharedFloat MaxRepeatShootDelay;
        
        private EnemyAttackController _attackController;

        public override void OnAwake()
        {
            _attackController = GetComponent<EnemyAttackController>();
        }

        public override void OnStart()
        {
            _attackController.Active(Enemy.Value, MaxRepeatShootDelay.Value);
        }

        public override TaskStatus OnUpdate()
        {
            _attackController.TickAiming();
            
            return TaskStatus.Running;
        }

        public override void OnEnd()
        {
            _attackController.Reset();
        }
    }
}