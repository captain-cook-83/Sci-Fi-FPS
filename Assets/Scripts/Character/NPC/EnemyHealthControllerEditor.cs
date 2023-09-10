using Cc83.Interactable;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace Cc83.Character
{
    public partial class EnemyHealthController
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
