using Unity.XR.CoreUtils.Bindings;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using Event = AK.Wwise.Event;

namespace Cc83.Interactable
{
    public class WwiseAffordanceReceiver : MonoBehaviour
    {
        [SerializeField]
        private BaseAffordanceStateProvider affordanceStateProvider;

        [SerializeField] 
        private AkGameObj akGameObj;

        [SerializeField] 
        private Event audioEvent;
        
        private readonly BindingsGroup _bindingsGroup = new ();
        
        private byte _lastAffordanceStateIndex = AffordanceStateShortcuts.idle;
        
        protected void OnValidate()
        {
            if (affordanceStateProvider == null)
            {
                affordanceStateProvider = GetComponentInParent<BaseAffordanceStateProvider>();
            }

            if (akGameObj == null)
            {
                akGameObj = GetComponentInParent<AkGameObj>();
            }
        }

        protected void OnEnable()
        {
            _bindingsGroup.AddBinding(affordanceStateProvider.currentAffordanceStateData.Subscribe(OnAffordanceStateUpdated));
        }
        
        protected void OnDisable()
        {
            _bindingsGroup.Clear();
        }

        private void OnAffordanceStateUpdated(AffordanceStateData affordanceStateData)
        {
            var newIndex = affordanceStateData.stateIndex;
            if (newIndex == _lastAffordanceStateIndex) return;
            
            if (newIndex == AffordanceStateShortcuts.hovered && _lastAffordanceStateIndex == AffordanceStateShortcuts.idle)
            {
                audioEvent.Post(akGameObj ? akGameObj.gameObject : gameObject);
            }
                
            _lastAffordanceStateIndex = newIndex;
        }
    }

}