using UnityEngine;

namespace Cc83.Interactable
{
    public class FireEffectManager : MonoBehaviour
    {
        public float BulletDistance = 100;
        public GameObject ImpactEffect;
        public ImpactInfo[] ImpactElemets;
    
        public void Shoot()
        {
            var t = transform;
            var position = t.position;
            
            Destroy(Instantiate(ImpactEffect, position, t.rotation), 4);
            
            var ray = new Ray(position, t.forward);
            if (Physics.Raycast(ray, out var hit, BulletDistance))
            {
                var effect = GetImpactEffect(hit.transform.gameObject);
                if (effect)
                {
                    var effectInstance = Instantiate(effect, hit.point, Quaternion.identity);
                    effectInstance.transform.LookAt(hit.point + hit.normal);
                    Destroy(effectInstance, 20);
                }
            }
        }
    
        private GameObject GetImpactEffect(GameObject impactedGameObject)
        {
            var materialType = impactedGameObject.GetComponent<MaterialType>();
            if (materialType)
            {
                foreach (var impactInfo in ImpactElemets)           //TODO Use a K-V data structure
                {
                    if (impactInfo.MaterialType == materialType.TypeOfMaterial)
                    {
                        return impactInfo.ImpactEffect;
                    }
                }
            }
            
            return null;
        }
        
        [System.Serializable]
        public class ImpactInfo
        {
            public MaterialType.MaterialTypeEnum MaterialType;
            
            public GameObject ImpactEffect;
        }
    }
}