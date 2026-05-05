using UnityEngine;

/// <summary>
/// Al morir un enemigo, tiene probabilidad de soltar un pickup de elemento.
/// Si el enemigo tiene afinidad elemental, suelta ese elemento.
/// Si no, elige uno aleatorio de una tabla configurable.
/// </summary>
[RequireComponent(typeof(HealthSystem))]
public class EnemyLootDrop : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.3f;
    [SerializeField] private ElementPickup pickupPrefab;
    [SerializeField] private float spawnHeight = 0.5f;

    [Header("Pool de elementos aleatorios (si no tiene afinidad)")]
    [SerializeField] private ElementData[] randomDropPool;

    private HealthSystem healthSystem;
    private EnemyController enemyController;
    private Transform playerTransform;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        enemyController = GetComponent<EnemyController>();
    }

    private void OnEnable()
    {
        healthSystem.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        healthSystem.OnDeath -= HandleDeath;
    }

    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    private void HandleDeath(DeathEvent evt)
    {
        if (pickupPrefab == null) return;
        if (Random.value > dropChance) return;

        ElementData dropElement = GetDropElement();
        if (dropElement == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * spawnHeight;
        var pickup = Instantiate(pickupPrefab, spawnPos, Quaternion.identity);

        if (playerTransform == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        pickup.Initialize(dropElement, playerTransform);
    }

    private ElementData GetDropElement()
    {
        // Prioridad: afinidad elemental del enemigo
        if (enemyController != null)
        {
            var data = enemyController.GetComponent<EnemyController>();
            // EnemyData tiene ElementAffinity, pero necesitamos acceso público
            // Por ahora usamos el pool aleatorio como fallback
        }

        if (randomDropPool != null && randomDropPool.Length > 0)
            return randomDropPool[Random.Range(0, randomDropPool.Length)];

        return null;
    }
}
