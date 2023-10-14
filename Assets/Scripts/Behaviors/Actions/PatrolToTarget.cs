using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Utils;
using UnityEngine;

namespace Cc83.Behaviors
{
    public class PatrolToTarget : MoveToTarget
    {
        [Range(15, 45)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float RepathAngle = 30;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTarget Enemy;

        private SensorAgent.SensorTarget _sensorTarget;

        private Vector3 _lastPathPoint;
        
        private Vector2 _lastDirection;
        
        private Vector3 _prevEnemyPosition;
        
        public override void OnStart()
        {
            base.OnStart();
            
            AnimatorStateController.ChangeTensity(Random.Range(AnimatorConstants.AimingTensity, AnimatorConstants.MaximumTensity));            // Tensity 直到移动之前才设置，避免之前的先举枪再转身的不自然表现
        }

        protected override void StartMonitor(List<Vector3> pathPoints)
        {
            _sensorTarget = Enemy.Value;

            _lastPathPoint = pathPoints[^1];
            _lastDirection = VectorUtils.Direction2D(pathPoints[^2], _lastPathPoint);
            _prevEnemyPosition = _sensorTarget.targetAgent.transform.position;
        }
        
        public override TaskStatus OnUpdate()
        {
            if (_sensorTarget == null) return base.OnUpdate();
            
            var targetPosition = _sensorTarget.targetAgent.transform.position;
            if (!targetPosition.Equals(_prevEnemyPosition))
            {
                var direction = VectorUtils.Direction2D(_lastPathPoint, targetPosition);
                var angle = Vector2.Angle(direction, _lastDirection);
                if (angle > RepathAngle)          // 此处的角度限制与 TurningToTarget.MinAngle 并没有什么关联
                {
                    return TaskStatus.Failure;
                }
            }

            return base.OnUpdate();
        }

        public override void OnEnd()
        {
            base.OnEnd();
            
            _sensorTarget = null;
        }
    }
}