using System.Collections;
using Cc83.Character;
using Cinemachine;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.Interactable
{
    [BurstCompile]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ShootController : BaseShootController
    {
        private static readonly int TriggerHold = Animator.StringToHash("TriggerHold");
        private static readonly int TriggerShoot = Animator.StringToHash("Shoot");
        private static readonly int TriggerStop = Animator.StringToHash("Stop");            // 停止循环播放的动画（ continuousShoot = true 的连续发射的武器才需要）

        public Animator triggerAnimator;

        public bool continuousShoot;

        public CinemachineImpulseSource impulseSource;

        [Range(0, 1)]
        public float recoil = 0.01f;
        
        public override bool IsEnabled => isActiveAndEnabled && _handController;
        
        private XRGrabInteractable _interactable;

        private HandController _handController;
        
        private uint? _akPlayingId;

        private uint _activeId;

        private Transform _cameraTransform;
        
        protected override void Awake()
        {
            base.Awake();
            
            _interactable = GetComponent<XRGrabInteractable>();
            
            _interactable.activated.AddListener(OnShootActive);
            _interactable.deactivated.AddListener(OnShootDeactivate);
            
            _cameraTransform = Camera.main.transform;
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
            if (impulseSource && recoil > 0)
            {
                float3 weaponForward = transform.forward;
                float3 cameraForward = _cameraTransform.forward;
                if (CalculateRecoilVelocity(recoil, ref weaponForward, ref cameraForward, out var velocity))
                {
                    impulseSource.GenerateImpulseAtPositionWithVelocity(transform.position, velocity);
                }
            }
            
            if (!continuousShoot)
            {
                _handController.Shake();
            }
        }

        private void OnShootActive(ActivateEventArgs args)
        {
            if (_handController) return;            // 防止 MultiGrab 模式物体被意外地重复激活
            
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
                    _handController.Interrupted += InterruptShooting;
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

            if (_handController)
            {
                InterruptShooting();
            }
        }
        
        private void InterruptShooting()
        {
            if (continuousShoot)
            {
                StopShooting();
            }
            
            _handController.Interrupted -= InterruptShooting;
            _handController = null;
            _activeId++;
        }

        private void StopShooting()
        {
            if (_akPlayingId == null) return;                   // 防止重复执行（尤其是 Animator 的 Trigger 重复设置）
            
            AkSoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Break, (uint) _akPlayingId);
            
            _akPlayingId = null;
            _handController.StopShake();
                
            if (Animator)
            {
                Animator.SetTrigger(TriggerStop);
            }
        }
        
        [BurstCompile]
        private static bool CalculateRecoilVelocity(float recoil, ref float3 weaponForward, ref float3 cameraForward, out float3 velocity)
        {
            var dot = math.dot(weaponForward, cameraForward) - 0.5f;        // 左右 60° 角范围内生效
            if (dot > 0)
            {
                velocity = weaponForward * (-math.sqrt(dot) * recoil);
                return true;
            }

            velocity = float3.zero;
            return false;
        }
    }
}
