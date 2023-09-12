using Cc83.Character;
using Cinemachine;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Interactable
{
    public class ExplosionActiveController : ActiveController
    {
        [SerializeField]
        private CinemachineImpulseSource impulseSource;
        
        [SerializeField]
        [Range(100, 1000)]
        private float damage = 500;

        [SerializeField]
        [Range(5, 30)]
        private float radius = 15;

        protected override void OnActivate(ActivateEventArgs args)
        {
            base.OnActivate(args);
            
            GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
        }

        protected override void OnExplode()
        {
            base.OnExplode();
            
            impulseSource.GenerateImpulse(0.02f);
            
            PhysicsManager.Instance.TakeExplosionDamage(transform.position, damage, radius);
        }
    }
}