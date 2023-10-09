using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class ClearSharedVariable : Action
    {
        [RequiredField]
        // ReSharper disable once UnassignedField.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public SharedVariable SharedVariable;
        
        public override TaskStatus OnUpdate()
        {
            SharedVariable.SetValue(null);

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            SharedVariable = null;
        }
    }
}
