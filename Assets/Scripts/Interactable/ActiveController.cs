using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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

        private bool _explodeOnCollision;
        
        private Coroutine _pendingExplosion;
        
        private void Awake()
        {
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
            interactable.deactivated.RemoveListener(OnDeactivate);
            interactable.selectExited.RemoveListener(OnDeselected);

            if (_pendingExplosion != null)
            {
                StopCoroutine(_pendingExplosion);
            }
        }

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
        }

        private IEnumerator ExplodeEffect(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            _pendingExplosion = null;
            
            Destroy(Instantiate(explodeEffect, transform.position, Quaternion.identity), 2);
            Destroy(gameObject);
        }
    }
}