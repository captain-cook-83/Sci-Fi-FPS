using BehaviorDesigner.Runtime;
using UnityEngine;

namespace Cc83.Character.Behaviour
{
    public class BehaviorTreeEventTrigger : StateMachineBehaviour
    {
        public string eventName;
        
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
            _behaviorTree.SendEvent(eventName);
        }
    }
}