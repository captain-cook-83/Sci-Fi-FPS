using Cc83.Interactable;
using UnityEngine;

namespace Cc83.Character
{
    [RequireComponent(typeof(AudioSource))]
    public class EnemyShootController : MonoBehaviour
    {
        private static readonly int TriggerShoot = Animator.StringToHash("Shoot");
        
        [Range(0.01f, 0.5f)]
        public float cdTime = 0.17f;
        
        public Transform shootPoint;
        
        public GameObject trajectoryPrefab;
        
        public FireEffectManager fireEffectManager;
        
        private AudioSource audioSource;
        
        private Animator animator;
        
        private float triggerTime;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
        }

        public void Shoot()
        {
            var currentTime = Time.time;
            if (currentTime < triggerTime)
            {
                return;
            }

            triggerTime = currentTime + cdTime;
            
            // 首先获取位置和旋转等数据，避免接下来的动画逻辑改变相关信息后计算出现偏差
            var shootInfo = fireEffectManager.transform;
            var shootPosition = shootInfo.position;
            var shootRotation = shootInfo.rotation;
            var shootDirection = shootInfo.forward;

            if (animator)
            {
                animator.SetTrigger(TriggerShoot);
            }
            
            audioSource.Play();
            fireEffectManager.Shoot(shootPosition, shootRotation, shootDirection);
            
            Instantiate(trajectoryPrefab, shootPoint.position, shootPoint.rotation);
        }
    }
}
