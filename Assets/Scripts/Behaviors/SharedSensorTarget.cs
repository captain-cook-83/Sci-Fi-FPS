using System;
using BehaviorDesigner.Runtime;

namespace Cc83.Behaviors
{
    [Serializable]
    public class SharedSensorTarget : SharedVariable<SensorAgent.SensorTarget>
    {
        public static implicit operator SharedSensorTarget(SensorAgent.SensorTarget value) { return new SharedSensorTarget { Value = value }; }
    }
}