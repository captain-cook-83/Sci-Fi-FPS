using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class TurningToTarget : Action
    {
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetPosition;

        public float TurnSpeed;
        
        private TaskStatus _status;
        
        public override void OnStart()
        {
            transform.LookAt(TargetPosition.Value);
        }

        public override TaskStatus OnUpdate()
        {
            return _status;
        }
    }
}
