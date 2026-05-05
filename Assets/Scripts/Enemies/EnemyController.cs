using UnityEngine;

/// <summary>
/// IA básica de enemigo para twin-stick shooter.
/// Persigue al jugador usando Rigidbody, aplica datos de EnemyData,
/// y se auto-configura con HealthSystem y ContactDamage.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(HealthSystem), typeof(ContactDamage))]
[RequireComponent(typeof(EffectReceiver), typeof(ElementTracker))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Renderer bodyRenderer;

    private Rigidbody rb;
    private HealthSystem healthSystem;
    private ContactDamage contactDamage;
    private EffectReceiver effectReceiver;
    private ElementTracker elementTracker;
    private Transform target;
    private EnemySpawner spawner;

    private MaterialPropertyBlock propertyBlock;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    // ── Inicialización ───────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        healthSystem = GetComponent<HealthSystem>();
        contactDamage = GetComponent<ContactDamage>();
        effectReceiver = GetComponent<EffectReceiver>();
        elementTracker = GetComponent<ElementTracker>();
        propertyBlock = new MaterialPropertyBlock();

        rb.freezeRotation = true;
        rb.useGravity = true;
    }

    /// <summary>
    /// Inicializa el enemigo con sus datos y referencia al jugador.
    /// Llamado por el spawner al activar una instancia del pool.
    /// </summary>
    public void Initialize(EnemyData data, Transform playerTarget, EnemySpawner ownerSpawner)
    {
        enemyData = data;
        target = playerTarget;
        spawner = ownerSpawner;

        healthSystem.SetMaxHealth(data.MaxHealth, healToNew: true);
        effectReceiver?.ClearAllEffects();
        elementTracker?.ClearAll();

        contactDamage.Configure(
            data.ContactDamage,
            data.ContactDamageType,
            data.ContactCooldown,
            data.ElementAffinity
        );

        ApplyVisuals();
    }

    private void OnEnable()
    {
        healthSystem.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        healthSystem.OnDeath -= HandleDeath;
    }

    // ── IA: persecución ──────────────────────────────────────────────

    private void FixedUpdate()
    {
        if (target == null) return;
        if (!healthSystem.IsAlive) return;

        ChaseTarget();
        RotateTowardTarget();
    }

    private void ChaseTarget()
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;

        if (distance < 0.1f) return;
        if (distance > enemyData.DetectionRange) return;

        Vector3 direction = toTarget / distance;
        float speed = enemyData.MoveSpeed;
        if (effectReceiver != null)
            speed *= effectReceiver.SpeedMultiplier;

        Vector3 desiredVelocity = direction * speed;

        Vector3 currentVel = rb.linearVelocity;
        Vector3 smoothed = Vector3.MoveTowards(
            new Vector3(currentVel.x, 0f, currentVel.z),
            desiredVelocity,
            enemyData.MoveSpeed * 5f * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector3(smoothed.x, currentVel.y, smoothed.z);
    }

    private void RotateTowardTarget()
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(toTarget);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            enemyData.RotationSpeed * Time.fixedDeltaTime
        );
    }

    // ── Muerte ───────────────────────────────────────────────────────

    private void HandleDeath(DeathEvent evt)
    {
        rb.linearVelocity = Vector3.zero;

        if (enemyData.DeathVfxPrefab != null)
        {
            var vfx = Instantiate(enemyData.DeathVfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        if (spawner != null)
            spawner.ReturnEnemy(this);
        else
            gameObject.SetActive(false);
    }

    // ── Visuales ─────────────────────────────────────────────────────

    private void ApplyVisuals()
    {
        if (bodyRenderer == null) return;

        bodyRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(BaseColorId, enemyData.TintColor);
        bodyRenderer.SetPropertyBlock(propertyBlock);
    }
}
