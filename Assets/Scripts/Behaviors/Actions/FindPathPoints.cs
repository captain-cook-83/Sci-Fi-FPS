using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Pathfinding;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class FindPathPoints : Action
    {
        [Range(0.1f, 0.5f)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float MinPathDistance = 0.1f;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetPosition;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetTurn;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3List PathPoints;
        
        private Seeker _seeker;
        
        private TaskStatus _status;
        
        public override void OnAwake()
        {
            _seeker = GetComponent<Seeker>();
        }

        public override void OnStart()
        {
            PathPoints.SetValue(null);
            
            var currentPosition = transform.position;
            var targetPosition = TargetPosition.Value;
            
            if (Vector3.Distance(currentPosition, targetPosition) > MinPathDistance)
            {
                _status = TaskStatus.Running;
                _seeker.StartPath(currentPosition, targetPosition, OnPathCalculated);
            }
            else
            {
                _status = TaskStatus.Success;
            }
        }
        
        public override TaskStatus OnUpdate()
        {
            return _status;
        }
        
        private void OnPathCalculated(Path path)
        {
            var vectorPath = path.vectorPath;
            
            if (path.error || vectorPath.Count < 2)
            {
                Debug.LogError($"Pathfinding error: {path.errorLog}.");
                _status = TaskStatus.Failure;
                return;
            }
            
            PathPoints.SetValue(vectorPath);
            TargetTurn.SetValue(vectorPath[1]);
            
            _status = TaskStatus.Success;
        }
    }
}