using System.Collections;
using Cc83.Interactable;
using UnityEngine;
using Event = AK.Wwise.Event;

namespace Cc83.Character
{
    public class EnemyShootController : MonoBehaviour
    {
        private static readonly int TriggerShoot = Animator.StringToHash("Shoot");
        
        private static readonly uint[] LoopSwitches =
        {
            AK.SWITCHES.WEAPON_FIRE_LOOP.SWITCH.LOOP_1,
            AK.SWITCHES.WEAPON_FIRE_LOOP.SWITCH.LOOP_2,
            AK.SWITCHES.WEAPON_FIRE_LOOP.SWITCH.LOOP_3,
            AK.SWITCHES.WEAPON_FIRE_LOOP.SWITCH.LOOP_4,
            AK.SWITCHES.WEAPON_FIRE_LOOP.SWITCH.LOOP_5
        };
        
        [Range(0.01f, 0.5f)]
        public float cdTime = 0.17f;
        
        public Transform shootPoint;
        
        public GameObject trajectoryPrefab;
        
        public FireEffectManager fireEffectManager;
        
        public Event akEvent;
        
        private Animator animator;
        
        private float triggerTime;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void Shoot(int times = 1)
        {
            var currentTime = Time.time;
            if (currentTime < triggerTime)
            {
                return;
            }

            triggerTime = currentTime + cdTime;

            StartCoroutine(AsyncShoot(times));
        }

        private IEnumerator AsyncShoot(int times)
        {
            while (times > 0 && enabled)
            {
                var loopTimes = Mathf.Min(LoopSwitches.Length, times);
                times -= LoopSwitches.Length;             // 无限循环控制变量必须首先处理，避免循环内异常导致进入死循环
                
                AkSoundEngine.SetSwitch(AK.SWITCHES.WEAPON_FIRE_LOOP.GROUP, LoopSwitches[loopTimes - 1], gameObject);
                var playingId = akEvent?.Post(gameObject);
                
                for (var i = 0; i < loopTimes; i++)
                {
                    if (enabled)
                    {
                        ActShoot();
                
                        yield return new WaitForSeconds(cdTime);
                    }
                    else if (playingId != null)
                    {
                        AkSoundEngine.StopPlayingID((uint)playingId);
                    }
                }
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
            
            fireEffectManager.Shoot(shootPosition, shootRotation, shootDirection);
            
            Instantiate(trajectoryPrefab, shootPoint.position, shootPoint.rotation);
        }
    }
}
