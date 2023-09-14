using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Character
{
    public class RayInteractorController : MonoBehaviour
    {
        [SerializeField]
        private InteractionLayerMask hideLineForLayers;
        
        private LineRenderer _lineRenderer;
        
        private XRInteractorLineVisual _xrInteractorLineVisual;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _xrInteractorLineVisual = GetComponent<XRInteractorLineVisual>();
        }
        
        public void OnSelectEntered(SelectEnterEventArgs args)
        {
            var interactionLayers = args.interactableObject.interactionLayers.value;
            if ((interactionLayers & hideLineForLayers.value) == interactionLayers)
            {
                _lineRenderer.enabled = _xrInteractorLineVisual.enabled = false;
            }
        }
        
        public void OnSelectExited(SelectExitEventArgs args)
        {
            var interactionLayers = args.interactableObject.interactionLayers.value;
            if ((interactionLayers & hideLineForLayers.value) == interactionLayers)
            {
                _lineRenderer.enabled = _xrInteractorLineVisual.enabled = true;
            }
        }
    }
}
