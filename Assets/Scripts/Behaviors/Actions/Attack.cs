using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;
using Cc83.Interactable;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class Attack : Action
    {
        [Range(0.5f, 5)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float MaxRepeatShootDelay = 3;
        
        // ReSharper disable once UnassignedField.Global
        public SharedFloat AttackFarDistance;
        
        // ReSharper disable once UnassignedField.Global
        public SharedFloat AttackNearDistance;
        
        // ReSharper disable once UnassignedField.Global
        public SharedSensorTargetList Enemies;
        
        private EnemyShootController _shootController;

        private float _nextShootTime;

        public override void OnAwake()
        {
            var weaponReference = GetComponent<WeaponReference>();
            if (weaponReference)
            {
                _shootController = weaponReference.weapon.GetComponent<EnemyShootController>();
            }
        }
        
        public override TaskStatus OnUpdate()
        {
            if (_shootController.IsEnabled)
            {
                var currentTime = Time.time;
                if (currentTime > _nextShootTime)
                {
                    var times = Random.Range(1, 6);
                    var duration = times * _shootController.cdTime;

                    _nextShootTime = currentTime + duration + Random.Range(0.5f, MaxRepeatShootDelay);
                    _shootController.Shoot(times);
                }
                
                return TaskStatus.Running;
            }
            
            return TaskStatus.Failure;
        }
    }
}
