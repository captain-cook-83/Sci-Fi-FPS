using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class FindAttackPosition : Action
    {
        [Range(5, 30)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once ConvertToConstant.Global
        public float AttackDistance = 5;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTargetList Enemies;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetPosition;

        public override TaskStatus OnUpdate()
        {
            var sensorTargets = Enemies.Value;
            if (sensorTargets == null || sensorTargets.Count == 0)
            {
                return TaskStatus.Failure;
            }
            
            var sensorTarget = sensorTargets[0];
            var targetTransform = sensorTarget.TargetAgent.transform;
            var attackDistance = Mathf.Min(AttackDistance, Mathf.Sqrt(sensorTarget.SqrDistance));
            TargetPosition.SetValue(targetTransform.position - sensorTarget.Direction.normalized * attackDistance);     // TODO 检测是否可到达（如果不可达，计算可用目标点）
            
            return TaskStatus.Success;
        }
    }
}
