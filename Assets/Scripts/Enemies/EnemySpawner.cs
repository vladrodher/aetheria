using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawner de enemigos por oleadas. Mantiene un pool por cada tipo de enemigo,
/// spawn en posiciones aleatorias alrededor del jugador, y escala la dificultad
/// incrementando la cantidad por oleada.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;

    [Header("Tipos de enemigos")]
    [SerializeField] private EnemyWaveEntry[] enemyTypes;

    [Header("Spawning")]
    [SerializeField] private float spawnRadius = 12f;
    [SerializeField] private float minSpawnDistance = 8f;
    [SerializeField] private int maxActiveEnemies = 30;
    [SerializeField] private int poolSizePerType = 15;

    [Header("Oleadas")]
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private int enemiesPerWaveIncrease = 2;
    [SerializeField] private float spawnInterval = 0.3f;

    private readonly Dictionary<EnemyData, Queue<EnemyController>> pools = new();
    private readonly List<EnemyController> activeEnemies = new();

    private int currentWave;
    private int enemiesSpawnedThisWave;
    private int enemiesToSpawnThisWave;
    private float nextSpawnTime;
    private float waveStartTime;
    private bool waveInProgress;

    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCompleted;

    public int CurrentWave => currentWave;
    public int ActiveEnemyCount => activeEnemies.Count;

    // ── Inicialización ───────────────────────────────────────────────

    private void Start()
    {
        InitializePools();
        StartNextWave();
    }

    private void InitializePools()
    {
        foreach (var entry in enemyTypes)
        {
            if (entry.Data == null || entry.Prefab == null) continue;

            var queue = new Queue<EnemyController>(poolSizePerType);
            for (int i = 0; i < poolSizePerType; i++)
                queue.Enqueue(CreateEnemy(entry));

            pools[entry.Data] = queue;
        }
    }

    private EnemyController CreateEnemy(EnemyWaveEntry entry)
    {
        var enemy = Instantiate(entry.Prefab, transform);
        enemy.gameObject.SetActive(false);
        return enemy;
    }

    // ── Update loop ──────────────────────────────────────────────────

    private void Update()
    {
        CleanupDeadEnemies();

        if (waveInProgress)
            HandleWaveSpawning();
        else if (activeEnemies.Count == 0 && Time.time >= waveStartTime + timeBetweenWaves)
            StartNextWave();
    }

    private void HandleWaveSpawning()
    {
        if (enemiesSpawnedThisWave >= enemiesToSpawnThisWave)
        {
            waveInProgress = false;
            return;
        }

        if (activeEnemies.Count >= maxActiveEnemies) return;
        if (Time.time < nextSpawnTime) return;

        SpawnNextEnemy();
        nextSpawnTime = Time.time + spawnInterval;
    }

    // ── Oleadas ──────────────────────────────────────────────────────

    private void StartNextWave()
    {
        currentWave++;
        enemiesSpawnedThisWave = 0;
        enemiesToSpawnThisWave = baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease;
        waveInProgress = true;
        waveStartTime = Time.time;

        OnWaveStarted?.Invoke(currentWave);
    }

    private void SpawnNextEnemy()
    {
        var entry = PickRandomEntry();
        if (entry.Data == null) return;

        if (!pools.TryGetValue(entry.Data, out var queue))
            return;

        EnemyController enemy = queue.Count > 0 ? queue.Dequeue() : CreateEnemy(entry);

        Vector3 spawnPos = GetSpawnPosition();
        enemy.transform.position = spawnPos;
        enemy.gameObject.SetActive(true);
        enemy.Initialize(entry.Data, playerTransform, this);

        activeEnemies.Add(enemy);
        enemiesSpawnedThisWave++;
    }

    private EnemyWaveEntry PickRandomEntry()
    {
        float totalWeight = 0f;
        foreach (var e in enemyTypes) totalWeight += e.SpawnWeight;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var e in enemyTypes)
        {
            cumulative += e.SpawnWeight;
            if (roll <= cumulative) return e;
        }

        return enemyTypes[0];
    }

    // ── Posición de spawn ────────────────────────────────────────────

    private Vector3 GetSpawnPosition()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = UnityEngine.Random.Range(minSpawnDistance, spawnRadius);

            Vector3 offset = new(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
            Vector3 candidate = playerTransform.position + offset;

            if (Physics.Raycast(candidate + Vector3.up * 5f, Vector3.down, out var hit, 10f))
                candidate.y = hit.point.y;

            return candidate;
        }

        return playerTransform.position + Vector3.forward * spawnRadius;
    }

    // ── Pool: devolver enemigos ──────────────────────────────────────

    public void ReturnEnemy(EnemyController enemy)
    {
        enemy.gameObject.SetActive(false);
        activeEnemies.Remove(enemy);

        var data = enemy.GetComponent<EnemyController>();
        foreach (var kvp in pools)
        {
            kvp.Value.Enqueue(enemy);
            break;
        }

        if (!waveInProgress && activeEnemies.Count == 0)
            OnWaveCompleted?.Invoke(currentWave);
    }

    private void CleanupDeadEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null || !activeEnemies[i].gameObject.activeInHierarchy)
                activeEnemies.RemoveAt(i);
        }
    }
}

/// <summary>
/// Entrada de configuración para un tipo de enemigo en el spawner.
/// Define el prefab, datos, y peso de probabilidad de spawn.
/// </summary>
[Serializable]
public struct EnemyWaveEntry
{
    public EnemyData Data;
    public EnemyController Prefab;
    [Range(0.1f, 10f)] public float SpawnWeight;
}
