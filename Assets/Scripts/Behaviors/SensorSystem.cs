using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace Cc83.Behaviors
{
    [DisallowMultipleComponent]
    public class SensorSystem : MonoBehaviour
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const bool OperationAdd = true;
        public const bool OperationRemove = false;

        public static SensorSystem Instance { get; private set; }
        
        [SerializeField] 
        [Range(0.1f, 5)]
        private float tickInterval = 1;

        private float _nextTickTime;
        
        #region 基础数据结构
        
        private SensorAgent _player;

        private void Awake()
        {
            if (Instance)
            {
                throw new ConstraintException($"Singleton Exception for multi instance of {nameof(SensorSystem)}");
            }
            
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void RegistryPlayer(SensorAgent player)
        {
            _player = player;

            if (isActiveAndEnabled)
            {
                _coroutine = StartCoroutine(ContinuesCalculating());
            }
        }
        
        public void UnRegistryPlayer()
        {
            _player = null;
            
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
        
        private readonly List<SensorAgent> _enemies = new ();
        private readonly Dictionary<SensorAgent, bool> _tmpEnemies = new ();
        
        public void RegistryEnemy(SensorAgent enemy)
        {
            _tmpEnemies.Add(enemy, OperationAdd);
        }
        
        public void UnRegistryEnemy(SensorAgent enemy)
        {
            if (!_tmpEnemies.Remove(enemy))
            {
                _tmpEnemies.Add(enemy, OperationRemove);
            }
        }
        
        #endregion

        private Coroutine _coroutine;

        private void OnEnable()
        {
            if (_player)
            {
                _coroutine = StartCoroutine(ContinuesCalculating());
            }
        }

        private void OnDisable()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        private IEnumerator ContinuesCalculating()
        {
            while (isActiveAndEnabled)
            {
                #region 控制周期间隔

                var waitingTime = _nextTickTime - Time.time;
                if (waitingTime > 0)
                {
                    yield return new WaitForSeconds(waitingTime);
                }

                _nextTickTime = Time.time + tickInterval;

                #endregion
                
                var playerTransform = _player.transform;
                var playerPosition = playerTransform.position;

                foreach (var enemy in _enemies)
                {
                    enemy.Clear();
                
                    var enemyPosition = enemy.transform.position;
                    var direction = playerPosition - enemyPosition;
                    var sqrDistance = direction.sqrMagnitude;
                    if (sqrDistance > enemy.sqrDistance)
                    {
                        continue;
                    }

                    var enemyForward = enemy.LookForward;
                    var angle = Vector3.Angle(direction, enemyForward);
                    var insideView = angle <= enemy.halfFov;
                    if (insideView)
                    {
                        enemy.NotifyEnemy(_player, direction, sqrDistance, angle);
                    }
                    
                    if (enemy is not TeammateSensitiveSensorAgent teammateSensitiveEnemy || !(teammateSensitiveEnemy.forceTeammateSensitive || insideView)) continue;

                    foreach (var otherEnemy in _enemies.Where(e => !ReferenceEquals(e, enemy)))
                    {
                        yield return null;
                        
                        var otherDirection = otherEnemy.transform.position - enemyPosition;
                        var otherSqrDistance = otherDirection.sqrMagnitude;
                        if (otherSqrDistance > enemy.sqrDistance)
                        {
                            Debug.LogWarning("Out of range");
                            continue;
                        }
                    
                        var otherAngle = Vector3.Angle(otherDirection, enemyForward);
                        if (otherAngle <= enemy.halfFov)
                        {
                            teammateSensitiveEnemy.NotifyTeammate(otherEnemy, otherDirection, otherSqrDistance, otherAngle);
                        }
                    }
                    
                    yield return null;
                }

                #region 同步处理动态增减的数据

                for (var i = _enemies.Count - 1; i >= 0; --i)
                {
                    var enemy = _enemies[i];
                    if (_tmpEnemies.TryGetValue(enemy, out var operation) && operation == OperationRemove)
                    {
                        enemy.Stop();
                        _enemies.RemoveAt(i);
                    }
                    else if (enemy is TeammateSensitiveSensorAgent teammateSensitiveEnemy)
                    {
                        teammateSensitiveEnemy.RemoveTeammates(_tmpEnemies);
                    }
                }
            
                foreach (var kv in _tmpEnemies)
                {
                    if (kv.Value == OperationAdd)
                    {
                        _enemies.Add(kv.Key);
                    }
                }
            
                _tmpEnemies.Clear();

                #endregion
                
                foreach (var enemy in _enemies)
                {
                    yield return null;
                    
                    enemy.Submit();
                }
            }
        }
    }
}
