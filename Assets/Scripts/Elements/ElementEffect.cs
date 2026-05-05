using UnityEngine;

/// <summary>
/// Efecto modular que un elemento puede aplicar al impactar.
/// Cada efecto es un asset independiente: Quemar, Congelar, Envenenar, etc.
/// Se asignan a ElementData o ElementReaction como listas, permitiendo
/// que un mismo efecto se reutilice en múltiples contextos.
/// </summary>
[CreateAssetMenu(fileName = "NewEffect", menuName = "Aetheria/Element Effect")]
public class ElementEffect : ScriptableObject
{
    [Header("Identidad")]
    [SerializeField] private string effectName;
    [SerializeField, TextArea] private string description;

    [Header("Tipo")]
    [SerializeField] private DamageType damageType = DamageType.Physical;
    [SerializeField] private bool isDebuff = true;

    [Header("Valores")]
    [SerializeField] private float damagePerSecond;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float slowMultiplier = 1f;

    [Header("Stacking")]
    [SerializeField] private int maxStacks = 1;
    [SerializeField] private bool refreshDurationOnReapply = true;

    [Header("Visuales")]
    [SerializeField] private Color tintColor = Color.white;
    [SerializeField] private GameObject vfxPrefab;

    // ── Propiedades públicas ─────────────────────────────────────────

    public string EffectName => effectName;
    public string Description => description;
    public DamageType DamageType => damageType;
    public bool IsDebuff => isDebuff;
    public float DamagePerSecond => damagePerSecond;
    public float Duration => duration;
    public float SlowMultiplier => slowMultiplier;
    public int MaxStacks => maxStacks;
    public bool RefreshDurationOnReapply => refreshDurationOnReapply;
    public Color TintColor => tintColor;
    public GameObject VfxPrefab => vfxPrefab;
}
