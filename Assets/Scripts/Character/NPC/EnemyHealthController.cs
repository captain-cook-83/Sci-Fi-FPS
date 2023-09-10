using System.Collections;
using Sirenix.Utilities;
using UnityEngine;

namespace Cc83.Character
{
    public partial class EnemyHealthController : HealthController
    {
        [SerializeField]
        private Rigidbody[] rigidbodies;
        
        [SerializeField]
        private Collider[] colliders;

        private bool _alive = true;

        private void Awake()
        {
            rigidbodies.ForEach(rb => rb.maxDepenetrationVelocity = 0.01f);
        }

        public override void TakeDamage(float damage, Transform part, ref Vector3 hitPoint, ref Vector3 direction, bool headShoot = false)
        {
            if (!_alive) return;
            
            hp -= damage;
            if (hp > 0 && !headShoot) return;
            
            _alive = false;
            
            animator.SetTrigger(TriggerDeath);
            colliders.ForEach(c => c.isTrigger = false);
            rigidbodies.ForEach(rb =>
            {
                rb.isKinematic = false;
                // rb.velocity = Vector3.zero;
            });
            
            var partRigidbody = part.GetComponent<Rigidbody>();
            if (partRigidbody)
            {
                partRigidbody.AddForceAtPosition(direction.normalized * 100, hitPoint, ForceMode.Impulse);       //TODO 力量随距离衰减
            }
            
            GetComponent<WeaponReference>().DropDown();

            // StartCoroutine(FreezeBody());
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
