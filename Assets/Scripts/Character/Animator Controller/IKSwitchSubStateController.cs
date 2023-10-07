using UnityEngine;

namespace Cc83.Character.Behaviour
{
    public class IKSwitchSubStateController : StateMachineBehaviour
    {
        [SerializeField]
        private bool primaryIk;

        [SerializeField]
        private bool secondaryIk;
        
        [SerializeField]
        private bool aimingLookAt;

        [SerializeField] 
        private bool recoverOnExit = true;
        
        private EnemyWeaponIKController _ikController;

        private bool _originPrimaryIk;
        
        private bool _originSecondaryIk;
        
        private bool _originAimingLookAt;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_ikController == null)
            {
                _ikController = animator.GetComponent<EnemyWeaponIKController>();
            }

            _originPrimaryIk = _ikController.primaryIk;
            _originSecondaryIk = _ikController.secondaryIk;
            _originAimingLookAt = _ikController.aimingActive;
            
            _ikController.primaryIk = primaryIk;
            _ikController.secondaryIk = secondaryIk;
            _ikController.aimingActive = aimingLookAt;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!recoverOnExit) return;
            
            _ikController.primaryIk = _originPrimaryIk;
            _ikController.secondaryIk = _originSecondaryIk;
            _ikController.aimingActive = _originAimingLookAt;
        }
    }
}