using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Utils;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class FindPrevEnemy : Action
    {
        // ReSharper disable once UnassignedField.Global
        public SharedTransform PrevEnemy;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetPosition;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetTurn;
        
        private SensorAgent _sensorAgent;
        
        private TaskStatus _status;
        
        public override void OnAwake()
        {
            _sensorAgent = GetComponent<SensorAgent>();
        }
        
        public override void OnStart()
        {
            var prevEnemy = PrevEnemy.Value;
            if (prevEnemy)
            {
                var targetPosition = prevEnemy.position;
                var angle = VectorUtils.Angle2D(transform.forward, targetPosition - transform.position);
                if (angle <= _sensorAgent.halfFov)
                {
                    TargetPosition.SetValue(targetPosition);
                    TargetTurn.SetValue(targetPosition);
                    _status = TaskStatus.Success;
                    return;
                }
            }
            
            _status = TaskStatus.Failure;
        }
        
        public override TaskStatus OnUpdate()
        {
            return _status;
        }
    }
}