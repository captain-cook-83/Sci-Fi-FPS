using UnityEngine;

namespace Cc83.Interactable
{
    public class TrajectoryController : MonoBehaviour
    {
        public float speed = 4;

        private Transform t;

        private void Awake()
        {
            t = transform;
            
            Destroy(gameObject, 1);
        }

        private void Update()
        {
            t.position += t.forward * speed;
        }
    }
}
