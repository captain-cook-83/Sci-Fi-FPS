using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cc83.Behaviors
{
    public class TeammateSensitiveSensorAgent : SensorAgent
    {
        [Tooltip("keep teammate sensitive even if there isn't any enemy")]
        public bool forceTeammateSensitive;
        
#if UNITY_EDITOR
        private List<SensorTarget> _gizmosTeammates;

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (_gizmosTeammates == null) return;
            
            var origin = LookOrigin;
            foreach (var teammate in Teammates)
            {
                var endOffset = teammate.Direction.normalized * gizmosLength;
                var originPoint = origin + endOffset;
                Debug.DrawLine(originPoint, originPoint + endOffset, Color.blue);
            }
            
            foreach (var gizmosTeammate in _gizmosTeammates)
            {
                Debug.DrawLine(origin, origin + gizmosTeammate.Direction.normalized * gizmosLength, Color.white);
            }
        }
#endif
        
        protected readonly List<SensorTarget> Teammates = new (4);

        protected override bool OnSubmit(Vector3 lookOrigin, SensorTarget sensorTarget)
        {
#if UNITY_EDITOR
            _gizmosTeammates = null;
#endif
            
            if (Teammates.Count == 0) return true;
            
            if (Teammates.Count > 1)
            {
                Teammates.ForEach(CalculateSortScore);
                Teammates.Sort(DefaultSortFunc);
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
            
            if (selectedTeammates.Count == 0)
            {
                return true;
            }
            
#if UNITY_EDITOR
            _gizmosTeammates = selectedTeammates;
#endif
            
            Debug.Log($"{BehaviorDefinitions.EnemyAndTeammateAppear}: {sensorTarget?.TargetAgent.name ?? "^"} {selectedTeammates.Count}");
            BehaviorTree.SendEvent(BehaviorDefinitions.EnemyAndTeammateAppear, sensorTarget, selectedTeammates);
            return false;
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
