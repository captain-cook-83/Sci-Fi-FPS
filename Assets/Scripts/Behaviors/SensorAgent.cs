using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;

namespace Cc83.Behaviors
{
    public class SensorAgent : MonoBehaviour
    {
        [Range(45, 62)]
        public float halfFov = 55;

        [Range(100, 10000)]
        public float sqrDistance = 900;

        private readonly Dictionary<SensorAgent, (Vector3, float, float)> _enemies = new (1);

        private readonly Dictionary<SensorAgent, (Vector3, float, float)> _teammates = new (4);

        private BehaviorTree _behaviorTree;

        private void Awake()
        {
            _behaviorTree = GetComponent<BehaviorTree>();
        }

        internal void NotifyEnemy(SensorAgent enemy, Vector3 direction, float sqrDistance, float angle)
        {
            _enemies.Add(enemy, (direction, sqrDistance, angle));
        }
        
        internal void NotifyTeammate(SensorAgent teammate, Vector3 direction, float sqrDistance, float angle)
        {
            _teammates.Add(teammate, (direction, sqrDistance, angle));
        }

        internal void RemoveTeammates(Dictionary<SensorAgent, bool> teammates)
        {
            foreach (var kv in teammates)
            {
                if (!kv.Value)
                {
                    _teammates.Remove(kv.Key);
                }
            }
        }
        
        internal void Submit()
        {
            // _behaviorTree.SendEvent();
        }
        
        internal void Clear()
        {
            _enemies.Clear();
            _teammates.Clear();
        }

        internal void Destroy()
        {
            Clear();
        }
    }   
}
