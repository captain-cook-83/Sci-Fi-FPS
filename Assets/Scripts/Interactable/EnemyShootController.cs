using System.Collections;
using UnityEngine;

namespace Cc83.Interactable
{
    public class EnemyShootController : BaseShootController
    {
        private static readonly uint[] LoopSwitches =
        {
            AK.SWITCHES.WEAPON_FIRE_LOOP.SWITCH.LOOP_1,
            AK.SWITCHES.WEAPON_FIRE_LOOP.SWITCH.LOOP_2,
            AK.SWITCHES.WEAPON_FIRE_LOOP.SWITCH.LOOP_3,
            AK.SWITCHES.WEAPON_FIRE_LOOP.SWITCH.LOOP_4,
            AK.SWITCHES.WEAPON_FIRE_LOOP.SWITCH.LOOP_5
        };
        
        private uint? _akPlayingId;
        
        protected override IEnumerator AsyncShoot(int times)
        {
            while (times > 0 && IsEnabled)
            {
                var loopTimes = Mathf.Min(LoopSwitches.Length, times);
                times -= LoopSwitches.Length;             // 无限循环控制变量必须首先处理，避免循环内异常导致进入死循环
                
                AkSoundEngine.SetSwitch(AK.SWITCHES.WEAPON_FIRE_LOOP.GROUP, LoopSwitches[loopTimes - 1], gameObject);
                _akPlayingId = akEvent?.Post(gameObject);
                
                for (var i = 0; i < loopTimes; i++)
                {
                    if (IsEnabled)
                    {
                        PendingShoot = true;
                        yield return new WaitForSeconds(cdTime);
                    }
                    else
                    {
                        if (_akPlayingId != null)
                        {
                            AkSoundEngine.StopPlayingID((uint)_akPlayingId);
                        }
                        break;
                    }
                }
            }

            _akPlayingId = null;
        }
    }
}
