using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace Cc83.Character
{
    public partial class PlayerHealthController
    {
#if UNITY_EDITOR
        [Button("Setup", ButtonSizes.Large)]
        public void Setup()
        {
            hp = 100;
            animator = GetComponent<Animator>();
            
            foreach (var c in GetComponentsInChildren<Collider>())
            {
                c.isTrigger = true;
                c.excludeLayers = -1;
                c.layerOverridePriority = 100;
                c.GetOrAddComponent<HealthListener>().healthController = this;
            }
        }
#endif
    }
}