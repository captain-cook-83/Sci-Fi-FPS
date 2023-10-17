using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;
using Cc83.Utils;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class AttackMoving : Action
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
        
        public override void OnAwake()
        {
            _animator = GetComponent<Animator>();
            _animatorStateController = GetComponent<AnimatorStateController>();
        }

        public override void OnStart()
        {
            _status = TaskStatus.Running;
            _coroutine = MovingToTarget(PathPoints.Value);
            StartCoroutine(_coroutine);
        }

        public override TaskStatus OnUpdate()
        {
            return _status;
        }

        public override void OnEnd()
        {
            _coroutine = null;
            _animatorStateController.ChangeSpeed(0, StopMoving);
            _animatorStateController.ChangeHSpeed(0, StopMoving);
        }

        public override void OnConditionalAbort()
        {
            StopCoroutine(_coroutine);
        }

        private IEnumerator MovingToTarget(IReadOnlyList<Vector3> pathPoints)
        {
            var movingSpeed = Random.Range(3, 4.5f);
            
            for (var i = 1; i < pathPoints.Count; i++)
            {
                var targetPoint = pathPoints[i];
                var position = transform.position;
                targetPoint.y = position.y;

                var forward = transform.forward;
                var direction = (targetPoint - position).normalized;
                var dotDirectionalAngle = VectorUtils.DotDirectionalAngle2D(forward, direction);

                var radian = Mathf.PI * dotDirectionalAngle / 180;
                var verticalSpeed = Mathf.Cos(radian) * movingSpeed;
                var horizontallySpeed = Mathf.Abs(Mathf.Sin(radian) * movingSpeed) * (dotDirectionalAngle < 0 ? -1 : 1);
                var hvRatio = horizontallySpeed / verticalSpeed;
                
                if (i == 1)
                {
                    _animatorStateController.CancelSpeed();
                    _animatorStateController.CancelHSpeed();
                    
                    _animator.SetBool(AnimatorConstants.AnimatorMoving, true);

                    var t = 0f;
                    var currentSpeed = _animator.GetFloat(AnimatorConstants.AnimatorSpeed);
                    var currentValue = currentSpeed;
                    while (Mathf.Abs(verticalSpeed - currentValue) > 0.001f)
                    {
                        t += Time.deltaTime;
                        currentValue = Mathf.Lerp(currentSpeed, verticalSpeed, t * 10);
                        _animator.SetFloat(AnimatorConstants.AnimatorSpeed, currentValue);
                        _animator.SetFloat(AnimatorConstants.AnimatorDirection, currentValue * hvRatio);
                        yield return null;
                    }
                }
                
                _animator.SetFloat(AnimatorConstants.AnimatorSpeed, verticalSpeed);
                _animator.SetFloat(AnimatorConstants.AnimatorDirection, horizontallySpeed);
                
                var targetProjection = 0f;
                do
                {
                    yield return null;
                    targetProjection = Vector3.Dot((targetPoint - transform.position).normalized, direction);
                } while (targetProjection > StopProjection);
            }
            
            _status = TaskStatus.Success;
        }
        
        private void StopMoving()
        {
            _animator.SetBool(AnimatorConstants.AnimatorMoving, false);
        }
    }
}