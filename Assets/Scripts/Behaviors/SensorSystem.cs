using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cc83.Behaviors
{
    [DisallowMultipleComponent]
    public class SensorSystem : MonoBehaviour
    {
        [SerializeField] 
        [Range(0.1f, 5)]
        private float tickInterval = 1;
        
        #region 基础数据结构
        
        private SensorAgent _player;
        
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
            _tmpEnemies.Add(enemy, true);
        }
        
        public void UnRegistryEnemy(SensorAgent enemy)
        {
            if (!_tmpEnemies.Remove(enemy))
            {
                _tmpEnemies.Add(enemy, false);
            }
        }
        
        // private readonly List<SensorAgent> _auxiliaries = new ();
        // private readonly Dictionary<SensorAgent, bool> _tmpAuxiliaries = new ();
        //
        // public void RegistryAuxiliary(SensorAgent auxiliary)
        // {
        //     _tmpAuxiliaries.Add(auxiliary, true);
        // }
        //
        // public void UnRegistryAuxiliary(SensorAgent auxiliary)
        // {
        //     if (!_tmpAuxiliaries.Remove(auxiliary))
        //     {
        //         _tmpAuxiliaries.Add(auxiliary, false);
        //     }
        // }
        
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
            yield return null;

            while (isActiveAndEnabled)
            {
                var playerTransform = _player.transform;
                var playerPosition = playerTransform.position;

                foreach (var enemy in _enemies)
                {
                    enemy.Clear();
                
                    var enemyTransform = enemy.transform;
                    var enemyPosition = enemyTransform.position;
                    var direction = playerPosition - enemyPosition;
                    var sqrDistance = direction.sqrMagnitude;
                    if (sqrDistance > enemy.sqrDistance)
                    {
                        continue;
                    }
                    
                    var angle = Vector2.Angle(direction, enemyTransform.forward);
                    if (angle > enemy.halfFov)
                    {
                        continue;
                    }
                    
                    enemy.NotifyEnemy(_player, direction, sqrDistance, angle);
                    
                    foreach (var otherEnemy in _enemies)
                    {
                        if (ReferenceEquals(enemy, otherEnemy)) continue;
                        
                        yield return null;
                        
                        var otherTransform = otherEnemy.transform;
                        var otherPosition = otherTransform.position;
                        var otherDirection = otherPosition - enemyPosition;
                        var otherSqrDistance = otherDirection.sqrMagnitude;
                        if (otherSqrDistance > enemy.sqrDistance)
                        {
                            continue;
                        }
                    
                        var otherAngle = Vector2.Angle(otherDirection, otherTransform.forward);
                        if (otherAngle < enemy.halfFov)
                        {
                            enemy.NotifyTeammate(otherEnemy, otherDirection, otherSqrDistance, otherAngle);
                        }
                    }
                
                    yield return null;
                }

                #region 同步处理动态增减的数据

                for (var i = _enemies.Count - 1; i >= 0; --i)
                {
                    var enemy = _enemies[i];
                    if (_tmpEnemies.TryGetValue(enemy, out var operation) && !operation)
                    {
                        enemy.Destroy();
                        _enemies.RemoveAt(i);
                    }
                    else
                    {
                        enemy.RemoveTeammates(_tmpEnemies);
                    }
                }
            
                foreach (var kv in _tmpEnemies)
                {
                    if (kv.Value)
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
