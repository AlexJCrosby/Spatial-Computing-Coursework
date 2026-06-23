using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Spawn Spread")]
    [SerializeField] private float spawnRadius = 3f;

    [Header("Enemy Setup")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Wave Settings")]
    [SerializeField] private float timeBetweenWaves = 3f;
    [SerializeField] private int startingEnemyCount = 1;
    [SerializeField] private int enemyIncreasePerWave = 4;

    public int CurrentWave { get; private set; }
    public int EnemiesRemaining => activeEnemies.Count;

    private readonly List<GameObject> activeEnemies = new();
    private bool waveInProgress;
    private bool waitingForNextWave;

    private void Start()
    {
        StartNextWave();
    }

    private void Update()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);

        if (waveInProgress && activeEnemies.Count == 0 && !waitingForNextWave)
        {
            waveInProgress = false;
            waitingForNextWave = true;

            Invoke(nameof(StartNextWave), timeBetweenWaves);
        }
    }

    private void StartNextWave()
    {
        waitingForNextWave = false;
        waveInProgress = true;

        CurrentWave++;

        int enemiesToSpawn =
            startingEnemyCount + ((CurrentWave - 1) * enemyIncreasePerWave);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
        }

        Debug.Log("Wave " + CurrentWave + " started with " + enemiesToSpawn + " enemies.");
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("WaveManager needs an enemy prefab and spawn points.");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

        Vector3 spawnPosition = spawnPoint.position + new Vector3(
            randomCircle.x,
            50f,
            randomCircle.y
        );

        if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 200f))
        {
            spawnPosition = hit.point;
        }
        else
        {
            Debug.LogWarning("No ground found below spawn point.");
        }

        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPosition,
            spawnPoint.rotation
        );

        activeEnemies.Add(enemy);
    }
}