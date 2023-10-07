namespace Cc83.Behaviors
{
    public class PatrolToTarget : MoveToTarget
    {
        protected override int Tensity => AnimatorConstants.MaximumTensity;
    }
}