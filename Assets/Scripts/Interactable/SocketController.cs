using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Interactable
{
    public class SocketController : MonoBehaviour
    {
        [SerializeField]
        private XRSocketInteractor socketInteractor;

        private void OnValidate()
        {
            if (socketInteractor == null)
            {
                socketInteractor = GetComponent<XRSocketInteractor>();
            }
        }

        public void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (socketInteractor.hasSelection) return;
            
            AkSoundEngine.PostEvent(AK.EVENTS.MOUNT_WEAPON, gameObject);
        }
        
        public void OnSelectEntered(SelectEnterEventArgs args)
        {
            AkSoundEngine.PostEvent(AK.EVENTS.MOUNT_WEAPON_READY, gameObject);
        }
    }
}