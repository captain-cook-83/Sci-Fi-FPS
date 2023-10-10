using BehaviorDesigner.Runtime.Tasks;
using Cc83.Character;
using UnityEngine;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    [TaskIcon("{SkinColor}WaitIcon.png")]
    public class WaitingByTensity : Action
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float MinTime = 0.5f;
        
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float MaxTime = 1;
        
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public float TimeGap = 1;
        
        private AnimatorStateController _animatorStateController;

        private float _endTime;
        
        public override void OnAwake()
        {
            _animatorStateController = GetComponent<AnimatorStateController>();
        }

        public override void OnStart()
        {
            _endTime = Time.time + Random.Range(Mathf.Min(MinTime, MaxTime), Mathf.Max(MinTime, MaxTime));
            
            if (_animatorStateController.Tensity < AnimatorConstants.WalkTensity)
            {
                _endTime += TimeGap;
            }
        }

        public override TaskStatus OnUpdate()
        {
            return Time.time < _endTime ? TaskStatus.Running : TaskStatus.Success;
        }
    }
}