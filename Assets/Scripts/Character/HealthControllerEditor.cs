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
            foreach (var c in GetComponentsInChildren<Collider>())
            {
                c.GetOrAddComponent<MaterialType>().typeOfMaterial = materialTypeEnum;
                c.GetOrAddComponent<HealthListener>().healthController = this;
            }
        }
    }
}
