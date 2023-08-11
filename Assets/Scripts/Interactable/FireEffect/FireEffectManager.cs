using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Cc83.Interactable
{
    public class FireEffectManager : MonoBehaviour
    {
        public float BulletDistance = 100;
        public GameObject ImpactEffect;
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

        public void Shoot()
        {
            var t = transform;
            var position = t.position;

            var impactEffect = effectPool.Get();
            impactEffect.transform.SetPositionAndRotation(position, t.rotation);
            StartCoroutine(ReleaseImpactEffect(impactEffect, 4));
            
            var ray = new Ray(position, t.forward);
            if (Physics.Raycast(ray, out var hit, BulletDistance))
            {
                var target = hit.transform;
                var impactInfo = GetImpactEffect(target.gameObject);
                if (impactInfo != null)
                {
                    var effectInstance = Instantiate(impactInfo.ImpactEffect, hit.point, Quaternion.identity);
                    effectInstance.transform.LookAt(hit.point + hit.normal);
                    Destroy(effectInstance, 5);

                    if (impactInfo.DecalEffect)
                    {
                        var effectTransform = effectInstance.transform;
                        Instantiate(impactInfo.DecalEffect, effectTransform.position + effectTransform.forward * -0.01f, effectTransform.rotation, target);
                    }
                }

                if (target.gameObject.isStatic) return;
                
                var targetRigidbody = target.GetComponent<Rigidbody>();
                if (targetRigidbody)
                {
                    targetRigidbody.AddForceAtPosition(t.forward * 100, hit.point, ForceMode.Force);
                }
            }
        }
        
        private ImpactInfo GetImpactEffect(GameObject impactedGameObject)
        {
            var materialType = impactedGameObject.GetComponent<MaterialType>();
            if (materialType && impactInfos.TryGetValue(materialType.TypeOfMaterial, out var impactEffect))
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