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
        private enum RotateType
        {
            None, Rotate, PreRotate
        }
        
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

        private readonly List<Vector3> _pathPoints = new (2);
        
        private readonly List<RotateType> _pathRotations = new (2);
        
        public override void OnAwake()
        {
            _animator = GetComponent<Animator>();
            _animatorStateController = GetComponent<AnimatorStateController>();
        }

        public override void OnStart()
        {
            var pathPoints = PathPoints.Value;
            for (var i = 2; i < pathPoints.Count; i++)
            {
                var n = i - 1;
                var direction = pathPoints[n] - pathPoints[n - 1];
                var length = direction.magnitude;
                direction.Normalize();

                var split = false;
                for (var j = 1; length > 6; j++)
                {
                    split = true;
                    length -= 4;
                    _pathPoints.Add(pathPoints[n - 1] + direction * (j * 4));
                    _pathRotations.Add(RotateType.None);
                }

                var lastPoint = pathPoints[n] - direction * 1.5f;
                if (split && length < 2)
                {
                    _pathPoints[^1] = lastPoint;
                    _pathRotations[^1] = RotateType.PreRotate;
                }
                else if (length >= 2)
                {
                    _pathPoints.Add(lastPoint);
                    _pathRotations.Add(RotateType.PreRotate);
                }
                
                _pathPoints.Add(pathPoints[n]);
                _pathRotations.Add(!split && length < 2 ? RotateType.Rotate : RotateType.None);
            }
            
            _pathPoints.Add(pathPoints[^1]);
            _pathRotations.Add(RotateType.None);
            
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
            OnConditionalAbort();
            
            _pathPoints.Clear();
            _pathRotations.Clear();
            _animatorStateController.ChangeSpeed(0, 
                () =>  _animatorStateController.ChangeHSpeed(0), 
                () => _animator.SetBool(AnimatorConstants.AnimatorMoving, false), true);
        }

        public override void OnConditionalAbort()
        {
            if (_coroutine == null) return;
            
            StopCoroutine(_coroutine);
            _coroutine = null;
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
                var hvRatio = horizontallySpeed / verticalSpeed;
                
                if (i == 0)
                {
                    _animatorStateController.CancelSpeed();
                    _animatorStateController.CancelHSpeed();
                    
                    _animator.SetBool(AnimatorConstants.AnimatorMoving, true);
                }

                var t = 0f;
                var currentSpeed = _animator.GetFloat(AnimatorConstants.AnimatorSpeed);
                var currentValue = currentSpeed;
                while (Mathf.Abs(verticalSpeed - currentValue) > 0.001f)
                {
                    t += Time.deltaTime;
                    yield return null;
                    currentValue = Mathf.Lerp(currentSpeed, verticalSpeed, t * 10);
                    _animator.SetFloat(AnimatorConstants.AnimatorSpeed, currentValue);
                    _animator.SetFloat(AnimatorConstants.AnimatorDirection, currentValue * hvRatio);
                }
                
                _animator.SetFloat(AnimatorConstants.AnimatorSpeed, verticalSpeed);
                _animator.SetFloat(AnimatorConstants.AnimatorDirection, horizontallySpeed);
                
                var targetProjection = 0f;
                do
                {
                    yield return null;
                    targetProjection = Vector3.Dot((targetPoint - transform.position).normalized, direction);
                } while (targetProjection > StopProjection);

                var pathRotation = _pathRotations[i];
                if (pathRotation == RotateType.None) continue;
                
                var index = pathRotation == RotateType.Rotate ? i + 1 : i + 2;
                var targetRotation = Quaternion.LookRotation(_pathPoints[index] - _pathPoints[index - 1]);
                var rotation = transform.rotation;
                var rotationAngle = Quaternion.Angle(rotation, targetRotation);
                if (rotationAngle < 45)
                {
                    continue;
                }
                    
                t = 0f;
                currentValue = verticalSpeed;
                while (currentValue > 0)
                {
                    t += Time.deltaTime;
                    yield return null;
                    currentValue = Mathf.Lerp(verticalSpeed, 0, t * 10);
                    _animator.SetFloat(AnimatorConstants.AnimatorSpeed, currentValue);
                    _animator.SetFloat(AnimatorConstants.AnimatorDirection, currentValue * hvRatio);
                }

                t = 0;
                rotation = transform.rotation;
                var prevRotationAngle = 0f;
                do
                {
                    t += Time.deltaTime;
                    yield return null;
                    rotation = Quaternion.Lerp(rotation, targetRotation, t * 2f);
                    prevRotationAngle = rotationAngle;
                    rotationAngle = Quaternion.Angle(rotation, targetRotation);
                    transform.rotation = rotation;
                } while (prevRotationAngle > rotationAngle);
                
                transform.rotation = targetRotation;
            }
            
            _status = TaskStatus.Success;
        }
    }
}