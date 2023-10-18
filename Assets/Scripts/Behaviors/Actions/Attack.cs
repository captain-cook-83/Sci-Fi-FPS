using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;
using Cc83.Utils;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class Attack : Action
    {
        // ReSharper disable once UnassignedField.Global
        public SharedFloat AttackFarDistance;
        
        // ReSharper disable once UnassignedField.Global
        public SharedFloat EscapeDistance;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTarget Enemy;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetTurn;

        // ReSharper disable once UnassignedField.Global
        public SharedBool EscapeFighting;
        
        private EnemyAttackController _attackController;

        private SensorAgent.SensorTarget _sensorTarget;

        private float _attackFarSqrDistance;

        private float _escapeSqrDistance;
        
        private float _middleSqrDistance;

        private bool _escapeMoving;         // 是否为逃离移动模式（false 为进攻移动模式），只有 EscapeFighting.Value 为 true 时，才有意义

        public override void OnAwake()
        {
            _attackController = GetComponent<EnemyAttackController>();
            
            _attackFarSqrDistance = Mathf.Pow(AttackFarDistance.Value, 2);
            _escapeSqrDistance = Mathf.Pow(EscapeDistance.Value, 2);
            _middleSqrDistance = Mathf.Pow((AttackFarDistance.Value + EscapeDistance.Value) * 0.5f, 2);
        }

        public override void OnStart()
        {
            _sensorTarget = Enemy.Value;
        }

        public override TaskStatus OnUpdate()
        {
            var position = transform.position;
            var targetPosition = _sensorTarget.targetAgent.transform.position;
            var direction = targetPosition - position;
            
            #region 移动
            
            //TODO 没有必要每帧计算
            var sqrDistance = Vector3.SqrMagnitude(direction);
            if (EscapeFighting.Value)
            {
                if (_escapeMoving ? sqrDistance > _middleSqrDistance : sqrDistance < _middleSqrDistance)
                {
                    EscapeFighting.SetValue(false);
                }
            }
            else
            {
                if (sqrDistance > _attackFarSqrDistance)
                {
                    _escapeMoving = false;
                    EscapeFighting.SetValue(true);
                }
                else if (sqrDistance < _escapeSqrDistance)
                {
                    _escapeMoving = true;;
                    EscapeFighting.SetValue(true);
                }
            }
            
            #endregion

            #region 射击与转身

            if (_attackController.Tick()) return TaskStatus.Running;
            
            var forward = transform.forward;
            var directionalAngle = VectorUtils.DotDirectionalAngle2D(forward, direction);
            var angle = Mathf.Min(TurningToTarget.MinAngle, EnemyAttackController.LeftRetargetAngle + EnemyAttackController.RightRetargetAngle - 5);            // 减小 5°，避免一侧转身后立即出发另一侧的临界角度
            targetPosition = position + Quaternion.AngleAxis(directionalAngle < 0 ? -angle : angle, Vector3.up) * forward;
            TargetTurn.SetValue(targetPosition);
            return TaskStatus.Success;

            #endregion
        }

        public override void OnEnd()
        {
            EscapeFighting.SetValue(false);
        }
    }
}
