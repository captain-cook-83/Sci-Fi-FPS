using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cc83.Behaviors
{
    public class TeammateSensitiveSensorAgent : SensorAgent
    {
        [Tooltip("keep teammate sensitive even if there isn't any enemy")]
        public bool forceTeammateSensitive;
        
        private readonly List<SensorTarget> _teammates = new (4);
        
        private readonly Dictionary<SensorAgent, SensorTarget> _prevTeammates = new (3);
        
#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            
            var origin = LookOrigin;
            foreach (var kv in _prevTeammates)
            {
                Debug.DrawLine(origin, origin + kv.Value.direction.normalized * gizmosLength, Color.white);
            }
        }
#endif
        
        protected override bool OnSubmit(Vector3 lookOrigin, List<SensorTarget> sensorTargets, bool haveAnyChanges)
        {
            switch (_teammates.Count)
            {
                case 0:
                    _prevTeammates.Clear();
                    return true;
                case > 1:
                    _teammates.ForEach(CalculateSortScore);
                    _teammates.Sort(DefaultSortFunc);
                    break;
            }

            var selectedTeammates = new List<SensorTarget>(3);
            foreach (var teammate in _teammates.Where(t => CanSeeTarget(lookOrigin, t)))
            {
                selectedTeammates.Add(teammate);
                if (selectedTeammates.Count == selectedTeammates.Capacity)
                {
                    break;
                }
            }

            var teammatesNumChanged = _prevTeammates.Count != selectedTeammates.Count;
            ConvertToDictionary(selectedTeammates, _prevTeammates);
            
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
            _teammates.Clear();
        }
        
        internal void NotifyTeammate(SensorAgent teammate, Vector3 direction, float sDistance, float angle)
        {
            if (_prevTeammates.TryGetValue(teammate, out var originTarget))
            {
                originTarget.direction = direction;
                originTarget.sqrDistance = sDistance;
                originTarget.angle = angle;
                
                _teammates.Add(originTarget);
            }
            else
            {
                _teammates.Add(new SensorTarget
                {
                    targetAgent = teammate,
                    direction = direction,
                    sqrDistance = sDistance,
                    angle = angle
                });
            }
        }

        internal void RemoveTeammates(Dictionary<SensorAgent, bool> teammates)
        {
            for (var i = _teammates.Count - 1; i >= 0; --i)
            {
                if (teammates.TryGetValue(_teammates[i].targetAgent, out var operation) && operation == SensorSystem.OperationRemove)
                {
                    _teammates.RemoveAt(i);
                }
            }
        }
    }
}
