using BehaviorDesigner.Runtime.Tasks;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class ResetSensorAgent : Action
    {
        // ReSharper disable once UnassignedField.Global
        public bool RetainEnemy;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTargetList Teammates;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTargetList Enemies;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTarget Enemy;
        
        private SensorAgent _sensorAgent;
        
        public override void OnAwake()
        {
            _sensorAgent = GetComponent<SensorAgent>();
        }
        
        public override void OnStart()
        {
            _sensorAgent.Reset();
            
            Teammates.SetValue(null);
            Enemies.SetValue(null);
            if (!RetainEnemy)
            {
                Enemy.SetValue(null);
            }
        }

        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}