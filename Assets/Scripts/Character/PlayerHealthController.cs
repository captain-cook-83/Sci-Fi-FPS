using Sirenix.Utilities;
using UnityEngine;

namespace Cc83.Character
{
    public partial class PlayerHealthController : HealthController
    {
        [SerializeField]
        private HandController[] handControllers;

        public override void TakeDamage(float damage, Transform part, ref Vector3 hitPoint, ref Vector3 direction, bool headShoot = false, float extraDamage = 0)
        {
            hp -= damage;
            handControllers.ForEach(c => c.WaggleShake());
        }
    }
}
