using UnityEngine;

namespace Cc83.Character
{
    public class HealthListener : MonoBehaviour
    {
        [SerializeField]
        public HealthController healthController;

        [Range(1, 100)]
        public float damage = 1;

        public bool headShoot;

        public void TakeDamage(Vector3 hitPoint, Vector3 direction)
        {
            healthController.TakeDamage(damage, transform, ref hitPoint, ref direction, headShoot);
        }
    }
}
