using UnityEngine;

namespace Cc83.Character.Behaviour
{
    public class StopBehaviourAnimator : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.enabled = false;
        }
    }
}