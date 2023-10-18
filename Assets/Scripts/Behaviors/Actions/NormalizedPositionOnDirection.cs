using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class NormalizedPositionOnDirection : Action
    {
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 DodgeDirection;

        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetTurn;

        public bool RevertDirection;

        public override void OnStart()
        {
            var direction = DodgeDirection.Value.normalized;
            TargetTurn.SetValue(transform.position + (RevertDirection ? -direction : direction));
        }
        
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}