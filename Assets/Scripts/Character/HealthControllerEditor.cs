using System.Linq;
using Cc83.Interactable;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace Cc83.Character
{
    public partial class HealthController
    {
        public MaterialType.MaterialTypeEnum materialTypeEnum;
        
        [Button("Batch Link Colliders", ButtonSizes.Large)]
        public void BatchLinkColliders()
        {
            var colliders = GetComponentsInChildren<Collider>().Where(c => c.isTrigger);
            foreach (var c in colliders)
            {
                c.GetOrAddComponent<MaterialType>().typeOfMaterial = materialTypeEnum;
                c.GetOrAddComponent<HealthListener>().healthController = this;
            }
        }
    }
}
