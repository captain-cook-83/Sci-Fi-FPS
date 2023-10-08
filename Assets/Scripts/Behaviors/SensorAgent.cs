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
        private SensorTarget _gizmosEnemy;

        protected virtual void OnDrawGizmos()
        {
            var origin = LookOrigin;
            
            if (_gizmosEnemy != null)
            {
                Debug.DrawLine(origin, origin + _gizmosEnemy.Direction.normalized * gizmosLength, Color.red);
            }

            foreach (var enemy in _enemies)
            {
                var endOffset = enemy.Direction.normalized * gizmosLength;
                var originPoint = origin + endOffset;
                Debug.DrawLine(originPoint, originPoint + endOffset, Color.blue);
            }
        }
#endif
        
        protected virtual bool OnSubmit(Vector3 lookOrigin, SensorTarget sensorTarget) { return true; }
        
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
            var sensorTarget = _enemies.FirstOrDefault(e => CanSeeTarget(lookOrigin, e));
            
#if UNITY_EDITOR
            _gizmosEnemy = sensorTarget;
#endif
            
            if (OnSubmit(lookOrigin, sensorTarget) && sensorTarget != null)
            {
                BehaviorTree.SendEvent(BehaviorDefinitions.EventEnemyAppear, sensorTarget);
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
#if UNITY_EDITOR
            if (Physics.Raycast(lookOrigin, target.Direction, out var hit, Mathf.Sqrt(target.SqrDistance), Definitions.ViewObstacleLayerMask))
            {
                Debug.LogWarning($"{target.TargetAgent.name} ---->>> {hit.transform.name}");
                return false;
            }
            return true;
#else
            return !Physics.Raycast(lookOrigin, target.Direction, Mathf.Sqrt(target.SqrDistance), Definitions.ViewObstacleLayerMask);
#endif
        }
    }   
}
