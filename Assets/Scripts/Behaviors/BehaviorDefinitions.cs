using UnityEngine;

namespace Cc83.Behaviors
{
    public enum Tensity
    {
        LowestIdle, LowerIdle, Rifle, HigherRifle, HighestRifle
    }
    
    public static class BehaviorDefinitions
    {
        public const string EventEnemyAppear = "EnemyAppear";
        
        public const string EventEnemyDisappear = "EnemyDisappear";
        
        public const string EventTeammateChange = "TeammateChange";
        
        public const string EventSoundAlert = "SoundAlert";
        
        public const string EventTurningStopped = "TurningStopped";

        public static Vector3 InvalidSharedVector3 = Vector3.zero;
    }
}