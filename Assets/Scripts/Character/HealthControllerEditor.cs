using System.Linq;
using Cc83.Interactable;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.VisualScripting;
using UnityEngine;

namespace Cc83.Character
{
    public partial class HealthController
    {
#if UNITY_EDITOR
        public MaterialType.MaterialTypeEnum materialTypeEnum;

        public Transform testHitPart;
        
        [Button("Batch Link Colliders", ButtonSizes.Large)]
        public void BatchLinkColliders()
        {
            rigidbodies = GetComponentsInChildren<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>();
            
            foreach (var c in colliders)
            {
                c.isTrigger = true;
                c.GetOrAddComponent<MaterialType>().typeOfMaterial = materialTypeEnum;
                c.GetOrAddComponent<HealthListener>().healthController = this;
            }
        }

        [Button("Enable Join Collision", ButtonSizes.Large)]
        public void EnableJoinCollision()
        {
            GetComponentsInChildren<ConfigurableJoint>()
                .Where(cj => !(cj.name.EndsWith("Foot") || cj.name.EndsWith("Hand") || cj.name.EndsWith("Head") || cj.name.EndsWith("Hips")))
                .ForEach(cj => cj.enableCollision = true);
        }
        
        [Button("Disable Join Collision", ButtonSizes.Large)]
        public void DisableJoinCollision()
        {
            GetComponentsInChildren<ConfigurableJoint>().ForEach(cj => cj.enableCollision = false);
        }
        
        [Button("Test Death", ButtonSizes.Large)]
        public void Death()
        {
            var position = testHitPart.position;
            var direction = - transform.forward;
            TakeDamage(100, testHitPart, ref position, ref direction, true);
        }
#endif
    }
}
