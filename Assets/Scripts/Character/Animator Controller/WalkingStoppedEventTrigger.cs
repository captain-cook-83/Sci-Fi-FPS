using BehaviorDesigner.Runtime;
using Cc83.Behaviors;
using UnityEngine;

namespace Cc83.Character.Behaviour
{
    public class WalkingStoppedEventTrigger : StateMachineBehaviour
    {
        private BehaviorTree _behaviorTree;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_behaviorTree == null)
            {
                _behaviorTree = animator.GetComponent<BehaviorTree>();
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _behaviorTree.SendEvent(BehaviorDefinitions.EventWalkingStopped);
        }
    }
}