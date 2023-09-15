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

        public override void TakeDamage(float damage, Transform part, ref Vector3 hitPoint, ref Vector3 direction, bool headShoot = false, float extraDamage = 0)
        {
            if (!_alive && extraDamage == 0) return;
            
            hp -= damage + extraDamage;
            if (hp > 0 && !headShoot) return;
            
            _alive = false;

            if (animator)
            {
                animator.SetTrigger(TriggerDeath);
            }
            
            colliders.ForEach(c => c.isTrigger = false);
            rigidbodies.ForEach(rb =>
            {
                rb.isKinematic = false;
                // rb.velocity = Vector3.zero;
            });

            var force = direction.normalized * 100;
            var partRigidbody = part.GetComponent<Rigidbody>();
            if (partRigidbody)
            {
                partRigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);       //TODO 力量随距离衰减
            }

            if (extraDamage > 0)
            {
                GetComponent<WeaponReference>().DropDown(force);
            }
            else
            {
                GetComponent<WeaponReference>().DropDown();
            }
            
            StartCoroutine(FreezeBody());
        }

        private IEnumerator FreezeBody()
        {
            yield return null;
            
            Destroy(animator);
            animator = null;
            
            yield return new WaitForSeconds(5);
            
            foreach (var rb in rigidbodies)
            {
                rb.isKinematic = true;
                yield return null;
            }
        }
    }
}
