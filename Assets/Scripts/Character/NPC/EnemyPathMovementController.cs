using System.Collections;
using System.Collections.Generic;
using Cc83.Interactable;
using Pathfinding;
using UnityEngine;

namespace Cc83.Character
{
    [RequireComponent(typeof(WeaponReference))]
    public class EnemyPathMovementController : MonoBehaviour
    {
        private static readonly int AnimatorTensity = Animator.StringToHash("Tensity");
        private static readonly int AnimatorSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimatorMoving = Animator.StringToHash("Moving");
        private static readonly int AnimatorCrouching = Animator.StringToHash("Crouching");
        
        public Transform target;
        public Transform lookTarget;

        [Range(0.05f, 1)]
        public float stopDistance = 0.1f;

        [Range(0.5f, 5)]
        public float maxRepeatShootDelay = 5;

        [SerializeField] 
        public GameObject animatorRoot;

        private Seeker _seeker;
        
        private Animator _animator;
        
        private EnemyShootController _shootController;

        private void Awake()
        {
            var weaponReference = GetComponent<WeaponReference>();
            if (weaponReference)
            {
                _shootController = weaponReference.weapon.GetComponent<EnemyShootController>();
            }
            
            _seeker = GetComponent<Seeker>();
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            _seeker.StartPath(transform.position, target.position, OnPathCallback);
            
            // AnimatorUtility.OptimizeTransformHierarchy(animatorRoot, null);
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
            _animator.SetFloat(AnimatorTensity, 1);
            _animator.SetFloat(AnimatorSpeed, 5);
            _animator.SetBool(AnimatorMoving, true);
            
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
            
            _animator.SetBool(AnimatorCrouching, true);
            _animator.SetBool(AnimatorMoving, false);
            _animator.SetFloat(AnimatorSpeed, 0);
            
            //TODO Remove Below Line
#if UNITY_EDITOR
            GetComponent<EnemyWeaponIKController>().aimTowards.position = lookTarget.position + Vector3.up * 1.5f;
#endif

            if (_shootController)
            {
                yield return new WaitForSeconds(Random.Range(0.5f, 1));
                
                while (_shootController.IsEnabled)
                {
                    _shootController.Shoot(Random.Range(1, 6));
                    
                    yield return new WaitForSeconds(Random.Range(0.5f, maxRepeatShootDelay));
                }
            }
        }
    }
}
