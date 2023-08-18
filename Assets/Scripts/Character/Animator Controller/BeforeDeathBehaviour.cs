using UnityEngine;

namespace Cc83.Character.Behaviour
{
    public class BeforeDeathBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var layerCount = animator.layerCount;
            for (var i = 0; i < layerCount; i++)
            {
                if (i != layerIndex)
                {
                    animator.SetLayerWeight(i, 0);
                }
            }
        }
    }
}
