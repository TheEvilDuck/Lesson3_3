using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Visitor
{
    public class Spawner: MonoBehaviour, IEnemyDeathNotifier
    {
        public event Action<Enemy> Notified;

        [SerializeField] private float _spawnCooldown;
        [SerializeField] private List<Transform> _spawnPoints;
        [SerializeField] private EnemyFactory _enemyFactory;

        private List<Enemy> _spawnedEnemies = new List<Enemy>();

        private Coroutine _spawn;
        private WeightCalculator _weightCalculator;

        public void StartWork()
        {
            StopWork();

            _spawn = StartCoroutine(Spawn());
        }

        public void StopWork()
        {
            if (_spawn != null)
                StopCoroutine(_spawn);
        }

        public void KillRandomEnemy()
        {
            if (_spawnedEnemies.Count == 0)
                return;

            _spawnedEnemies[UnityEngine.Random.Range(0, _spawnedEnemies.Count)].Kill();
        }

        public void Init(SpawnerWeightConfig spawnerWeightConfig)
        {
            _weightCalculator = new WeightCalculator(spawnerWeightConfig);
        }
        private IEnumerator Spawn()
        {
            while (true)
            {
                if (!_weightCalculator.IsFull)
                {
                    Enemy enemy = _enemyFactory.Get((EnemyType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(EnemyType)).Length));
                    enemy.MoveTo(_spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Count)].position);
                    enemy.Died += OnEnemyDied;
                    _spawnedEnemies.Add(enemy);
                    _weightCalculator.Visit(enemy);
                }
                yield return new WaitForSeconds(_spawnCooldown);
            }
        }

        private void OnEnemyDied(Enemy enemy)
        {
            Notified?.Invoke(enemy);
            enemy.Died -= OnEnemyDied;
            _spawnedEnemies.Remove(enemy);
        }

        private class WeightCalculator : IEnemyVisitor
        {
            private readonly SpawnerWeightConfig _spawnerWeightConfig;
            private int _currentWeight;

            public bool IsFull => _currentWeight>=_spawnerWeightConfig.MaxWeight;

            public WeightCalculator(SpawnerWeightConfig spawnerWeightConfig)
            {
                _spawnerWeightConfig = spawnerWeightConfig;
                _currentWeight = 0;
            }
            public void Visit(Ork ork)
            {
                _currentWeight+=_spawnerWeightConfig.OrcWeight;

                DecreaseOnDeath(ork,_spawnerWeightConfig.OrcWeight);
            }
            public void Visit(Human human)
            {
                _currentWeight+=_spawnerWeightConfig.HumanWeight;

               DecreaseOnDeath(human,_spawnerWeightConfig.HumanWeight);
            }
            public void Visit(Elf elf)
            {
                _currentWeight+=_spawnerWeightConfig.ElfWeight;

                DecreaseOnDeath(elf,_spawnerWeightConfig.ElfWeight);
            }
            public void Visit(Enemy enemy)
            {
                Visit((dynamic)enemy);
                Debug.Log($"Текущий вес мобов в спаунере: {_currentWeight}");
            }

            private void DecreaseOnDeath(Enemy enemy, int amount)
            {
                Action<Enemy> OnDied = null;
                OnDied = (Enemy enemy) =>
                {
                    _currentWeight-=amount;
                    enemy.Died-=OnDied;
                    OnDied = null;
                };
                enemy.Died+=OnDied;
            }
        }
    }
}
