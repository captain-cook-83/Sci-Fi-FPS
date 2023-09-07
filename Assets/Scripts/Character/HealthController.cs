using System.Collections;
using Sirenix.Utilities;
using UnityEngine;

namespace Cc83.Character
{
    public partial class HealthController : MonoBehaviour
    {
        private static readonly int TriggerDeath = Animator.StringToHash("Death");
        
        [Range(1, 100)]
        public float hp = 100;

        [SerializeField]
        private Rigidbody[] rigidbodies;
        
        [SerializeField]
        private Collider[] colliders;
        
        private Animator _animator;

        private bool _alive = true;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            
            foreach (var rb in GetComponentsInChildren<Rigidbody>())
            {
                rb.maxDepenetrationVelocity = 0.01f;
            }
        }

        public void TakeDamage(float damage, Transform part, ref Vector3 hitPoint, ref Vector3 direction, bool headShoot = false)
        {
            if (!_alive) return;
            
            hp -= damage;
            if (hp > 0 && !headShoot) return;
            
            _alive = false;
            _animator.SetTrigger(TriggerDeath);
            
            colliders.ForEach(c => c.isTrigger = false);
            rigidbodies.ForEach(rb =>
            {
                rb.isKinematic = false;
                // rb.velocity = Vector3.zero;
                // rb.maxDepenetrationVelocity = 0.01f;
            });
            
            var partRigidbody = part.GetComponent<Rigidbody>();
            if (partRigidbody)
            {
                partRigidbody.AddForceAtPosition(direction.normalized * 100, hitPoint, ForceMode.Impulse);       //TODO 力量随距离衰减
            }
            
            GetComponent<WeaponReference>().DropDown();

            StartCoroutine(FreezeBody());
        }

        private IEnumerator FreezeBody()
        {
            yield return new WaitForSeconds(5);
            
            foreach (var rb in rigidbodies)
            {
                rb.isKinematic = true;
                yield return null;
            }
        }
    }
}
