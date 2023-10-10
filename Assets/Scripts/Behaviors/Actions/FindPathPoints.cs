using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Pathfinding;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class FindPathPoints : Action
    {
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetPosition;
        
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
            _status = TaskStatus.Running;
            _seeker.StartPath(transform.position, TargetPosition.Value, OnPathCalculated);
        }
        
        public override TaskStatus OnUpdate()
        {
            return _status;
        }
        
        private void OnPathCalculated(Path path)
        {
            if (path.error || path.vectorPath.Count < 2)
            {
                Debug.LogError($"Pathfinding error: {path.errorLog}.");
                _status = TaskStatus.Failure;
                return;
            }
            
            PathPoints.SetValue(path.vectorPath);
            _status = TaskStatus.Success;
        }
    }
}