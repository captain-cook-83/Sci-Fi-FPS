using UnityEngine;

namespace Cc83.Character
{
    public abstract class HealthController : MonoBehaviour
    {
        [Range(1, 100)]
        public float hp = 100;

        public HealthListener[] lethalParts;
        
        [SerializeField]
        protected Animator animator;
        
        public abstract void TakeDamage(float damage, Transform part, ref Vector3 hitPoint, ref Vector3 direction, bool headShoot = false, float extraDamage = 0);
    }
}