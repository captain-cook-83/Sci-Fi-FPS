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
                var angle = VectorUtils.DotDirectionalAngle2D(forward, target.targetAgent.transform.position - position);
                if (angle is > -Attack.LeftRetargetAngle and < Attack.RightRetargetAngle)
                {
                    _status = TaskStatus.Failure;
                }
                else
                {
                    var searchAngle = Random.Range(90, Angle);
                    var rotation = Quaternion.AngleAxis(angle < 0 ? -searchAngle : searchAngle, Vector3.up);
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