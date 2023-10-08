using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;

namespace Cc83.Behaviors
{
    [Serializable]
    public class SharedSensorTargetList : SharedVariable<List<SensorAgent.SensorTarget>>
    {
        public static implicit operator SharedSensorTargetList(List<SensorAgent.SensorTarget> value) { return new SharedSensorTargetList { Value = value }; }
    }
}