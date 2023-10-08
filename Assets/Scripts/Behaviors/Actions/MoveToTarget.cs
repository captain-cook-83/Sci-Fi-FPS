using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Utils;
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
        
        protected TaskStatus Status;
        
        private Seeker _seeker;

        private bool _interrupted;
        
        public override void OnAwake()
        {
            Animator = GetComponent<Animator>();
            _seeker = GetComponent<Seeker>();
        }

        public override void OnStart()
        {
            Status = TaskStatus.Running;
            _interrupted = false;
            _seeker.StartPath(transform.position, TargetPosition.Value, OnPathCalculated);
        }

        public override TaskStatus OnUpdate()
        {
            return Status;
        }

        public override void OnEnd()
        {
            _interrupted = true;
            
            Animator.SetBool(AnimatorConstants.AnimatorMoving, false);
            Animator.SetFloat(AnimatorConstants.AnimatorSpeed, 0);
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
            
            StartCoroutine(AnimatorUtils.ChangeFloat(Animator, AnimatorConstants.AnimatorTensity, Tensity, NotInterrupted));
            StartCoroutine(MovingToTarget(path.vectorPath));
        }

        private IEnumerator MovingToTarget(IReadOnlyList<Vector3> pathPoints)
        {
            var movingSpeed = Random.Range(1.5f, 4.5f);
            var prevCoroutine = AnimatorUtils.ChangeFloat(Animator, AnimatorConstants.AnimatorSpeed, movingSpeed, NotInterrupted);
            System.Action clearCoroutine = () => prevCoroutine = null;
            
            StartCoroutine(prevCoroutine);
            Animator.SetBool(AnimatorConstants.AnimatorMoving, true);
            
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
                
                if (changeSpeed)
                {
                    StopNullableCoroutine(prevCoroutine);

                    if (!_interrupted)
                    {
                        // prevCoroutine = null;
                        // _animator.SetFloat(AnimatorConstants.AnimatorSpeed, 0);
                        prevCoroutine = AnimatorUtils.ChangeFloat(Animator, AnimatorConstants.AnimatorSpeed, 0, NotInterrupted, 1, clearCoroutine);
                        StartCoroutine(prevCoroutine);
                    }
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

                if (changeSpeed)
                {
                    StopNullableCoroutine(prevCoroutine);
                    
                    if (!_interrupted)
                    {
                        prevCoroutine = AnimatorUtils.ChangeFloat(Animator, AnimatorConstants.AnimatorSpeed, movingSpeed, NotInterrupted, 1, clearCoroutine);
                        StartCoroutine(prevCoroutine);
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

        private void StopNullableCoroutine(IEnumerator coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        private bool NotInterrupted()
        {
            return !_interrupted;
        }
    }
}