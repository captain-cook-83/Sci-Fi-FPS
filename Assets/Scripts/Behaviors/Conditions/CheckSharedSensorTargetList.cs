using BehaviorDesigner.Runtime.Tasks;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class CheckSharedSensorTargetList : Conditional
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTargetList SensorTargetList;

        // ReSharper disable once UnassignedField.Global
        public bool IsEmpty;
        
        public override TaskStatus OnUpdate()
        {
            var targetList = SensorTargetList.Value;
            return (targetList == null || targetList.Count == 0) == IsEmpty ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
