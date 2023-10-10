using UnityEngine;

namespace Cc83.Character.Behaviour
{
    public class ClearTurnParameter : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetFloat(AnimatorConstants.AnimatorTurn, 360);         // 因为角度有正负，所以使用 360 代表不旋转
        }
    }
}