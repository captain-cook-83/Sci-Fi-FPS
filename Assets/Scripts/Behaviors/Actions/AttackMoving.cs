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
            _animatorStateController.ChangeSpeed(0, 
                () =>  _animator.SetFloat(AnimatorConstants.AnimatorDirection, 0), 
                () => _animator.SetBool(AnimatorConstants.AnimatorMoving, true), true);
            
            StartCoroutine(_coroutine);
        }

        public override TaskStatus OnUpdate()
        {
            return _status;
        }

        public override void OnEnd()
        {
            _coroutine = null;
            _animatorStateController.ChangeSpeed(0, 
                () =>  _animator.SetFloat(AnimatorConstants.AnimatorDirection, 0), 
                () => _animator.SetBool(AnimatorConstants.AnimatorMoving, false), true);
        }

        public override void OnConditionalAbort()
        {
            StopCoroutine(_coroutine);
        }

        private IEnumerator MovingToTarget(IReadOnlyList<Vector3> pathPoints)
        {
            var movingSpeed = Random.Range(1.5f, 4.5f);
            
            for (var i = 1; i < pathPoints.Count; i++)
            {
                var targetPoint = pathPoints[i];
                var position = transform.position;
                targetPoint.y = position.y;

                var forward = transform.forward;
                var direction = (targetPoint - position).normalized;
                var dotDirectionalAngle = VectorUtils.DotDirectionalAngle2D(forward, direction);

                var verticalSpeed = Vector3.Dot(forward, direction) < 0 ? -movingSpeed : movingSpeed;
                var horizontallySpeed = Mathf.Abs(Mathf.Tan(Mathf.PI * dotDirectionalAngle / 180) * movingSpeed) * (dotDirectionalAngle < 0 ? -1 : 1);
                
                if (i == 1)
                {
                    _animatorStateController.ChangeSpeed(verticalSpeed, () => _animator.SetFloat(AnimatorConstants.AnimatorDirection, horizontallySpeed));
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
            }
            
            _status = TaskStatus.Success;
        }
    }
}