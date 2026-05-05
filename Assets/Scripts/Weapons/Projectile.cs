using UnityEngine;

/// <summary>
/// Proyectil elemental. Se inicializa con un ElementData que define su daño,
/// velocidad, color y efectos. Usa Rigidbody para moverse y detectar colisiones.
/// Gestionado por ProjectilePool (nunca se destruye, se recicla).
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class Projectile : MonoBehaviour
{
    [Header("Configuración base")]
    [SerializeField] private float maxLifetime = 3f;
    [SerializeField] private LayerMask hitLayers = ~0;

    [Header("Visuales")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private TrailRenderer trail;

    private Rigidbody rb;
    private ProjectilePool pool;

    private ElementData elementData;
    private float damage;
    private float spawnTime;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        propertyBlock = new MaterialPropertyBlock();
    }

    public void SetPool(ProjectilePool ownerPool)
    {
        pool = ownerPool;
    }

    /// <summary>
    /// Configura y lanza el proyectil. Llamado por WeaponController al disparar.
    /// </summary>
    public void Initialize(ElementData element, Vector3 direction, float damageValue)
    {
        elementData = element;
        damage = damageValue;
        spawnTime = Time.time;

        rb.linearVelocity = direction.normalized * element.ProjectileSpeed;

        ApplyVisuals();
    }

    private void ApplyVisuals()
    {
        Color color = elementData.PrimaryColor;

        if (bodyRenderer != null)
        {
            bodyRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(EmissionColorId, color * 2f);
            bodyRenderer.SetPropertyBlock(propertyBlock);
        }

        if (trail != null)
        {
            trail.startColor = color;
            trail.endColor = new Color(color.r, color.g, color.b, 0f);
            trail.Clear();
        }
    }

    private void Update()
    {
        if (Time.time - spawnTime >= maxLifetime)
            ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((hitLayers & (1 << other.gameObject.layer)) == 0) return;

        if (other.TryGetComponent(out IDamageable target) && target.IsAlive)
        {
            target.TakeDamage(damage, elementData.DamageType, elementData);

            if (elementData.OnHitEffects != null
                && elementData.OnHitEffects.Length > 0
                && other.TryGetComponent(out EffectReceiver receiver))
            {
                receiver.ApplyEffects(elementData.OnHitEffects);
            }

            if (other.TryGetComponent(out ElementTracker tracker)
                && ReactionManager.Instance != null)
            {
                other.TryGetComponent(out HealthSystem health);
                other.TryGetComponent(out EffectReceiver effects);
                ReactionManager.Instance.TryTriggerReaction(
                    elementData, tracker, health, effects, transform.position);
            }
        }

        SpawnImpactVfx();
        ReturnToPool();
    }

    private void SpawnImpactVfx()
    {
        if (elementData.ImpactVfxPrefab == null) return;

        // TODO: poolear VFX de impacto también
        var vfx = Instantiate(elementData.ImpactVfxPrefab, transform.position, Quaternion.identity);
        Destroy(vfx, 2f);
    }

    private void ReturnToPool()
    {
        rb.linearVelocity = Vector3.zero;
        if (pool != null)
            pool.Return(this);
        else
            gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        rb.linearVelocity = Vector3.zero;
    }
}
