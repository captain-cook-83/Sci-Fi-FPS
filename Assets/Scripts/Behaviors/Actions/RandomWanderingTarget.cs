using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;
using Pathfinding;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class RandomWanderingTarget : Action
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public int MaxGScore = 10000;           // 10m 左右的范围

        // ReSharper disable once UnassignedField.Global
        public SharedInt Randomness;
        
        // ReSharper disable once UnassignedField.Global
        public SharedTransform CantonmentPoint;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetPosition;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetTurn;
        
        private AnimatorStateController _animatorStateController;

        private TaskStatus _status;
        
        public override void OnAwake()
        {
            _animatorStateController = GetComponent<AnimatorStateController>();
        }
        
        public override void OnStart()
        {
            // 为了调试过程中正确反映实际数据，首先做一次清理，以防止下面逻辑短路返回
            TargetPosition.SetValue(BehaviorDefinitions.InvalidSharedVector3);
            TargetTurn.SetValue(BehaviorDefinitions.InvalidSharedVector3);
            
            var cantonmentPoint = CantonmentPoint.Value;
            var startPoint = cantonmentPoint ? cantonmentPoint.position : transform.position;
            AstarPath.StartPath(ConstantPath.Construct(startPoint, MaxGScore, OnPathCalculated));
            
            _animatorStateController.ChangeTensity(AnimatorConstants.WalkTensity);
            _status = TaskStatus.Running;
        }

        public override TaskStatus OnUpdate()
        {
            return _status;
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
            
            var randomPoints = PathUtilities.GetPointsOnNodes(constantPath.allNodes, 1 + Mathf.Min(5, Randomness.Value));    // 获取多个随机点，增加随机性
            if (randomPoints.Count == 0)
            {
                _status = TaskStatus.Failure;
            }
            else
            {
                var randomPoint = randomPoints[Random.Range(0, randomPoints.Count)];
                TargetPosition.SetValue(randomPoint);
                TargetTurn.SetValue(randomPoint);
                
                _status = TaskStatus.Success;
            }
        }
    }
}