using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class KeepAimingToTarget : Action
    {
        private EnemyAttackController _attackController;

        public override void OnAwake()
        {
            _attackController = GetComponent<EnemyAttackController>();
        }

        public override TaskStatus OnUpdate()
        {
            _attackController.TickAiming(false);
            
            return TaskStatus.Running;
        }
    }
}