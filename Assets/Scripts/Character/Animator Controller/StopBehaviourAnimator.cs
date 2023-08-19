using UnityEngine;

namespace Cc83.Character.Behaviour
{
    [SharedBetweenAnimators]
    public class StopBehaviourAnimator : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.enabled = false;
        }
    }
}