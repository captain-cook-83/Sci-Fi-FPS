using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;
using Pathfinding;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public abstract class MoveToTarget : Action
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public float StopProjection = 0.1f;
        
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnassignedField.Global
        public float LastStopDistance;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetPosition;
        
        protected virtual int Tensity => 0;
        
        protected Animator Animator;

        protected AnimatorStateController AnimatorStateController;
        
        protected TaskStatus Status;
        
        private Seeker _seeker;

        private bool _interrupted;
        
        public override void OnAwake()
        {
            Animator = GetComponent<Animator>();
            AnimatorStateController = GetComponent<AnimatorStateController>();
            _seeker = GetComponent<Seeker>();
        }

        public override void OnStart()
        {
            _interrupted = false;

            var position = transform.position;
            var targetPosition = TargetPosition.Value;
            
            if (Vector3.Distance(position, targetPosition) < StopProjection)
            {
                Status = TaskStatus.Success;
            }
            else
            {
                Status = TaskStatus.Running;
                _seeker.StartPath(position, targetPosition, OnPathCalculated);
            }
        }

        public override TaskStatus OnUpdate()
        {
            return Status;
        }

        public override void OnEnd()
        {
            _interrupted = true;

            AnimatorStateController.ChangeSpeed(0, 0.1f, () => Animator.SetBool(AnimatorConstants.AnimatorMoving, false));
        }

        private void OnPathCalculated(Path path)
        {
            if (path.error || path.vectorPath.Count == 0)
            {
                Debug.LogError($"Pathfinding error: {path.errorLog}.");
                Status = TaskStatus.Failure;
                return;
            }

            if (_interrupted) return;
            
            AnimatorStateController.ChangeTensity(Tensity);
            StartCoroutine(MovingToTarget(path.vectorPath));
        }

        private IEnumerator MovingToTarget(IReadOnlyList<Vector3> pathPoints)
        {
            var movingSpeed = 1+Random.Range(1.5f, 4.5f);
            
            Animator.SetBool(AnimatorConstants.AnimatorMoving, true);
            AnimatorStateController.ChangeSpeed(movingSpeed, 0.1f);
            
            for (var i = 1; i < pathPoints.Count; i++)
            {
                var targetPoint = pathPoints[i];
                var position = transform.position;
                targetPoint.y = position.y;
                
                var direction = (targetPoint - position).normalized;
                var targetRotation = Quaternion.LookRotation(direction);

                if (LastStopDistance > 0 && i + 1 == pathPoints.Count)
                {
                    targetPoint -= direction * LastStopDistance;
                }
                
                var rotation = transform.rotation;
                var rotationAngle = Quaternion.Angle(rotation, targetRotation);
                var changeSpeed = rotationAngle > 45;
                if (changeSpeed && NotInterrupted())
                {
                    AnimatorStateController.ChangeSpeed(0, 0.1f);
                }
                
                var prevRotationAngle = 0f;
                do
                {
                    if (_interrupted) yield break; else yield return null;
                    
                    rotation = Quaternion.Lerp(rotation, targetRotation, Time.deltaTime * 10f);
                    prevRotationAngle = rotationAngle;
                    rotationAngle = Quaternion.Angle(rotation, targetRotation);
                    transform.rotation = rotation;
                } while (prevRotationAngle > rotationAngle);

                transform.rotation = targetRotation;

                if (changeSpeed && NotInterrupted())
                {
                    if (NotInterrupted())
                    {
                        AnimatorStateController.ChangeSpeed(movingSpeed, 0.1f);
                    }
                }
                
                var targetProjection = 0f;
                do
                {
                    if (_interrupted) yield break; else yield return null;
                    
                    targetProjection = Vector3.Dot((targetPoint - transform.position).normalized, direction);
                } while (targetProjection > StopProjection);
            }
            
            Status = TaskStatus.Success;
        }

        private bool NotInterrupted()
        {
            return !_interrupted;
        }
    }
}