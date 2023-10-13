using UnityEngine;

namespace Cc83.Utils
{
    public static class VectorUtils
    {
        /// <summary>
        /// Left(-) or Right(+), 0 means ahead
        /// </summary>
        /// <param name="forward"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static float DotDirection2D(Vector3 forward, Vector3 direction)
        {
            var forward2 = new Vector2(forward.x, forward.z);
            var direction2 = new Vector2(direction.x, direction.z);
            return Vector2.Dot(new Vector2(forward2.y, -forward2.x), direction2);
        }
        
        /// <summary>
        /// [-180, 180], Left(-) or Right(+)
        /// </summary>
        /// <param name="forward"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static float DotDirectionalAngle2D(Vector3 forward, Vector3 direction)
        {
            var forward2 = new Vector2(forward.x, forward.z);
            var direction2 = new Vector2(direction.x, direction.z);
            var angle = Vector2.Angle(forward2, direction2);
            var dotDirection = Vector2.Dot(new Vector2(forward2.y, -forward2.x), direction2);
            return dotDirection < 0 ? -angle : angle;
        }
    }
}