using Cc83.Character;
using Cinemachine;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Interactable
{
    public class GrenadeActiveController : ActiveController
    {
        private const string KwEmission = "_EMISSION";
        
        [SerializeField]
        private CinemachineImpulseSource impulseSource;
        
        [SerializeField]
        [Range(100, 1000)]
        private float damage = 500;
        
        [SerializeField]
        [Range(100, 1000)]
        private float force = 500;

        [SerializeField]
        [Range(5, 30)]
        private float radius = 5;

        private Material _material;

        protected override void Awake()
        {
            base.Awake();

            _material = GetComponent<MeshRenderer>().material;
        }
        
        private void OnEnable()
        {
            _material.DisableKeyword(KwEmission);
        }

        protected override void OnActivate(ActivateEventArgs args)
        {
            base.OnActivate(args);
            
            _material.EnableKeyword(KwEmission);
        }

        protected override void OnExplode()
        {
            base.OnExplode();
            
            impulseSource.GenerateImpulse(0.02f);
            
            PhysicsManager.Instance.TakeExplosionDamage(transform.position, radius, force, damage);
        }
    }
}