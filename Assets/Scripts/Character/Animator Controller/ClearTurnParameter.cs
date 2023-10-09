using UnityEngine;

namespace Cc83.Character.Behaviour
{
    public class ClearTurnParameter : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetFloat(AnimatorConstants.AnimatorTurn, 360);
        }
    }
}