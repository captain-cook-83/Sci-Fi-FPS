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
        public SharedSensorTarget Enemy;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetPosition;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetTurn;
        
        private AnimatorStateController _animatorStateController;

        private TaskStatus _status;

        public override void OnAwake()
        {
            _animatorStateController = GetComponent<AnimatorStateController>();
        }

        public override void OnStart()
        {
            // 为了调试过程中正确反映实际数据，首先做一次清理，以防止下面逻辑短路返回
            Enemy.SetValue(null);
            TargetPosition.SetValue(BehaviorDefinitions.InvalidSharedVector3);
            TargetTurn.SetValue(BehaviorDefinitions.InvalidSharedVector3);
            
            var sensorTargets = Enemies.Value;
            if (sensorTargets == null || sensorTargets.Count == 0)
            {
                _status = TaskStatus.Failure;
                return;
            }
            
            var sensorTarget = sensorTargets[0];                    // TODO 增加更多目标选取可能性
            var targetTransform = sensorTarget.targetAgent.transform;
            var targetPosition = Mathf.Sqrt(sensorTarget.sqrDistance) > AttackFarDistance.Value
                ? targetTransform.position - sensorTarget.direction.normalized * AttackFarDistance.Value        // TODO 检测是否可到达（如果不可达，计算可用目标点）
                : transform.position;

            Enemy.SetValue(sensorTarget);
            TargetPosition.SetValue(targetPosition);
            TargetTurn.SetValue(targetTransform.position);
            
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
