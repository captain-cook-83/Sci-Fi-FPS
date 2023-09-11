using Cinemachine;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Interactable
{
    public class ExplosionActiveController : ActiveController
    {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");      // material.SetColor(EmissionColor, Color.white);

        [SerializeField]
        private CinemachineImpulseSource impulseSource;

        protected override void OnActivate(ActivateEventArgs args)
        {
            base.OnActivate(args);
            
            GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
        }

        protected override void OnExplode()
        {
            base.OnExplode();
            
            impulseSource.GenerateImpulse(0.02f);
        }
    }
}