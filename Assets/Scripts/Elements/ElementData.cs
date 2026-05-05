using UnityEngine;

/// <summary>
/// Datos maestros de un elemento de la tabla periódica.
/// Cada elemento es un asset .asset que combina datos químicos reales
/// con propiedades de gameplay (daño, cadencia, efectos).
/// </summary>
[CreateAssetMenu(fileName = "NewElement", menuName = "Aetheria/Element Data")]
public class ElementData : ScriptableObject
{
    // ── Identidad química ────────────────────────────────────────────

    [Header("Identidad")]
    [SerializeField] private int atomicNumber;
    [SerializeField] private string symbol;
    [SerializeField] private string elementName;
    [SerializeField, TextArea] private string description;

    [Header("Clasificación")]
    [SerializeField] private ElementCategory category;
    [SerializeField] private ElementPhase phase;
    [SerializeField] private int period;
    [SerializeField] private int group;

    // ── Visuales ─────────────────────────────────────────────────────

    [Header("Visuales")]
    [SerializeField] private Sprite icon;
    [SerializeField] private Color primaryColor = Color.white;
    [SerializeField] private Color secondaryColor = Color.gray;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject impactVfxPrefab;

    // ── Stats de combate (como munición) ─────────────────────────────

    [Header("Proyectil")]
    [SerializeField] private DamageType damageType = DamageType.Physical;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float fireRate = 5f;
    [SerializeField] private float areaOfEffect;

    // ── Stats como modificador de arma ───────────────────────────────

    [Header("Modificador")]
    [SerializeField, Range(0.5f, 2f)] private float damageMultiplier = 1f;
    [SerializeField, Range(0.5f, 2f)] private float speedMultiplier = 1f;
    [SerializeField, Range(0.5f, 2f)] private float rateMultiplier = 1f;

    // ── Efectos al impactar ──────────────────────────────────────────

    [Header("Efectos")]
    [SerializeField] private ElementEffect[] onHitEffects;

    // ── Crafteo ──────────────────────────────────────────────────────

    [Header("Crafteo")]
    [SerializeField] private int rarity = 1;
    [SerializeField] private bool discoveredByDefault;

    // ── Propiedades públicas: Identidad ──────────────────────────────

    public int AtomicNumber => atomicNumber;
    public string Symbol => symbol;
    public string ElementName => elementName;
    public string Description => description;
    public ElementCategory Category => category;
    public ElementPhase Phase => phase;
    public int Period => period;
    public int Group => group;

    // ── Propiedades públicas: Visuales ───────────────────────────────

    public Sprite Icon => icon;
    public Color PrimaryColor => primaryColor;
    public Color SecondaryColor => secondaryColor;
    public GameObject ProjectilePrefab => projectilePrefab;
    public GameObject ImpactVfxPrefab => impactVfxPrefab;

    // ── Propiedades públicas: Combate ────────────────────────────────

    public DamageType DamageType => damageType;
    public float BaseDamage => baseDamage;
    public float ProjectileSpeed => projectileSpeed;
    public float FireRate => fireRate;
    public float AreaOfEffect => areaOfEffect;

    // ── Propiedades públicas: Modificador ────────────────────────────

    public float DamageMultiplier => damageMultiplier;
    public float SpeedMultiplier => speedMultiplier;
    public float RateMultiplier => rateMultiplier;

    // ── Propiedades públicas: Efectos ────────────────────────────────

    public ElementEffect[] OnHitEffects => onHitEffects;

    // ── Propiedades públicas: Crafteo ────────────────────────────────

    public int Rarity => rarity;
    public bool DiscoveredByDefault => discoveredByDefault;

    // ── Utilidades ───────────────────────────────────────────────────

    /// <summary>
    /// Posición (columna, fila) dentro de la cuadrícula de la tabla periódica.
    /// Útil para la UI de la tabla.
    /// </summary>
    public Vector2Int GridPosition => new(group, period);
}
