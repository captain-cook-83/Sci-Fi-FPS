using Cc83.Behaviors;
using Cc83.Interactable;
using Cc83.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cc83.Character
{
    public class EnemyAttackController : MonoBehaviour
    {
        // 左右两侧夹角之和，必须大于 TurningToTarget.MinAngle，否则会出现转向某一侧之后因不满足新的条件而立即转向另一侧的尴尬情况
        private const float LeftRetargetAngle = 35;
        private const float RightRetargetAngle = 15;

        [SerializeField]
        private Transform aimingTowards;
        
        private EnemyShootController _shootController;

        private SensorAgent.SensorTarget _sensorTarget;

        private float _nearDistance;
        
        private float _farDistance;
        
        private float _maxRepeatShootDelay;
        
        private float _nextShootTime = float.MaxValue;

        private Vector3 _currentDirection;

        private Vector3 _aimingTarget;
        
        private float _prevRotationAngle;

        private void Awake()
        {
            var weaponReference = GetComponent<WeaponReference>();
            if (weaponReference)
            {
                _shootController = weaponReference.weapon.GetComponent<EnemyShootController>();
            }
        }
        
        public void Active(SensorAgent.SensorTarget sensorTarget, float nearDistance, float farDistance, float maxRepeatShootDelay)
        {
            _sensorTarget = sensorTarget;
            _nearDistance = nearDistance;
            _farDistance = farDistance;

            _currentDirection = sensorTarget.direction;
            _prevRotationAngle = float.MaxValue;
            _maxRepeatShootDelay = maxRepeatShootDelay;
            _nextShootTime = 0;
            
            _aimingTarget = sensorTarget.targetAgent.HitPosition;
        }
        
        public int Tick()
        {
            var targetDirection = _sensorTarget.direction;
            if (!_currentDirection.Equals(targetDirection))     // 在目标未移动的情况下，_currentDirection 可以做到精准 Equals；而当前 NPC 的 forward 做不到这一点，从而无法进行当前检测优化
            {
                var targetRotation = Quaternion.LookRotation(targetDirection);
                var rotationAngle = Quaternion.Angle(transform.rotation, targetRotation);
                var dotDirection = VectorUtils.DotDirection2D(transform.forward, targetDirection);
                switch (dotDirection)
                {
                    case < 0:
                        if (rotationAngle > LeftRetargetAngle) return Mathf.FloorToInt(dotDirection);
                        break;
                    case > 0:
                        if (rotationAngle > RightRetargetAngle) return Mathf.CeilToInt(dotDirection);
                        break;
                }

                var currentHitPosition = _sensorTarget.targetAgent.HitPosition;
                var aimingDeviation = Vector3.SqrMagnitude(aimingTowards.position - currentHitPosition);
                if (aimingDeviation > 0.01f)       // 0.1m
                {
                    _aimingTarget = currentHitPosition;
                }
            }

            var aimingPosition = aimingTowards.position;
            if (!aimingPosition.Equals(_aimingTarget))
            {
                var position = Vector3.Lerp(aimingPosition, _aimingTarget, Time.deltaTime * 5f);            // TODO 旋转速度需要依据旋转量，不应该使用固定速度
                aimingTowards.position = position;
                
                if (Vector3.SqrMagnitude(position - _aimingTarget) < 0.01f)
                {
                    _aimingTarget = position;                       // 此处并非像常规差值结果一样执行 aimingTowards.position = _aimingTarget，而是反过来修改原始目标值 _aimingTarget，这样来营造射击时重新瞄准造成的偏差
                }
            }
            
            var currentTime = Time.time;
            if (currentTime > _nextShootTime)
            {
                var times = Random.Range(1, 6);
                var duration = times * _shootController.cdTime;

                _nextShootTime = currentTime + duration + Random.Range(0.5f, _maxRepeatShootDelay);
                _shootController.Shoot(times);
            }

            return 0;
        }
        
        public void Deactive()
        {
            _sensorTarget = null;
        }
    }
}