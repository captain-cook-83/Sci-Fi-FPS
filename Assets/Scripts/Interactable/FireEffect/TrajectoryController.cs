using UnityEngine;

namespace Cc83.Interactable
{
    public class TrajectoryController : MonoBehaviour
    {
        [Range(1, 10)]
        public float speed = 5;

        private Transform _t;

        private void Awake()
        {
            _t = transform;
        }

        private void Update()
        {
            _t.position += _t.forward * speed;
        }
    }
}
