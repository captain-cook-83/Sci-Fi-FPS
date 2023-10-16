using System;
using BehaviorDesigner.Runtime;

namespace Cc83.Behaviors
{
    [Serializable]
    public class SharedSoundData : SharedVariable<SoundData>
    {
        public static implicit operator SharedSoundData(SoundData value) { return new SharedSoundData { Value = value }; }
    }
}