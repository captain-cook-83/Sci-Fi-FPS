namespace Cc83.Behaviors
{
    public class WalkToTarget : MoveToTarget
    {
        public override void OnConditionalAbort()
        {
            base.OnConditionalAbort();
            
            Animator.SetTrigger(AnimatorConstants.AnimatorStop);            // 避免进入 Walk To Stop 动画状态下
            Animator.SetBool(AnimatorConstants.AnimatorMoving, false);
        }
    }
}
