using Cc83.Behaviors;
using Cc83.Interactable;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cc83.Character
{
    public class EnemyAttackController : MonoBehaviour
    {
        public bool IsActive { get; private set; }
        
        private EnemyShootController _shootController;
        
        private float _maxRepeatShootDelay;
        
        private float _nextShootTime = float.MaxValue;

        private void Awake()
        {
            var weaponReference = GetComponent<WeaponReference>();
            if (weaponReference)
            {
                _shootController = weaponReference.weapon.GetComponent<EnemyShootController>();
            }
        }

        private void Update()
        {
            var currentTime = Time.time;
            if (currentTime > _nextShootTime)
            {
                var times = Random.Range(1, 6);
                var duration = times * _shootController.cdTime;

                _nextShootTime = currentTime + duration + Random.Range(0.5f, _maxRepeatShootDelay);
                _shootController.Shoot(times);
            }
        }
        
        public void Active(SensorAgent.SensorTarget sensorTarget, float nearDistance, float farDistance, float maxRepeatShootDelay)
        {
            _maxRepeatShootDelay = maxRepeatShootDelay;
            _nextShootTime = 0;

            IsActive = true;
        }

        public void Deactive()
        {
            _nextShootTime = float.MaxValue;

            IsActive = false;
        }
    }
}