using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class StandIdle : Action
    {
        private Animator _animator;
        
        private AnimatorStateController _animatorStateController;
        
        public override void OnAwake()
        {
            _animator = GetComponent<Animator>();
            _animatorStateController = GetComponent<AnimatorStateController>();
        }
        
        public override void OnStart()
        {
            _animatorStateController.ChangeTensity(Random.Range(AnimatorConstants.MinimumTensity, 0));  // Range 用来控制 Idle 动画的不同表现，实际要求小于 0 即可
            _animator.SetBool(AnimatorConstants.AnimatorMoving, false);
        }
        
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}
