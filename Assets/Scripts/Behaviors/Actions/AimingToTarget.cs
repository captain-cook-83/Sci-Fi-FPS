using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class AimingToTarget : Action
    {
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetTurn;

        private Quaternion _targetRotation;

        private float _prevRotationAngle;

        public override void OnStart()
        {
            var position = transform.position;
            var targetPosition = TargetTurn.Value;
            targetPosition.y = position.y;
            
            _targetRotation = Quaternion.LookRotation(targetPosition - position);
            _prevRotationAngle = Quaternion.Angle(transform.rotation, _targetRotation);
        }

        public override TaskStatus OnUpdate()
        {
            var rotation = Quaternion.Lerp(transform.rotation, _targetRotation, Time.deltaTime * 10f);
            var rotationAngle = Quaternion.Angle(rotation, _targetRotation);
            if (rotationAngle < _prevRotationAngle)
            {
                _prevRotationAngle = rotationAngle;
                transform.rotation = rotation;
                return TaskStatus.Running;
            }

            transform.rotation = _targetRotation;
            return TaskStatus.Success;
        }
    }
}