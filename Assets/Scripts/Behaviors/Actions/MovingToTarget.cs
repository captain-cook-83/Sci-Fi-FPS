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
    public class MovingToTarget : Action
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public float StopDistance = 0.1f;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3 TargetPosition;
        
        private Animator _animator;
        
        private Seeker _seeker;
        
        private TaskStatus _status;
        
        public override void OnAwake()
        {
            _animator = GetComponent<Animator>();
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
            if (path.error || path.vectorPath.Count == 0)
            {
                Debug.LogError($"Pathfinding error: {path.errorLog}.");
                _status = TaskStatus.Failure;
                return;
            }
            
            StartCoroutine(AnimatorUtils.ChangeFloat(_animator, AnimatorConstants.AnimatorTensity, 1));
            StartCoroutine(MoveToTarget(path.vectorPath));
        }

        private IEnumerator MoveToTarget(List<Vector3> pathPoints)
        {
            var movingSpeed = Random.Range(1.5f, 4.5f);
            var prevCoroutine = AnimatorUtils.ChangeFloat(_animator, AnimatorConstants.AnimatorSpeed, movingSpeed);
            System.Action clearCoroutine = () => prevCoroutine = null;
            
            StartCoroutine(prevCoroutine);
            _animator.SetBool(AnimatorConstants.AnimatorMoving, true);
            
            for (var i = 1; i < pathPoints.Count; i++)
            {
                var targetPoint = pathPoints[i];
                var position = transform.position;
                targetPoint.y = position.y;
                
                var direction = targetPoint - position;
                var targetRotation = Quaternion.LookRotation(direction);
                
                var rotation = transform.rotation;
                var rotationAngle = Quaternion.Angle(rotation, targetRotation);
                var changeSpeed = rotationAngle > 45;
                
                if (changeSpeed)
                {
                    StopNullableCoroutine(prevCoroutine);
                    // prevCoroutine = null;
                    // _animator.SetFloat(AnimatorConstants.AnimatorSpeed, 0);
                    prevCoroutine = AnimatorUtils.ChangeFloat(_animator, AnimatorConstants.AnimatorSpeed, 0, 1, clearCoroutine);
                    StartCoroutine(prevCoroutine);
                }
                
                var prevRotationAngle = 0f;
                do
                {
                    yield return null;
                    rotation = Quaternion.Lerp(rotation, targetRotation, Time.deltaTime * 10f);
                    prevRotationAngle = rotationAngle;
                    rotationAngle = Quaternion.Angle(rotation, targetRotation);
                    transform.rotation = rotation;
                } while (prevRotationAngle > rotationAngle);

                transform.rotation = targetRotation;

                if (changeSpeed)
                {
                    StopNullableCoroutine(prevCoroutine);
                    prevCoroutine = AnimatorUtils.ChangeFloat(_animator, AnimatorConstants.AnimatorSpeed, movingSpeed, 1, clearCoroutine);
                    StartCoroutine(prevCoroutine);
                }
                
                var targetProjection = 0f;
                do
                {
                    yield return null;
                    targetProjection = Vector3.Dot(targetPoint - transform.position, direction);
                } while (targetProjection > StopDistance);
                
                yield return null;
            }
            
            _status = TaskStatus.Success;
            
            // 异步停止，使动作更加连贯
            StartCoroutine(AnimatorUtils.ChangeFloat(_animator, AnimatorConstants.AnimatorSpeed, 0, 0.2f, () =>
            {
                _animator.SetBool(AnimatorConstants.AnimatorMoving, false);
            })); 
        }

        private void StopNullableCoroutine(IEnumerator coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
    }
}