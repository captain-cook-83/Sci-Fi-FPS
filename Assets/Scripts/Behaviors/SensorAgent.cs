using System;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime;
using UnityEngine;

namespace Cc83.Behaviors
{
    public class SensorAgent : MonoBehaviour
    {
        private const float ReciprocalOfMaxAngle = 90;
        
        [Serializable]
        public class SensorTarget
        {
            public SensorAgent TargetAgent;

            public Vector3 Direction;

            public float SqrDistance;

            public float Angle;

            public float SortScore;
        }

        public enum SensorAgentType
        {
            Player, Enemy
        }

        public SensorAgentType type;
        
        [Range(45, 90)]
        public float halfFov = 60;

        [Range(100, 10000)]
        public float sqrDistance = 900;

        [Range(1, 2)]
        public float viewHeight = 1.6f;
        
#if UNITY_EDITOR
        [Range(1, 10)]
        public int gizmosLength = 5;
#endif
        
        protected BehaviorTree BehaviorTree;
        
        protected internal Vector3 LookForward => transform.forward;
        
        protected Vector3 LookOrigin => transform.position + Vector3.up * viewHeight;

        private readonly List<SensorTarget> _enemies = new (1);

        private readonly Dictionary<SensorAgent, SensorTarget> _prevEnemies = new (1);

        private void Awake()
        {
            BehaviorTree = GetComponent<BehaviorTree>();
        }

        private void OnEnable()
        {
            switch (type)
            {
                case SensorAgentType.Player:
                    SensorSystem.Instance.RegistryPlayer(this);
                    break;
                case SensorAgentType.Enemy:
                    SensorSystem.Instance.RegistryEnemy(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDisable()
        {
            if (SensorSystem.Instance == null) return;
            
            switch (type)
            {
                case SensorAgentType.Player:
                    SensorSystem.Instance.UnRegistryPlayer();
                    break;
                case SensorAgentType.Enemy:
                    SensorSystem.Instance.UnRegistryEnemy(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            var origin = LookOrigin;
            foreach (var kv in _prevEnemies)
            {
                Debug.DrawLine(origin, origin + kv.Value.Direction.normalized * gizmosLength, Color.red);
            }
        }
#endif
        
        protected virtual bool OnSubmit(Vector3 lookOrigin, List<SensorTarget> sensorTargets, bool haveAnyChanges) { return true; }
        
        protected virtual void OnClear() { }
        
        protected virtual void OnStop() { }

        internal void NotifyEnemy(SensorAgent enemy, Vector3 direction, float sDistance, float angle)
        {
            _enemies.Add(new SensorTarget
            {
                TargetAgent = enemy,
                Direction = direction,
                SqrDistance = sDistance,
                Angle = angle
            });
        }
        
        internal void Submit()
        {
            if (_enemies.Count > 1)
            {
                _enemies.ForEach(CalculateSortScore);
                _enemies.Sort(DefaultSortFunc);
            }

            var lookOrigin = LookOrigin;
            var sensorTargets = _enemies.Where(e => CanSeeTarget(lookOrigin, e)).ToList();
            var haveAnyChanges = CompareChanges(sensorTargets, _prevEnemies);

            if (OnSubmit(lookOrigin, sensorTargets, haveAnyChanges) && haveAnyChanges)
            {
                if (sensorTargets.Count > 0)
                {
                    BehaviorTree.SendEvent<object, object>(BehaviorDefinitions.EventEnemyAppear, sensorTargets, null);
                    Debug.LogWarning($"{transform.name} - {BehaviorDefinitions.EventEnemyAppear}");
                }
                else
                {
                    BehaviorTree.SendEvent(BehaviorDefinitions.EventEnemyDisappear);
                    Debug.LogWarning($"{transform.name} - {BehaviorDefinitions.EventEnemyDisappear}");
                }
            }

            _prevEnemies.Clear();
            foreach (var sensorTarget in sensorTargets)
            {
                _prevEnemies.Add(sensorTarget.TargetAgent, sensorTarget);
            }
        }
        
        internal void Clear()
        {
            OnClear();
            
            _enemies.Clear();
        }

        internal void Stop()
        {
            Clear();
            OnStop();
        }

        protected static int DefaultSortFunc(SensorTarget a, SensorTarget b)
        {
            return a.SortScore > b.SortScore ? 1 : (a.SortScore < b.SortScore ? -1 : 0);
        }

        protected static void CalculateSortScore(SensorTarget sensorTarget)
        {
            sensorTarget.SortScore = (1 + sensorTarget.Angle * ReciprocalOfMaxAngle) * sensorTarget.SqrDistance;
        }

        protected static bool CanSeeTarget(Vector3 lookOrigin, SensorTarget target)
        {
            return !Physics.Raycast(lookOrigin, target.Direction, Mathf.Sqrt(target.SqrDistance), Definitions.ViewObstacleLayerMask);
        }

        private static bool CompareChanges(IReadOnlyCollection<SensorTarget> targets, IReadOnlyDictionary<SensorAgent, SensorTarget> previous)
        {
            return targets.Count != previous.Count || targets.Any(t => !previous.ContainsKey(t.TargetAgent));
        }
    }   
}
