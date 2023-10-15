using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Pathfinding;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class FindEscapePathPoints : Action
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public int MaxGScore = 10000;           // 10m 左右的范围
        
        // ReSharper disable once UnassignedField.Global
        public SharedFloat AttackFarDistance;
        
        // ReSharper disable once UnassignedField.Global
        public SharedFloat AttackNearDistance;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTarget Enemy;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3List PathPoints;
        
        private Seeker _seeker;
        
        private TaskStatus _status;

        private Vector3 _optimalDirection;

        private float _optimalDistance;

        private bool _finalSearch;
        
        public override void OnAwake()
        {
            _seeker = GetComponent<Seeker>();
        }
        
        public override void OnStart()
        {
            var position = transform.position;
            var enemyPosition = Enemy.Value.targetAgent.transform.position;
            
            _finalSearch = false;
            _optimalDirection = (position - enemyPosition).normalized;
            _optimalDistance = (AttackNearDistance.Value + AttackFarDistance.Value) * 0.5f;
            
            _status = TaskStatus.Running;
            _seeker.StartPath(position, enemyPosition + _optimalDirection * _optimalDistance, OnDirectPathCalculated);
        }
        
        public override TaskStatus OnUpdate()
        {
            return _status;
        }

        private void OnDirectPathCalculated(Path path)
        {
            if (path.error)
            {
                Debug.LogError($"Pathfinding error: {path.errorLog}.");
                _status = TaskStatus.Failure;
                return;
            }

            var enemyPosition = Enemy.Value.targetAgent.transform.position;
            var vectorPath = path.vectorPath;
            if (vectorPath.Count >= 2 && Mathf.Abs(Vector3.Distance(vectorPath[^1], enemyPosition) - AttackNearDistance.Value) > 1)         // 至少可以后退 1 米
            {
                PathPoints.SetValue(vectorPath);
                _status = TaskStatus.Success;
                return;
            }

            if (_finalSearch)
            {
                _status = TaskStatus.Failure;
            }
            else
            {
                _finalSearch = true;
                AstarPath.StartPath(ConstantPath.Construct(enemyPosition, (int)(MaxGScore * 0.1f * AttackFarDistance.Value), OnPathCalculated));        // 采用 AttackFarDistance 获得有效地址的概率高于 _optimalDistance
            }
        }
        
        private void OnPathCalculated(Path path)
        {
            if (path.error)
            {
                Debug.LogError($"Pathfinding error: {path.errorLog}.");
                _status = TaskStatus.Failure;
                return;
            }
            
            var constantPath = (ConstantPath) path;
            if (constantPath.allNodes.Count == 0)
            {
                Debug.LogError($"ConstantPath can not found any node.");
                _status = TaskStatus.Failure;
                return;
            }
            
            var randomPoints = PathUtilities.GetPointsOnNodes(constantPath.allNodes, 3);
            if (randomPoints.Count == 0)
            {
                _status = TaskStatus.Failure;
            }
            else
            {
                var maxDistance = 0f;
                var optimalPosition = Vector3.zero;
                var position = transform.position;
                foreach (var randomPoint in randomPoints)
                {
                    var direction = randomPoint - position;
                    var distance = direction.magnitude;
                    if (distance < AttackNearDistance.Value || Vector3.Dot(direction.normalized, _optimalDirection) < 0.5f) continue;
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        optimalPosition = randomPoint;
                    }
                }

                if (maxDistance == 0)
                {
                    _status = TaskStatus.Failure;
                    return;
                }
                
                _seeker.StartPath(position, optimalPosition, OnDirectPathCalculated);
            }
        }
    }
}