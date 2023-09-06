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
        
        public FireEffectManager fireEffectManager;
        
        public Event akEvent;

        public bool IsEnabled => isActiveAndEnabled;
        
        private Animator _animator;
        
        private float _triggerTime;

        private bool _triggerShoot;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void LateUpdate()
        {
            if (_triggerShoot)
            {
                _triggerShoot = false;
                ActShoot();
            }
        }

        public void Shoot(int times = 1)
        {
            var currentTime = Time.time;
            if (currentTime < _triggerTime) return;

            _triggerTime = currentTime + cdTime;
            StartCoroutine(AsyncShoot(times));
        }

        private IEnumerator AsyncShoot(int times)
        {
            while (times > 0 && IsEnabled)
            {
                var loopTimes = Mathf.Min(LoopSwitches.Length, times);
                times -= LoopSwitches.Length;             // 无限循环控制变量必须首先处理，避免循环内异常导致进入死循环
                
                AkSoundEngine.SetSwitch(AK.SWITCHES.WEAPON_FIRE_LOOP.GROUP, LoopSwitches[loopTimes - 1], gameObject);
                var playingId = akEvent?.Post(gameObject);
                
                for (var i = 0; i < loopTimes; i++)
                {
                    if (IsEnabled)
                    {
                        _triggerShoot = true;
                        yield return new WaitForSeconds(cdTime);
                    }
                    else
                    {
                        if (playingId != null)
                        {
                            AkSoundEngine.StopPlayingID((uint)playingId);
                        }
                        break;
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

            if (_animator)
            {
                _animator.SetTrigger(TriggerShoot);
            }
            
            fireEffectManager.Shoot(shootPosition, shootRotation, shootDirection);
        }
    }
}
