using BehaviorDesigner.Runtime.Tasks;
using Pathfinding;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace Cc83.Behaviors
{
    [TaskCategory("Cc83")]
    public class DebugSeekerGizmos : Action
    {
#if UNITY_EDITOR
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public bool DrawGizmos = true;
        
        private Seeker _seeker;
        
        public override void OnAwake()
        {
            _seeker = GetComponent<Seeker>();
        }

        public override void OnStart()
        {
            _seeker.drawGizmos = DrawGizmos;
        }
#endif
        
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}
