using UnityEngine;

namespace Cc83.Interactable
{
    public class TrajectoryController : MonoBehaviour
    {
        public float speed = 5;

        private Transform t;

        private void Awake()
        {
            t = transform;
            
            Destroy(gameObject, 0.6f);
        }

        private void Update()
        {
            t.position += t.forward * speed;
        }
    }
}
