using System.Collections;
using System.Collections.Generic;
using RailGame.Enemy;
using UnityEngine;

namespace Railgame.Enemy
{
    /// <summary>
    /// 맵의 EnemySpawnMarker를 이용해 플레이어 작업을 방해할 적을 소규모로 투입한다.
    /// 기차의 체력이나 파괴 조건에는 관여하지 않으며, 프리팹과 시작 여부는 씬에서 선택적으로 설정한다.
    /// </summary>
    public sealed class RailgameEnemyObstructionDirector : MonoBehaviour
    {
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private Transform playerTarget;
        [SerializeField] private bool beginOnStart;
        [SerializeField, Min(0f)] private float initialDelay = 3f;
        [SerializeField, Min(0.1f)] private float spawnInterval = 8f;
        [SerializeField, Min(1)] private int maxAlive = 6;

        private readonly List<GameObject> aliveEnemies = new();
        private EnemySpawnMarker[] markers;
        private Coroutine spawnRoutine;

        public int AliveCount
        {
            get
            {
                RemoveDestroyedEnemies();
                return aliveEnemies.Count;
            }
        }

        public void Initialize(GameObject[] prefabs, EnemySpawnMarker[] spawnMarkers,
            Transform target = null, bool startImmediately = false)
        {
            enemyPrefabs = prefabs;
            markers = spawnMarkers;
            playerTarget = target;
            if (startImmediately)
                Begin();
        }

        private void Start()
        {
            EnsureSceneReferences();
            if (beginOnStart)
                Begin();
        }

        public void Begin()
        {
            if (spawnRoutine != null)
                return;

            EnsureSceneReferences();
            if (!HasSpawnConfiguration())
            {
                Debug.LogWarning("[EnemyObstructionDirector] 적 프리팹 또는 스폰 마커가 없어 대기합니다.", this);
                return;
            }

            spawnRoutine = StartCoroutine(SpawnLoop());
        }

        public void StopSpawning()
        {
            if (spawnRoutine == null)
                return;

            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        public GameObject SpawnOnce()
        {
            EnsureSceneReferences();
            RemoveDestroyedEnemies();
            if (!HasSpawnConfiguration() || aliveEnemies.Count >= maxAlive)
                return null;

            EnemySpawnMarker marker = markers[Random.Range(0, markers.Length)];
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            if (marker == null || marker.SpawnPoint == null || prefab == null)
                return null;

            Transform point = marker.SpawnPoint;
            GameObject instance = Instantiate(prefab, point.position, point.rotation);
            aliveEnemies.Add(instance);

            RailGame.Enemy.Enemy enemy = instance.GetComponentInChildren<RailGame.Enemy.Enemy>();
            if (enemy != null && playerTarget != null)
                enemy.SetTarget(playerTarget);

            return instance;
        }

        private IEnumerator SpawnLoop()
        {
            if (initialDelay > 0f)
                yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                SpawnOnce();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void EnsureSceneReferences()
        {
            if (markers == null || markers.Length == 0)
                markers = FindObjectsByType<EnemySpawnMarker>(FindObjectsSortMode.None);

            if (playerTarget == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTarget = player.transform;
            }
        }

        private bool HasSpawnConfiguration()
        {
            return enemyPrefabs != null && enemyPrefabs.Length > 0 && markers != null && markers.Length > 0;
        }

        private void RemoveDestroyedEnemies()
        {
            aliveEnemies.RemoveAll(enemy => enemy == null);
        }

        private void OnDisable()
        {
            StopSpawning();
        }
    }
}
