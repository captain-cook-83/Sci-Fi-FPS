using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;
using Cc83.Utils;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class SearchMoving : Action
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public float StopProjection = 0.1f;
        
        // ReSharper disable once UnassignedField.Global
        public SharedVector3List PathPoints;
        
        private Animator _animator;

        private AnimatorStateController _animatorStateController;
        
        private IEnumerator _coroutine;

        private TaskStatus _status;

        private List<Vector3> _pathPoints = new (2);
        
        private List<bool> _pathRotations = new (2);
        
        public override void OnAwake()
        {
            _animator = GetComponent<Animator>();
            _animatorStateController = GetComponent<AnimatorStateController>();
        }

        // public override void OnDrawGizmos()
        // {
        //     for (var i = 1; i < _pathPoints.Count; i++)
        //     {
        //         Debug.DrawLine(_pathPoints[i - 1] + Vector3.up, _pathPoints[i] + Vector3.up, Color.black);
        //     }
        // }

        public override void OnStart()
        {
            var pathPoints = PathPoints.Value;
            for (var i = 2; i < pathPoints.Count; i++)
            {
                var n = i - 1;
                var direction = pathPoints[n] - pathPoints[n - 1];
                if (direction.sqrMagnitude > 2)
                {
                    _pathPoints.Add(pathPoints[n] - direction.normalized);
                    _pathRotations.Add(true);
                }
                
                _pathPoints.Add(pathPoints[n]);
                _pathRotations.Add(false);
            }
            
            _pathPoints.Add(pathPoints[^1]);
            
            _status = TaskStatus.Running;
            _coroutine = MovingToTarget(_pathPoints);
            StartCoroutine(_coroutine);
        }

        public override TaskStatus OnUpdate()
        {
            for (var i = 1; i < _pathPoints.Count; i++)
            {
                Debug.DrawLine(_pathPoints[i - 1] + Vector3.up * 0.05f, _pathPoints[i] + Vector3.up * 0.05f, Color.black);
            }
            
            return _status;
        }

        public override void OnEnd()
        {
            _pathPoints.Clear();
            _pathRotations.Clear();
            
            _coroutine = null;
            _animatorStateController.ChangeSpeed(0, 
                () =>  _animatorStateController.ChangeHSpeed(0), 
                () => _animator.SetBool(AnimatorConstants.AnimatorMoving, false), true);
        }

        public override void OnConditionalAbort()
        {
            StopCoroutine(_coroutine);
        }

        private IEnumerator MovingToTarget(IReadOnlyList<Vector3> pathPoints)
        {
            var movingSpeed = Random.Range(2, 4.5f);
            
            for (var i = 0; i < pathPoints.Count; i++)
            {
                var targetPoint = pathPoints[i];
                var position = transform.position;
                targetPoint.y = position.y;

                var forward = transform.forward;
                var direction = (targetPoint - position).normalized;
                var dotDirectionalAngle = VectorUtils.DotDirectionalAngle2D(forward, direction);

                var verticalSpeed = Mathf.Cos(Mathf.PI * dotDirectionalAngle / 180) * movingSpeed;
                var horizontallySpeed = Mathf.Abs(Mathf.Sin(Mathf.PI * dotDirectionalAngle / 180) * movingSpeed) * (dotDirectionalAngle < 0 ? -1 : 1);
                
                if (i == 0)
                {
                    _animatorStateController.ChangeSpeed(verticalSpeed);
                    _animatorStateController.ChangeHSpeed(horizontallySpeed);
                    _animator.SetBool(AnimatorConstants.AnimatorMoving, true);
                }
                else
                {
                    _animator.SetFloat(AnimatorConstants.AnimatorSpeed, verticalSpeed);
                    _animator.SetFloat(AnimatorConstants.AnimatorDirection, horizontallySpeed);
                }
                
                var targetProjection = 0f;
                do
                {
                    yield return null;
                    targetProjection = Vector3.Dot((targetPoint - transform.position).normalized, direction);
                } while (targetProjection > StopProjection);

                if (_pathRotations[i])
                {
                    direction = _pathPoints[i + 2] - _pathPoints[i + 1];
                        
                    var targetRotation = Quaternion.LookRotation(direction);
                    var rotation = transform.rotation;
                    var rotationAngle = Quaternion.Angle(rotation, targetRotation);
                    // var changeSpeed = rotationAngle > 45;
                    // if (changeSpeed)
                    // {
                    //     _animatorStateController.ChangeSpeed(0);
                    //     _animatorStateController.ChangeHSpeed(0);
                    // }
                
                    // var prevRotationAngle = 0f;
                    // do
                    // {
                    //     yield return null;
                    //
                    //     rotation = Quaternion.Lerp(rotation, targetRotation, Time.deltaTime * 10f);
                    //     prevRotationAngle = rotationAngle;
                    //     rotationAngle = Quaternion.Angle(rotation, targetRotation);
                    //     transform.rotation = rotation;
                    // } while (prevRotationAngle > rotationAngle);
                
                    transform.rotation = targetRotation;
                
                    // if (changeSpeed)
                    // {
                    //     _animatorStateController.ChangeSpeed(verticalSpeed);
                    //     _animatorStateController.ChangeHSpeed(horizontallySpeed);
                    // }
                }
            }
            
            _status = TaskStatus.Success;
        }
    }
}