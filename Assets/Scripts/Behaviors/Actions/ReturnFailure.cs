using BehaviorDesigner.Runtime.Tasks;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class ReturnFailure : Action
    {
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Failure;
        }
    }
}