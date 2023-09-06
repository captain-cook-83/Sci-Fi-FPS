using System.Collections;
using System.Collections.Generic;
using Cc83.Character;
using UnityEngine;
using UnityEngine.Pool;

namespace Cc83.Interactable
{
    public class FireEffectManager : MonoBehaviour
    {
        [Range(50, 300)]
        public float bulletDistance = 100;
        
        public GameObject trajectoryPrefab;

        [Range(0.1f, 1)]
        public float trajectoryDuration = 0.5f;
        
        public GameObject impactEffect;
        
        [Range(0.1f, 5f)]
        public float effectDuration = 1f;
        
        public ImpactInfo[] impactElements;

        private readonly Dictionary<MaterialType.MaterialTypeEnum, ImpactInfo> _impactInfos = new ();
        
        private ObjectPool<GameObject> _effectPool;

        private ObjectPool<GameObject> _trajectoryPool;

        private void Awake()
        {
            foreach (var impactElement in impactElements)
            {
                _impactInfos.Add(impactElement.MaterialType, impactElement);
            }
            
            _effectPool = new ObjectPool<GameObject>(() => Instantiate(impactEffect), 
                go => go.SetActive(true), go => go.SetActive(false), Destroy,
                true, 8, 16);
            
            _trajectoryPool = new ObjectPool<GameObject>(() => Instantiate(trajectoryPrefab), 
                go => go.SetActive(true), go => go.SetActive(false), Destroy,
                true, 2, 4);
        }

        private void OnDestroy()
        {
            _effectPool.Dispose();
            _trajectoryPool.Dispose();
        }

        public void Shoot(Vector3 position, Quaternion rotation, Vector3 direction)
        {
            var effect = _effectPool.Get();
            effect.transform.SetPositionAndRotation(position, rotation);
            StartCoroutine(ReleasePoolElement(effect, _effectPool, effectDuration));

            var trajectory = _trajectoryPool.Get();
            trajectory.transform.SetPositionAndRotation(position, rotation);
            StartCoroutine(ReleasePoolElement(trajectory, _trajectoryPool, trajectoryDuration));
            
            var ray = new Ray(position, direction);
            if (Physics.Raycast(ray, out var hit, bulletDistance))
            {
                var target = hit.transform;
                var targetGameObject = target.gameObject;
                var hitPoint = hit.point;
                var impactInfo = GetImpactEffect(targetGameObject);
                if (impactInfo != null)
                {
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

                if (targetGameObject.isStatic) return;
                
                if (targetGameObject.layer == Definitions.CharacterLayer.value)
                {
                    var healthListener = target.GetComponent<HealthListener>();
                    if (healthListener)
                    {
                        healthListener.TakeDamage(hitPoint, direction);
                    }
                }
                else
                {
                    var targetRigidbody = target.GetComponent<Rigidbody>();
                    if (targetRigidbody)
                    {
                        StartCoroutine(AddForceToTarget(targetRigidbody, direction * 100, hit.point));
                    }
                }
            }
        }
        
        private ImpactInfo GetImpactEffect(GameObject impactedGameObject)
        {
            var materialType = impactedGameObject.GetComponent<MaterialType>();
            if (materialType && _impactInfos.TryGetValue(materialType.typeOfMaterial, out var impactEffect))
            {
                return impactEffect;
            }

            return null;
        }
        
        private static IEnumerator ReleasePoolElement(GameObject element, IObjectPool<GameObject> pool, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            pool.Release(element);
        }

        private static IEnumerator AddForceToTarget(Rigidbody target, Vector3 force, Vector3 position)
        {
            yield return null;

            target.AddForceAtPosition(force, position, ForceMode.Force);
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