using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class StandIdle : Action
    {
        private Animator _animator;
        
        public override void OnAwake()
        {
            _animator = GetComponent<Animator>();
        }
        
        public override void OnStart()
        {
            _animator.SetBool(AnimatorConstants.AnimatorCrouching, false);
            _animator.SetFloat(AnimatorConstants.AnimatorTensity, -1);
        }
        
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}
