using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cc83.Behaviors
{
    public class TeammateSensitiveSensorAgent : SensorAgent
    {
        [Tooltip("keep teammate sensitive even if there isn't any enemy")]
        public bool forceTeammateSensitive;
        
        private List<SensorTarget> _gizmosTeammates;
        
#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (_gizmosTeammates == null) return;
            
            var origin = LookOrigin;
            foreach (var gizmosTeammate in _gizmosTeammates)
            {
                Debug.DrawLine(origin, origin + gizmosTeammate.Direction.normalized * gizmosLength, Color.white);
            }
        }
#endif
        
        protected readonly List<SensorTarget> Teammates = new (4);

        protected override bool OnSubmit(Vector3 lookOrigin, List<SensorTarget> sensorTargets, bool haveAnyChanges)
        {
            switch (Teammates.Count)
            {
                case 0:
                    _gizmosTeammates = null;
                    return true;
                case > 1:
                    Teammates.ForEach(CalculateSortScore);
                    Teammates.Sort(DefaultSortFunc);
                    break;
            }

            var selectedTeammates = new List<SensorTarget>(3);
            foreach (var teammate in Teammates.Where(t => CanSeeTarget(lookOrigin, t)))
            {
                selectedTeammates.Add(teammate);
                if (selectedTeammates.Count == selectedTeammates.Capacity)
                {
                    break;
                }
            }

            var teammatesNumChanged = _gizmosTeammates == null
                ? selectedTeammates.Count > 0
                : _gizmosTeammates.Count != selectedTeammates.Count;
            _gizmosTeammates = selectedTeammates;
            
            if (haveAnyChanges && sensorTargets.Count > 0 && selectedTeammates.Count > 0)
            {
                BehaviorTree.SendEvent<object, object>(BehaviorDefinitions.EventEnemyAppear, sensorTargets, selectedTeammates);
                return false;
            } 
            
            if (teammatesNumChanged)
            {
                BehaviorTree.SendEvent(BehaviorDefinitions.EventTeammateChange, selectedTeammates);
            }
                
            return true;
        }

        protected override void OnClear()
        {
            Teammates.Clear();
        }
        
        internal void NotifyTeammate(SensorAgent teammate, Vector3 direction, float sDistance, float angle)
        {
            Teammates.Add(new SensorTarget
            {
                TargetAgent = teammate,
                Direction = direction,
                SqrDistance = sDistance,
                Angle = angle
            });
        }

        internal void RemoveTeammates(Dictionary<SensorAgent, bool> teammates)
        {
            for (var i = Teammates.Count - 1; i >= 0; --i)
            {
                if (teammates.TryGetValue(Teammates[i].TargetAgent, out var operation) && operation == SensorSystem.OperationRemove)
                {
                    Teammates.RemoveAt(i);
                }
            }
        }
    }
}
