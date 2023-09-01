using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace Cc83.Character
{
    [RequireComponent(typeof(WeaponReference))]
    public class AIPathMovementController : MonoBehaviour
    {
        private static readonly int AnimatorTensity = Animator.StringToHash("Tensity");
        private static readonly int AnimatorSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimatorMoving = Animator.StringToHash("Moving");
        private static readonly int AnimatorCrouching = Animator.StringToHash("Crouching");
        
        public Transform target;
        public Transform lookTarget;

        [Range(0.05f, 1)]
        public float stopDistance = 0.1f;

        [Range(1, 10)]
        public float shootFrequency = 5;

        private Seeker seeker;
        
        private Animator animator;
        
        private EnemyShootController shootController;

        private void Awake()
        {
            var weaponReference = GetComponent<WeaponReference>();
            if (weaponReference)
            {
                shootController = weaponReference.weapon.GetComponent<EnemyShootController>();
            }
            
            seeker = GetComponent<Seeker>();
            animator = GetComponent<Animator>();
            
            seeker.pathCallback += OnPathCallback;
        }

        private void Start()
        {
            seeker.StartPath(transform.position, target.position);
        }

        private void OnDestroy()
        {
            seeker.pathCallback -= OnPathCallback;
        }

        private void OnPathCallback(Path path)
        {
            if (path.error)
            {
                Debug.LogError($"Pathfinding error: {path.errorLog}");
                return;
            }

            var pathPoints = path.vectorPath;
            if (pathPoints.Count > 0)
            {
                StartCoroutine(MoveToTarget(pathPoints));
            }
        }

        private IEnumerator MoveToTarget(List<Vector3> pathPoints)
        {
            animator.SetFloat(AnimatorTensity, 1);
            animator.SetFloat(AnimatorSpeed, 5);
            animator.SetBool(AnimatorMoving, true);
            
            foreach (var pathPoint in pathPoints)
            {
                var position = transform.position;
                var targetPoint = new Vector3
                {
                    x = pathPoint.x,
                    y = position.y,
                    z = pathPoint.z
                };

                transform.LookAt(targetPoint);
                
                var direction = targetPoint - position;
                var targetProjection = 0f;
                do
                {
                    yield return null;
                    targetProjection = Vector3.Dot(targetPoint - transform.position, direction);
                } while (targetProjection > stopDistance);
            }

            var aimTarget = lookTarget.position;
            aimTarget.y = transform.position.y;
            transform.LookAt(aimTarget);
            
            animator.SetBool(AnimatorCrouching, true);
            animator.SetBool(AnimatorMoving, false);
            animator.SetFloat(AnimatorSpeed, 0);

            if (shootController)
            {
                while (true)
                {
                    yield return new WaitForSeconds(shootController.cdTime * Random.Range(1, shootFrequency));
                    shootController.Shoot();
                }
            }
        }
    }
}
