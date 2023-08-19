using System.Collections.Generic;
using UnityEngine;

namespace Cc83.Character.Behaviour
{
    public class TossGrenadeBehaviour : StateMachineBehaviour
    {
        private readonly Dictionary<int, float> layerWeights = new();

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var layerCount = animator.layerCount;
            for (var i = 0; i < layerCount; i++)
            {
                if (i == layerIndex) continue;
                
                var layerWeight = animator.GetLayerWeight(i);
                if (layerWeight == 0) continue;
                
                layerWeights.Add(i, layerWeight);
                animator.SetLayerWeight(i, 0);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (var kv in layerWeights)
            {
                if (animator.GetLayerWeight(kv.Key) == 0)      // 防止 OnStateEnter 与 OnStateExit 之间某些层级权重被修改
                {
                    animator.SetLayerWeight(kv.Key, kv.Value);
                }
            }

            layerWeights.Clear();
        }
    }
}
