using UnityEngine;

/// <summary>
/// Define qué ocurre cuando dos elementos interactúan.
/// Ej: Hidrógeno (1) + Oxígeno (8) → Reacción "Vapor" con efecto de área.
/// El orden de los reactantes no importa (A+B == B+A).
/// </summary>
[CreateAssetMenu(fileName = "NewReaction", menuName = "Aetheria/Element Reaction")]
public class ElementReaction : ScriptableObject
{
    [Header("Reactantes")]
    [SerializeField] private ElementData elementA;
    [SerializeField] private ElementData elementB;

    [Header("Resultado")]
    [SerializeField] private string reactionName;
    [SerializeField, TextArea] private string description;

    [Header("Efectos de la reacción")]
    [SerializeField] private ElementEffect[] resultEffects;
    [SerializeField] private float damageMultiplier = 1.5f;
    [SerializeField] private float areaMultiplier = 1f;

    [Header("Visuales")]
    [SerializeField] private Color reactionColor = Color.white;
    [SerializeField] private GameObject reactionVfxPrefab;

    [Header("Gameplay")]
    [SerializeField] private float cooldown;
    [SerializeField] private float triggerRadius = 2f;

    // ── Propiedades públicas ─────────────────────────────────────────

    public ElementData ElementA => elementA;
    public ElementData ElementB => elementB;
    public string ReactionName => reactionName;
    public string Description => description;
    public ElementEffect[] ResultEffects => resultEffects;
    public float DamageMultiplier => damageMultiplier;
    public float AreaMultiplier => areaMultiplier;
    public Color ReactionColor => reactionColor;
    public GameObject ReactionVfxPrefab => reactionVfxPrefab;
    public float Cooldown => cooldown;
    public float TriggerRadius => triggerRadius;

    // ── Utilidades ───────────────────────────────────────────────────

    /// <summary>
    /// Genera una clave única para este par de elementos, independiente del orden.
    /// Se usa para indexar reacciones en el diccionario del database.
    /// </summary>
    public long ReactionKey => GetReactionKey(elementA, elementB);

    public static long GetReactionKey(ElementData a, ElementData b)
    {
        int lo = Mathf.Min(a.AtomicNumber, b.AtomicNumber);
        int hi = Mathf.Max(a.AtomicNumber, b.AtomicNumber);
        return ((long)lo << 32) | (uint)hi;
    }

    /// <summary>
    /// Verifica si esta reacción involucra al par dado (en cualquier orden).
    /// </summary>
    public bool Matches(ElementData a, ElementData b)
    {
        return ReactionKey == GetReactionKey(a, b);
    }
}
