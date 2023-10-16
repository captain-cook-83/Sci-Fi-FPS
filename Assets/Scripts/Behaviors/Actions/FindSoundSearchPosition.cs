using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class FindSoundSearchPosition : Action
    {
        [Range(3, 10)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float OffsetDistance = 6;

        public float MaxDistance = 20;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSoundData Sound;

        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetPosition;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetTurn;

        public override void OnStart()
        {
            var position = transform.position;
            var targetPosition = Sound.Value.position;
            var randomPosition = Random.insideUnitCircle * OffsetDistance;
            targetPosition.x += randomPosition.x;
            targetPosition.z += randomPosition.y;
            targetPosition = position + (targetPosition - position).normalized * MaxDistance;
            
            TargetPosition.SetValue(targetPosition);
            TargetTurn.SetValue(targetPosition);
        }
        
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}