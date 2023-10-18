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
        // ReSharper disable once MemberCanBePrivate.Global
        public const float LeftRetargetAngle = 35;
        // ReSharper disable once MemberCanBePrivate.Global
        public const float RightRetargetAngle = 15;
        
        [SerializeField] 
        private Transform aimingAxis;
        
        private EnemyShootController _shootController;

        private EnemyWeaponIKController _weaponIKController;

        private SensorAgent.SensorTarget _sensorTarget;
        
        private float _maxRepeatShootDelay;
        
        private float _nextShootTime = float.MaxValue;

        private bool _validAiming;

        private bool _shoot;

        private void Awake()
        {
            var weaponReference = GetComponent<WeaponReference>();
            if (weaponReference)
            {
                _shootController = weaponReference.weapon.GetComponent<EnemyShootController>();
            }
            
            _weaponIKController = GetComponent<EnemyWeaponIKController>();
        }

        public void Active(SensorAgent.SensorTarget sensorTarget, float maxRepeatShootDelay)
        {
            _sensorTarget = sensorTarget;
            _maxRepeatShootDelay = maxRepeatShootDelay;
            _nextShootTime = Time.time + 0.5f;                      // 延后 0.5s，确保可能出现的转身动作后的枪口朝向已经对齐
            _validAiming = true;

            _weaponIKController.blockAiming = true;
        }
        
        public bool Tick()
        {
            if (!_validAiming) return false;
            if (_nextShootTime > Time.time) return true;
            
            var times = Random.Range(1, 4);     // 6
            var duration = times * _shootController.cdTime;

            _nextShootTime = Time.time + duration + Random.Range(0.5f, _maxRepeatShootDelay);
            _shootController.Shoot(times);
            return true;
        }

        public void TickAiming()
        {
            var t = transform;
            var hitPosition = _sensorTarget.targetAgent.HitPosition;
            var directionAngle = VectorUtils.DotDirectionalAngle2D(t.forward, hitPosition - t.position);

            _validAiming = directionAngle is > -LeftRetargetAngle and < RightRetargetAngle;
            if (_validAiming)
            {
                var direction = hitPosition - aimingAxis.transform.position;
                direction.y = 0;
                aimingAxis.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }

        public void Reset()
        {
            _weaponIKController.blockAiming = false;
        }
    }
}