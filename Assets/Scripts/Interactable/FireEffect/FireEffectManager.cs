using System.Collections;
using System.Collections.Generic;
using Cc83.Character;
using UnityEngine;
using UnityEngine.Pool;

namespace Cc83.Interactable
{
    public class FireEffectManager : MonoBehaviour
    {
        public float BulletDistance = 100;
        
        public GameObject ImpactEffect;
        
        [Range(0.1f, 5f)]
        public float effectDuration = 1f;
        
        public ImpactInfo[] ImpactElemets;

        private readonly Dictionary<MaterialType.MaterialTypeEnum, ImpactInfo> impactInfos = new ();
        private ObjectPool<GameObject> effectPool;

        private void Awake()
        {
            foreach (var impactElement in ImpactElemets)
            {
                impactInfos.Add(impactElement.MaterialType, impactElement);
            }
            
            effectPool = new ObjectPool<GameObject>(CreateEffectInstance, 
                go => go.SetActive(true), go => go.SetActive(false), Destroy,
                true, 8, 16);
        }

        public void Shoot(Vector3 position, Quaternion rotation, Vector3 direction)
        {
            var impactEffect = effectPool.Get();
            impactEffect.transform.SetPositionAndRotation(position, rotation);
            StartCoroutine(ReleaseImpactEffect(impactEffect, effectDuration));
            
            var ray = new Ray(position, direction);
            if (Physics.Raycast(ray, out var hit, BulletDistance))
            {
                var target = hit.transform;
                var impactInfo = GetImpactEffect(target.gameObject);
                if (impactInfo != null)
                {
                    var hitPoint = hit.point;
                    var effectInstance = Instantiate(impactInfo.ImpactEffect, hitPoint, Quaternion.identity);
                    effectInstance.transform.LookAt(hitPoint + hit.normal);
                    Destroy(effectInstance, 5);

                    if (impactInfo.DecalEffect)
                    {
                        var effectTransform = effectInstance.transform;
                        var effectForward = effectTransform.forward;
                        Instantiate(impactInfo.DecalEffect, effectTransform.position + effectForward * 0.01f, Quaternion.LookRotation(-effectForward), target);
                    }
                }

                if (target.gameObject.isStatic) return;
                
                var targetRigidbody = target.GetComponent<Rigidbody>();
                if (targetRigidbody)
                {
                    StartCoroutine(AddForceToTarget(targetRigidbody, direction * 100, hit.point));
                }

                if (target.gameObject.layer == Definitions.CharacterLayer.value)
                {
                    var healthListener = target.GetComponent<HealthListener>();
                    if (healthListener)
                    {
                        healthListener.TakeDamage(direction);
                    }
                }
            }
        }
        
        private ImpactInfo GetImpactEffect(GameObject impactedGameObject)
        {
            var materialType = impactedGameObject.GetComponent<MaterialType>();
            if (materialType && impactInfos.TryGetValue(materialType.typeOfMaterial, out var impactEffect))
            {
                return impactEffect;
            }

            return null;
        }
        
        private IEnumerator ReleaseImpactEffect(GameObject effectInstance, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            effectPool.Release(effectInstance);
        }

        private static IEnumerator AddForceToTarget(Rigidbody target, Vector3 force, Vector3 position)
        {
            yield return null;

            target.AddForceAtPosition(force, position, ForceMode.Force);
        }
        
        private GameObject CreateEffectInstance()
        {
            return Instantiate(ImpactEffect);
        }
        
        [System.Serializable]
        public class ImpactInfo
        {
            public MaterialType.MaterialTypeEnum MaterialType;
            
            public GameObject ImpactEffect;

            public GameObject DecalEffect;
        }
    }
}