using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Utils;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class FindBackTarget : Action
    {
        [Range(90, 180)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float Angle = 120;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTarget Enemy;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetTurn;

        private TaskStatus _status;
        
        public override void OnStart()
        {
            var target = Enemy.Value;
            if (target != null && target.targetAgent)
            {
                var position = transform.position;
                var forward = transform.forward;
                var targetDirection = (target.targetAgent.transform.position - position).normalized;
                if (Vector3.Dot(forward, targetDirection) > 0.2f)           // TODO 比较随意的先写 0.2
                {
                    _status = TaskStatus.Failure;
                }
                else
                {
                    var dotDirection = VectorUtils.DotDirection2D(forward, targetDirection);
                    var rotation = Quaternion.AngleAxis(dotDirection < 0 ? -Angle : Angle, Vector3.up);
                    TargetTurn.SetValue(position + rotation * forward);
                    _status = TaskStatus.Success;
                }
            }
            else
            {
                _status = TaskStatus.Failure;
            }
        }
        
        public override TaskStatus OnUpdate()
        {
            return _status;
        }
    }
}