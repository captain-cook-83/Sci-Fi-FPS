using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Interactable
{
    public class SocketController : MonoBehaviour
    {
        public void OnHoverEntered(HoverEnterEventArgs args)
        {
            AkSoundEngine.PostEvent(AK.EVENTS.CATCH_PISTOL_PLAYER, gameObject);
        }
        
        public void OnSelectEntered(SelectEnterEventArgs args)
        {
            // AkSoundEngine.PostEvent(AK.EVENTS.CATCH_PISTOL_PLAYER, gameObject);
        }
    }
}