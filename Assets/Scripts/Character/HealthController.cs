using UnityEngine;

namespace Cc83.Character
{
    public partial class HealthController : MonoBehaviour
    {
        private static readonly int TriggerHeadShootDeath = Animator.StringToHash("HeadShootDeath");
        private static readonly int TriggerDeath = Animator.StringToHash("Death");
        
        [Range(1, 100)]
        public float hp = 100;
        
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void TakeDamage(float damage, Transform part, ref Vector3 hitPoint, ref Vector3 direction, bool headShoot = false)
        {
            hp -= damage;
            if (hp > 0) return;
            
            _animator.SetTrigger(TriggerDeath);
            
            GetComponent<WeaponReference>().DropDown();
            
            var partRigidbody = part.GetComponent<Rigidbody>();
            if (partRigidbody)
            {
                partRigidbody.AddForceAtPosition(direction.normalized * 100, hitPoint, ForceMode.Force);
            }
        }
    }
}
