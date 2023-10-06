using System.Collections.Generic;
using UnityEngine;

namespace Cc83.Character.Behaviour
{
    public class HideOtherLayerBehaviour : StateMachineBehaviour
    {
        [SerializeField]
        private bool hidePrimaryIk;

        [SerializeField]
        private bool hideSecondaryIk;
        
        private readonly Dictionary<int, float> _layerWeights = new();

        private EnemyWeaponIKController _ikController;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var layerCount = animator.layerCount;
            for (var i = 0; i < layerCount; i++)
            {
                if (i == layerIndex) continue;
                
                var layerWeight = animator.GetLayerWeight(i);
                if (layerWeight == 0) continue;
                
                _layerWeights.Add(i, layerWeight);
                animator.SetLayerWeight(i, 0);
            }

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
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (var kv in _layerWeights)
            {
                if (animator.GetLayerWeight(kv.Key) == 0)      // 防止 OnStateEnter 与 OnStateExit 之间某些层级权重被修改
                {
                    animator.SetLayerWeight(kv.Key, kv.Value);
                }
            }

            _layerWeights.Clear();

            if (hidePrimaryIk)
            {
                _ikController.primaryIk = true;
            }
            
            if (hideSecondaryIk)
            {
                _ikController.secondaryIk = true;
            }
        }
    }
}
