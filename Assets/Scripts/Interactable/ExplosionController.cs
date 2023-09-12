using System.Collections;
using Cc83.Character;
using Cinemachine;
using UnityEngine;
using Event = AK.Wwise.Event;

namespace Cc83.Interactable
{
    public class ExplosionController : MonoBehaviour
    {
        [SerializeField] 
        [Range(100, 1000)]
        private float hp = 200;
        
        [SerializeField]
        [Range(0.05f, 1)]
        private float explodeDelay = 0.2f;
        
        [SerializeField]
        private GameObject explodeEffect;
        
        [SerializeField]
        [Range(2, 10)]
        private float explodeEffectDuration = 3;
        
        [SerializeField]
        private GameObject followEffect;
        
        [SerializeField]
        [Range(0, 15)]
        private float followEffectDelay;
        
        [SerializeField]
        [Range(2, 30)]
        private float followEffectDuration = 5;
        
        [SerializeField] 
        private Event explodeEvent;
        
        [SerializeField]
        private CinemachineImpulseSource impulseSource;
        
        [SerializeField]
        [Range(100, 1000)]
        private float damage = 500;
        
        [SerializeField]
        [Range(100, 1000)]
        private float force = 200;

        [SerializeField]
        [Range(5, 30)]
        private float radius = 5;

        [SerializeField]
        private Transform effectAnchor;

        public void TakeDamage(float damage)
        {
            if (hp < 0) return;
            
            hp -= damage;
            if (hp < 0)
            {
                StartCoroutine(DelayExplode(explodeDelay));
            }
        }

        private IEnumerator DelayExplode(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            explodeEvent?.Post(gameObject);
            impulseSource.GenerateImpulse(0.02f);
            
            Destroy(Instantiate(explodeEffect, effectAnchor.position, Quaternion.identity), explodeEffectDuration);
            PhysicsManager.Instance.TakeExplosionDamage(transform.position, radius, force, damage);
            
            if (followEffect)
            {
                StartCoroutine(FollowExplode(followEffectDelay));
            }
        }

        private IEnumerator FollowExplode(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            Destroy(Instantiate(followEffect, effectAnchor.position, Quaternion.identity), followEffectDuration);
        }
    }
}