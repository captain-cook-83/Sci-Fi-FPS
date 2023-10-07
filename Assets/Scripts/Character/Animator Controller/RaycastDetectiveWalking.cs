using UnityEngine;

namespace Cc83.Character.Behaviour
{
    public class RaycastDetectiveWalking : StateMachineBehaviour
    {
        [Range(0.5f, 5)]
        public float distance = 1;

        [Range(0.1f, 1)]
        public float interval = 0.1f;

        private float _tickTime;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _tickTime = Time.time + 1;      // 1s 之后开始检测
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var currentTime = Time.time;
            if (currentTime > _tickTime)
            {
                _tickTime = currentTime + interval;

                var transform = animator.transform;
                if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out var hitInfo, distance,
                        Definitions.MovingObstacleLayerMask))
                {
                    _tickTime = float.MaxValue;
                    
                    animator.SetTrigger(AnimatorConstants.AnimatorStop);
                    animator.SetBool(AnimatorConstants.AnimatorMoving, false);
                    Debug.LogError("Stop");
                }
            }
        }
    }
}