using Cc83.Behaviors;
using Cc83.Interactable;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cc83.Character
{
    public class EnemyAttackController : MonoBehaviour
    {
        [SerializeField]
        private Transform aimingTowards;
        
        private EnemyShootController _shootController;

        private SensorAgent.SensorTarget _sensorTarget;
        
        private float _maxRepeatShootDelay;
        
        private float _nextShootTime = float.MaxValue;

        private Vector3 _aimingTarget;

        private void Awake()
        {
            var weaponReference = GetComponent<WeaponReference>();
            if (weaponReference)
            {
                _shootController = weaponReference.weapon.GetComponent<EnemyShootController>();
            }
        }
        
        public void Active(SensorAgent.SensorTarget sensorTarget, float maxRepeatShootDelay)
        {
            _sensorTarget = sensorTarget;
            _aimingTarget = sensorTarget.targetAgent.HitPosition;
            _maxRepeatShootDelay = maxRepeatShootDelay;
            _nextShootTime = 0;
        }
        
        public void Tick()
        {
            TickAiming();
            
            var currentTime = Time.time;
            if (currentTime > _nextShootTime)
            {
                var times = Random.Range(1, 6);
                var duration = times * _shootController.cdTime;

                _nextShootTime = currentTime + duration + Random.Range(0.5f, _maxRepeatShootDelay);
                _shootController.Shoot(times);
            }
        }

        public void TickAiming(bool delayed = true)             // TODO 应当使用方向差进行旋转，更加高效准确
        {
            var currentHitPosition = _sensorTarget.targetAgent.HitPosition;
            var aimingDeviation = Vector3.SqrMagnitude(aimingTowards.position - currentHitPosition);
            if (aimingDeviation > 0.01f)       // 0.1m
            {
                _aimingTarget = currentHitPosition;
            }
            
            var aimingPosition = aimingTowards.position;
            if (!aimingPosition.Equals(_aimingTarget))
            {
                var speed = delayed ? 15 : 45;
                var position = Vector3.Lerp(aimingPosition, _aimingTarget, Time.deltaTime * speed);            // TODO 旋转速度需要依据旋转量，不应该使用固定速度
                aimingTowards.position = position;
                
                if (Vector3.SqrMagnitude(position - _aimingTarget) < 0.01f)
                {
                    _aimingTarget = position;                       // 此处并非像常规差值结果一样执行 aimingTowards.position = _aimingTarget，而是反过来修改原始目标值 _aimingTarget，这样来营造射击时重新瞄准造成的偏差
                }
            }
        }
    }
}