using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class ResetSensorAgent : Action
    {
        // ReSharper disable once UnassignedField.Global
        public bool RetainPrevEnemy;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTargetList Teammates;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTargetList Enemies;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTarget Enemy;

        // ReSharper disable once UnassignedField.Global
        public SharedTransform PrevEnemy;
        
        private SensorAgent _sensorAgent;
        
        public override void OnAwake()
        {
            _sensorAgent = GetComponent<SensorAgent>();
        }
        
        public override void OnStart()
        {
            _sensorAgent.Reset();

            if (Enemy is { Value: not null })
            {
                PrevEnemy.SetValue(RetainPrevEnemy ? Enemy.Value.targetAgent.transform : null);
            }
            else
            {
                PrevEnemy.SetValue(null);
            }
            
            Teammates.SetValue(null);
            Enemies.SetValue(null);
            Enemy.SetValue(null);
        }

        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}