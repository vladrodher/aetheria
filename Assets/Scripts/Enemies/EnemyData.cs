using UnityEngine;

/// <summary>
/// Configuración de un tipo de enemigo. Define stats, afinidad elemental,
/// y comportamiento. Un solo asset puede usarse para múltiples instancias del mismo tipo.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemy", menuName = "Aetheria/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identidad")]
    [SerializeField] private string enemyName;
    [SerializeField, TextArea] private string description;

    [Header("Stats base")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float contactCooldown = 1f;

    [Header("Afinidad elemental")]
    [SerializeField] private ElementData elementAffinity;
    [SerializeField] private DamageType contactDamageType = DamageType.Physical;

    [Header("Resistencias")]
    [SerializeField, Range(0f, 1f)] private float physicalResistance;
    [SerializeField, Range(0f, 1f)] private float fireResistance;
    [SerializeField, Range(0f, 1f)] private float iceResistance;
    [SerializeField, Range(0f, 1f)] private float electricResistance;
    [SerializeField, Range(0f, 1f)] private float toxicResistance;
    [SerializeField, Range(0f, 1f)] private float radiantResistance;
    [SerializeField, Range(0f, 1f)] private float corrosiveResistance;
    [SerializeField, Range(0f, 1f)] private float explosiveResistance;

    [Header("Comportamiento")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Recompensa")]
    [SerializeField] private int scoreValue = 10;

    [Header("Visuales")]
    [SerializeField] private Color tintColor = Color.white;
    [SerializeField] private GameObject deathVfxPrefab;

    // ── Propiedades públicas ─────────────────────────────────────────

    public string EnemyName => enemyName;
    public string Description => description;

    public float MaxHealth => maxHealth;
    public float MoveSpeed => moveSpeed;
    public float ContactDamage => contactDamage;
    public float ContactCooldown => contactCooldown;

    public ElementData ElementAffinity => elementAffinity;
    public DamageType ContactDamageType => contactDamageType;

    public float DetectionRange => detectionRange;
    public float AttackRange => attackRange;
    public float RotationSpeed => rotationSpeed;

    public int ScoreValue => scoreValue;
    public Color TintColor => tintColor;
    public GameObject DeathVfxPrefab => deathVfxPrefab;

    // Acceso indexado a resistencias para inyectarlas en HealthSystem
    public float GetResistance(DamageType type)
    {
        return type switch
        {
            DamageType.Physical  => physicalResistance,
            DamageType.Fire      => fireResistance,
            DamageType.Ice       => iceResistance,
            DamageType.Electric  => electricResistance,
            DamageType.Toxic     => toxicResistance,
            DamageType.Radiant   => radiantResistance,
            DamageType.Corrosive => corrosiveResistance,
            DamageType.Explosive => explosiveResistance,
            _                    => 0f
        };
    }
}
