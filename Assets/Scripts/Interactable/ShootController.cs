using System.Collections;
using Cc83.Character;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Interactable
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ShootController : BaseShootController
    {
        private static readonly int TriggerHold = Animator.StringToHash("TriggerHold");
        private static readonly int TriggerShoot = Animator.StringToHash("Shoot");
        private static readonly int TriggerStop = Animator.StringToHash("Stop");            // 停止循环播放的动画（ continuousShoot = true 的连续发射的武器才需要）

        public Animator triggerAnimator;

        public bool continuousShoot;
        
        public override bool IsEnabled => isActiveAndEnabled && _handController;
        
        private XRGrabInteractable _interactable;

        private HandController _handController;
        
        private uint? _akPlayingId;

        private uint _activeId;

        protected override void Awake()
        {
            base.Awake();
            
            _interactable = GetComponent<XRGrabInteractable>();
            
            _interactable.activated.AddListener(OnShootActive);
            _interactable.deactivated.AddListener(OnShootDeactivate);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _interactable.activated.RemoveListener(OnShootActive);
            _interactable.deactivated.RemoveListener(OnShootDeactivate);
        }

        protected override IEnumerator AsyncShoot(int times)
        {
            if (continuousShoot)
            {
                PendingShoot = true;
                
                var activeId = _activeId;           // 防止 WaitForSeconds 期间扳机连续切换
                while (--times > 0)
                {
                    yield return new WaitForSeconds(cdTime);
                    if (!(activeId == _activeId && IsEnabled)) break;
                    
                    PendingShoot = true;
                }

                if (activeId == _activeId)
                {
                    StopShooting();
                }
            }
            else
            {
                PendingShoot = true;
                _akPlayingId = akEvent?.Post(gameObject);
            }
        }

        protected override void OnShootAnimation()
        {
            if (!continuousShoot)
            {
                _handController.Shake();
            }
        }

        private void OnShootActive(ActivateEventArgs args)
        {
            if (triggerAnimator)
            {
                triggerAnimator.SetBool(TriggerHold, true);
            }
            
            if (AmmunitionQuantity <= 0) return;
            
            _handController = args.interactorObject.transform.GetComponentInParent<HandController>();
            _activeId++;
            
            if (Shoot(continuousShoot ? AmmunitionQuantity : 1))
            {
                _akPlayingId = akEvent?.Post(gameObject);
                
                if (Animator)
                {
                    Animator.SetTrigger(TriggerShoot);
                }
                
                if (continuousShoot)
                {
                    _handController.Shake();
                }
            }
        }

        private void OnShootDeactivate(DeactivateEventArgs args)
        {
            if (triggerAnimator)
            {
                triggerAnimator.SetBool(TriggerHold, false);
            }
            
            if (_handController == null) return;
            
            if (continuousShoot)
            {
                StopShooting();
            }
            
            _handController = null;
            _activeId++;
        }

        private void StopShooting()
        {
            if (_akPlayingId != null)
            {
                AkSoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Break, (uint) _akPlayingId);
                _akPlayingId = null;
            }
                
            if (Animator)
            {
                Animator.SetTrigger(TriggerStop);
            }
            
            _handController.StopShake();
        }
    }
}
