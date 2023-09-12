using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Event = AK.Wwise.Event;

namespace Cc83.Interactable
{
    public class ActiveController : MonoBehaviour
    {
        [SerializeField]
        private XRGrabInteractable interactable;
        
        [SerializeField]
        private GameObject explodeEffect;
        
        [SerializeField]
        [Range(0, 5)]
        private float explodeDelayAfterCollision = 1f;

        [SerializeField]
        [Range(3, 5)]
        private float explodeDelayBeforeThrow = 4.5f;

        [SerializeField] 
        private Event activeEvent;

        [SerializeField] 
        private Event comingEvent;

        [SerializeField] 
        private Event momentEvent;

        private bool _explodeOnCollision;
        
        private Coroutine _pendingExplosion;
        
        private void Awake()
        {
            interactable.activated.AddListener(OnActivate);
            interactable.deactivated.AddListener(OnDeactivate);
            interactable.selectExited.AddListener(OnDeselected);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (_explodeOnCollision)
            {
                _pendingExplosion = StartCoroutine(ExplodeEffect(explodeDelayAfterCollision));
            }
        }

        private void OnDestroy()
        {
            interactable.activated.RemoveListener(OnActivate);
            interactable.deactivated.RemoveListener(OnDeactivate);
            interactable.selectExited.RemoveListener(OnDeselected);

            if (_pendingExplosion != null)
            {
                StopCoroutine(_pendingExplosion);
            }
        }

        protected virtual void OnActivate(ActivateEventArgs args)
        {
            activeEvent?.Post(gameObject);
        }

        protected virtual void OnExplode() { }

        private void OnDeselected(SelectExitEventArgs args)
        {
            if (_pendingExplosion != null)
            {
                StopCoroutine(_pendingExplosion);
                
                _pendingExplosion = null;
                _explodeOnCollision = true;
            }
        }

        private void OnDeactivate(DeactivateEventArgs args)
        {
            _pendingExplosion = StartCoroutine(ExplodeEffect(explodeDelayBeforeThrow));
            
            comingEvent?.Post(gameObject);
        }

        private IEnumerator ExplodeEffect(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            _pendingExplosion = null;

            comingEvent?.Stop(gameObject);
            momentEvent?.Post(gameObject);
            
            OnExplode();
            Destroy(Instantiate(explodeEffect, transform.position, Quaternion.identity), 2);
            Destroy(gameObject);
        }
    }
}