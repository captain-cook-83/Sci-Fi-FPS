using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class FindEnemyTurn : Action
    {
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTarget Enemy;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetTurn;

        public override void OnStart()
        {
            TargetTurn.SetValue(Enemy.Value.targetAgent.transform.position);
        }

        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}