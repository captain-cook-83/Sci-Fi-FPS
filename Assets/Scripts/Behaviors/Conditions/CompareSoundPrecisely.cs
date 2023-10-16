using BehaviorDesigner.Runtime.Tasks;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class CompareSoundPrecisely : Conditional
    {
        // ReSharper disable once UnassignedField.Global
        public SharedSoundData Sound;

        // ReSharper disable once UnassignedField.Global
        public bool Value;

        public override TaskStatus OnUpdate()
        {
            return Sound.Value is { precisely: true } == Value ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}