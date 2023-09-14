using System.Collections.Generic;
using Cc83.Interactable;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Cc83.Character
{
    [BurstCompile]
    public class PhysicsManager : MonoBehaviour
    {
        public static PhysicsManager Instance { get; private set; }

        public LayerMask hitLayers;

        public LayerMask blockExplosionLayers;

        [SerializeField] 
        private HealthController playerHealthController;

        [SerializeField] 
        private Transform enemies;

        [SerializeField]
        [Range(8, 64)]
        private int batchForDynamics = 16;

        [SerializeField] 
        private Transform[] overlapSphereDetectContainers;              // 临时方案，规避 Physics.OverlapSphereNonAlloc 调用时 layerMask 参数无效的问题

        private HealthController[] _enemyHealthControllers;
        
        private NativeArray<float4> _playerDetections;

        private NativeArray<float4> _enemyDetections;
        
        private NativeArray<float4> _dynamicDetections;

        private Collider[] _dynamicColliders;

        public void TakeExplosionDamage(Vector3 position, float radius, float force, float damage)
        {
            position += Vector3.up * 0.01f;     // 向上移动 1cm，避免贴表面检测
            
            float3 origin = position;

            #region 处理玩家伤害

            var lethalParts = playerHealthController.lethalParts;
            for (var i = 0; i < lethalParts.Length; i++)
            {
                _playerDetections[i] = new float4(lethalParts[i].transform.position, 0);
            }

            CalculateDirections(ref origin, ref _playerDetections, _playerDetections.Length, _playerDetections.Length, radius);
            for (var i = 0; i < _playerDetections.Length; i++)
            {
                var direction = (Vector3) _playerDetections[i].xyz;
                var distance = _playerDetections[i].w;
                if (distance > radius) break;
                
                if (!Physics.Raycast(position, direction, distance, blockExplosionLayers.value))
                {
                    var lethalPart = playerHealthController.lethalParts[i];
                    lethalPart.TakeDamage(lethalPart.transform.position, direction, damage);
                    break;
                }
            }

            #endregion
            
            #region 处理 NPC 伤害
            
            var index = 0;
            foreach (var hController in _enemyHealthControllers)
            {
                foreach (var lethalPart in hController.lethalParts)
                {
                    _enemyDetections[index++] = new float4(lethalPart.transform.position, 0);
                }
            }

            var unitSize = index / _enemyHealthControllers.Length;
            CalculateDirections(ref origin, ref _enemyDetections, unitSize, index, radius);
            for (var i = 0; i < index; i++)
            {
                var unitEnd = i + unitSize;
                for (; i < unitEnd; i++)
                {
                    var direction = (Vector3) _enemyDetections[i].xyz;
                    var distance = _enemyDetections[i].w;
                    if (distance > radius)
                    {
                        i = unitEnd;
                        break;
                    } 
                    
                    if (!Physics.Raycast(position, direction, distance, blockExplosionLayers.value))
                    {
                        var lethalPart = _enemyHealthControllers[i / unitSize].lethalParts[i % unitSize];
                        lethalPart.TakeDamage(lethalPart.transform.position, direction, damage);
                        i = unitEnd;
                        break;
                    }
                }

                i--;
            }
            
            #endregion
            
            #region 处理动态物体
            
            var hits = Physics.OverlapSphereNonAlloc(position, radius, _dynamicColliders, hitLayers);
            // var hits = OverlapSphereNonAlloc(position, radius, _dynamicColliders);
            for (var i = 0; i < hits; i++)
            {
                _dynamicDetections[i] = new float4(_dynamicColliders[i].transform.position, 0);
            }
            
            CalculateDirections(ref origin, ref _dynamicDetections, 1, hits, radius);
            for (var i = 0; i < hits; i++)
            {
                var direction = (Vector3) _dynamicDetections[i].xyz;
                var distance = _dynamicDetections[i].w;
                if (_dynamicColliders[i].TryGetComponent<Rigidbody>(out var rb))
                {
                    if (!Physics.Raycast(position, direction, distance, blockExplosionLayers.value))
                    {
                        rb.AddExplosionForce(force, position, radius);
                        if (rb.TryGetComponent<ExplosionController>(out var controller))
                        {
                            controller.TakeDamage(damage);
                        }
                    }
                }
            }
            
            #endregion
        }

        private void Awake()
        {
            Instance = this;

            hitLayers &= ~(1 << Definitions.CharacterLayer);

            var totalLethalPartsLength = 0;
            _enemyHealthControllers = new HealthController[enemies.childCount];
            for (var i = 0; i < _enemyHealthControllers.Length; i++)
            {
                var enemy = enemies.GetChild(i);
                var healthController = enemy.GetComponent<HealthController>();
                var lethalPartsLength = healthController.lethalParts.Length;
                
                if (totalLethalPartsLength % lethalPartsLength != 0)
                {
                    Debug.LogError($"the length of lethalParts {lethalPartsLength} from Enemy[{enemy.name}] must be equals with the others.");
                }
                else
                {
                    totalLethalPartsLength += lethalPartsLength;
                }
                
                _enemyHealthControllers[i] = healthController;
            }

            _playerDetections = new NativeArray<float4>(playerHealthController.lethalParts.Length, Allocator.Persistent);
            _enemyDetections = new NativeArray<float4>(totalLethalPartsLength, Allocator.Persistent);
            _dynamicDetections = new NativeArray<float4>(batchForDynamics, Allocator.Persistent);
            _dynamicColliders = new Collider[batchForDynamics];
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private int OverlapSphereNonAlloc(Vector3 position, float radius, IList<Collider> results)
        {
            var sqrRadius = radius * radius;
            var index = 0;
            foreach (var container in overlapSphereDetectContainers)
            {
                for (var i = 0; i < container.childCount; i++)
                {
                    var child = container.GetChild(i);
                    if ((child.position - position).sqrMagnitude < sqrRadius)
                    {
                        results[index++] = child.GetComponent<Collider>();
                    }
                }
            }

            return index;
        }

        [BurstCompile]
        private static void CalculateDirections(ref float3 origin, ref NativeArray<float4> inOut, int unitSize, int size, float maxDistance)
        {
            for (var i = 0; i < size; i++)
            {
                var direction = inOut[i].xyz - origin;
                var distance = math.length(direction);
                if (distance > maxDistance)
                {
                    var zeroEnd = math.min(i - i % unitSize + unitSize, size);
                    for (; i < zeroEnd; i++)
                    {
                        inOut[i] = new float4(direction, distance);
                    }

                    i--;
                }
                else
                {
                    inOut[i] = new float4(direction, distance);
                }
            }
        }
    }
}