using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class FindAttackPosition : Action
    {
        // ReSharper disable once UnassignedField.Global
        public SharedFloat AttackFarDistance;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTargetList Enemies;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetPosition;
        
        private AnimatorStateController _animatorStateController;

        private TaskStatus _status;

        public override void OnAwake()
        {
            _animatorStateController = GetComponent<AnimatorStateController>();
        }

        public override void OnStart()
        {
            var sensorTargets = Enemies.Value;
            if (sensorTargets == null || sensorTargets.Count == 0)
            {
                _status = TaskStatus.Failure;
                return;
            }
            
            var sensorTarget = sensorTargets[0];
            var targetTransform = sensorTarget.TargetAgent.transform;
            var attackDistance = Mathf.Min(AttackFarDistance.Value, Mathf.Sqrt(sensorTarget.SqrDistance));
            TargetPosition.SetValue(targetTransform.position - sensorTarget.Direction.normalized * attackDistance);     // TODO 检测是否可到达（如果不可达，计算可用目标点）

            if (_animatorStateController.Tensity < AnimatorConstants.WalkTensity)           // 此时确保进入双手持枪状态，避免单手持枪
            {
                _animatorStateController.ChangeTensity(AnimatorConstants.WalkTensity);
            }
            
            _status = TaskStatus.Success;
        }

        public override TaskStatus OnUpdate()
        {
            return _status;
        }
    }
}
