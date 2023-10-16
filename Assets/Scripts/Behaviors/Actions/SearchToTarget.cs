using UnityEngine;

namespace Cc83.Behaviors
{
    public class SearchToTarget : MoveToTarget
    {
        protected override float Speed => Random.Range(2.5f, 4f);
    }
}