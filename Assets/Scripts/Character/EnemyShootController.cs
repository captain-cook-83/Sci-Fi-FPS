using System.Collections;
using Cc83.Interactable;
using UnityEngine;

namespace Cc83.Character
{
    public class EnemyShootController : MonoBehaviour
    {
        private static readonly int TriggerShoot = Animator.StringToHash("Shoot");
        
        [Range(0.01f, 0.5f)]
        public float cdTime = 0.17f;
        
        public Transform shootPoint;
        
        public GameObject trajectoryPrefab;
        
        public FireEffectManager fireEffectManager;
        
        private Animator animator;

        private AkAmbient akAmbient;
        
        private float triggerTime;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            akAmbient = GetComponent<AkAmbient>();
        }

        public void Shoot(int times = 1)
        {
            var currentTime = Time.time;
            if (currentTime < triggerTime)
            {
                return;
            }

            triggerTime = currentTime + cdTime;

            if (times == 1)
            {
                ActShoot();
            }
            else
            {
                StartCoroutine(AsyncShoot(times));
            }
        }

        private IEnumerator AsyncShoot(int times)
        {
            for (var i = 0; i < times && enabled; i++)
            {
                ActShoot();
                
                yield return new WaitForSeconds(cdTime);
            }
        }
        
        private void ActShoot()
        {
            // 首先获取位置和旋转等数据，避免接下来的动画逻辑改变相关信息后计算出现偏差
            var shootInfo = fireEffectManager.transform;
            var shootPosition = shootInfo.position;
            var shootRotation = shootInfo.rotation;
            var shootDirection = shootInfo.forward;

            if (animator)
            {
                animator.SetTrigger(TriggerShoot);
            }

            akAmbient.data?.Post(gameObject);
            fireEffectManager.Shoot(shootPosition, shootRotation, shootDirection);
            
            Instantiate(trajectoryPrefab, shootPoint.position, shootPoint.rotation);
        }
    }
}
