using BehaviorDesigner.Runtime.Tasks;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class ResetSensorAgent : Action
    {
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
            Teammates.SetValue(null);
            Enemies.SetValue(null);
            Enemy.SetValue(null);
            
            _sensorAgent.Reset();
        }

        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}