using UnityEngine;

namespace Cc83.Character.Behaviour
{
    public class IKSwitchController : StateMachineBehaviour
    {
        [SerializeField]
        private bool hidePrimaryIk;

        [SerializeField]
        private bool hideSecondaryIk;
        
        [SerializeField]
        private bool cancelAimingLookAt;
        
        private EnemyWeaponIKController _ikController;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_ikController == null)
            {
                _ikController = animator.GetComponent<EnemyWeaponIKController>();
            }

            if (hidePrimaryIk)
            {
                _ikController.primaryIk = false;
            }
            
            if (hideSecondaryIk)
            {
                _ikController.secondaryIk = false;
            }

            if (cancelAimingLookAt)
            {
                _ikController.aimingActive = false;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (hidePrimaryIk)
            {
                _ikController.primaryIk = true;
            }
            
            if (hideSecondaryIk)
            {
                _ikController.secondaryIk = true;
            }

            if (cancelAimingLookAt)
            {
                _ikController.aimingActive = true;
            }
        }
    }
}