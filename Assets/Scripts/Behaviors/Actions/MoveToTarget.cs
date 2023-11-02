using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;
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
        public SharedVector3List PathPoints;
        
        protected Animator Animator;

        protected AnimatorStateController AnimatorStateController;

        protected virtual TaskStatus FastEndingStatus => TaskStatus.Success;
        
        protected virtual float Speed => 1 + Random.Range(1.5f, 4.5f);

        private TaskStatus _status;

        private bool _interrupted;
        
        public override void OnAwake()
        {
            Animator = GetComponent<Animator>();
            AnimatorStateController = GetComponent<AnimatorStateController>();
        }

        public override void OnStart()
        {
            _interrupted = false;

            var pathPoints = PathPoints.Value;
            if (pathPoints == null || pathPoints.Count == 0 || Vector3.Distance(transform.position, pathPoints[^1]) < StopProjection)
            {
                _status = FastEndingStatus;
            }
            else
            {
                _status = TaskStatus.Running;
                StartCoroutine(MovingToTarget(pathPoints));
                StartMonitor(pathPoints);
            }
        }

        public override TaskStatus OnUpdate()
        {
            return _status;
        }

        public override void OnEnd()
        {
            Interrupt();

            AnimatorStateController.ChangeSpeed(0, null, () =>
            {
                if (Animator) Animator.SetBool(AnimatorConstants.AnimatorMoving, false);
            }, true);
        }
        
        protected void Interrupt()
        {
            _interrupted = true;
        }
        
        protected virtual void StartMonitor(List<Vector3> pathPoints) { }

        private IEnumerator MovingToTarget(IReadOnlyList<Vector3> pathPoints)
        {
            var movingSpeed = Speed;
            
            AnimatorStateController.ChangeSpeed(movingSpeed, () => Animator.SetBool(AnimatorConstants.AnimatorMoving, true));
            
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

                #region 强制旋转方向

                var rotation = transform.rotation;
                var rotationAngle = Quaternion.Angle(rotation, targetRotation);
                var changeSpeed = rotationAngle > 45;
                if (changeSpeed && NotInterrupted())
                {
                    AnimatorStateController.ChangeSpeed(0);
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
                    AnimatorStateController.ChangeSpeed(movingSpeed);
                }

                #endregion
                
                var targetProjection = 0f;
                do
                {
                    if (_interrupted) yield break; else yield return null;
                    
                    targetProjection = Vector3.Dot((targetPoint - transform.position).normalized, direction);
                } while (targetProjection > StopProjection);
            }
            
            _status = TaskStatus.Success;
        }

        private bool NotInterrupted()
        {
            return !_interrupted;
        }
    }
}