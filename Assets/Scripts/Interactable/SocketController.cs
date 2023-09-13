using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Interactable
{
    public class SocketController : MonoBehaviour
    {
        public void OnHoverEntered(HoverEnterEventArgs args)
        {
            AkSoundEngine.PostEvent(AK.EVENTS.MOUNT_WEAPON, gameObject);
        }
        
        public void OnSelectEntered(SelectEnterEventArgs args)
        {
            AkSoundEngine.PostEvent(AK.EVENTS.MOUNT_WEAPON_READY, gameObject);
        }
    }
}