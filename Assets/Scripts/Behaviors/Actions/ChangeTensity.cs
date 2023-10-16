using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;
using Cc83.Utils;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class ChangeTensity : Action
    {
        // ReSharper disable once UnassignedField.Global
        public Tensity Tensity;

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float Speed = 1.0f;
        
        private AnimatorStateController _animatorStateController;

        public override void OnAwake()
        {
            _animatorStateController = GetComponent<AnimatorStateController>();
        }

        public override void OnStart()
        {
            _animatorStateController.ChangeTensity(AnimatorUtils.ConvertTensity(Tensity), Speed);
        }

        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}