using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class FindAttackPosition : Action
    {
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTargetList Enemies;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetPosition;

        public override TaskStatus OnUpdate()
        {
            var sensorTargets = Enemies.Value;
            TargetPosition.SetValue(sensorTargets[0].TargetAgent.transform.position);
            
            return TaskStatus.Success;
        }
    }
}
