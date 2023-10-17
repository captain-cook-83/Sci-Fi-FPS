using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Utils;
using Pathfinding;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class FindDodgePosition : Action
    {
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 DodgeDirection;

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
            var direction = DodgeDirection.Value;
            var dodgeDirection = Random.value < 0.5f ? VectorUtils.GetLeftDirection(-direction) : VectorUtils.GetRightDirection(-direction);

            var position = transform.position;
            var distance = Random.Range(0.6f, 1.2f);
            var pathOrigin = new Vector3(position.x, 0.5f, position.z);         // 太高举例，避免贴地面检测
            if (Physics.Raycast(pathOrigin, dodgeDirection, distance + 1, Definitions.MovingObstacleLayerMask))
            {
                dodgeDirection *= -1;
                if (Physics.Raycast(pathOrigin, dodgeDirection, distance + 1, Definitions.MovingObstacleLayerMask))
                {
                    _status = TaskStatus.Failure;
                }
            }
            
            _seeker.StartPath(position, position + dodgeDirection * distance, OnPathCalculated);
            _status = TaskStatus.Running;
        }
        
        public override TaskStatus OnUpdate()
        {
            return _status;
        }

        public override void OnEnd()
        {
            DodgeDirection.SetValue(BehaviorDefinitions.InvalidSharedVector3);
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
            
            _status = TaskStatus.Success;
        }
    }
}