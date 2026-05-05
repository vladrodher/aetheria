using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona los efectos elementales activos sobre una entidad.
/// Aplica DoT via HealthSystem, calcula slow acumulado, gestiona VFX,
/// y emite eventos cuando se aplican o expiran efectos.
/// </summary>
[RequireComponent(typeof(HealthSystem))]
public class EffectReceiver : MonoBehaviour
{
    [SerializeField] private Renderer[] tintRenderers;

    private HealthSystem healthSystem;
    private readonly List<ActiveEffect> activeEffects = new();
    private MaterialPropertyBlock propertyBlock;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    /// <summary>
    /// Multiplicador de velocidad combinado de todos los efectos de slow activos.
    /// Los sistemas de movimiento deben multiplicar su velocidad por este valor.
    /// </summary>
    public float SpeedMultiplier { get; private set; } = 1f;

    public event Action<ActiveEffect> OnEffectApplied;
    public event Action<ActiveEffect> OnEffectExpired;

    public IReadOnlyList<ActiveEffect> Effects => activeEffects;
    public bool HasActiveEffects => activeEffects.Count > 0;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        propertyBlock = new MaterialPropertyBlock();

        if (tintRenderers == null || tintRenderers.Length == 0)
            tintRenderers = GetComponentsInChildren<Renderer>();
    }

    private void Update()
    {
        if (activeEffects.Count == 0) return;

        float dt = Time.deltaTime;
        bool needsCleanup = false;

        for (int i = 0; i < activeEffects.Count; i++)
        {
            var effect = activeEffects[i];
            float dot = effect.Tick(dt);

            if (dot > 0f && healthSystem.IsAlive)
                healthSystem.TakeDamage(dot, effect.Definition.DamageType, null);

            if (effect.IsExpired)
                needsCleanup = true;
        }

        if (needsCleanup)
            RemoveExpiredEffects();

        RecalculateSpeedMultiplier();
        UpdateVisualTint();
    }

    // ── Aplicar efectos ──────────────────────────────────────────────

    /// <summary>
    /// Aplica un efecto. Si ya existe, intenta stackear/refrescar.
    /// </summary>
    public void ApplyEffect(ElementEffect definition)
    {
        if (definition == null) return;
        if (!healthSystem.IsAlive) return;

        var existing = FindEffect(definition);
        if (existing != null)
        {
            existing.TryStack();
            return;
        }

        var active = new ActiveEffect(definition);
        activeEffects.Add(active);

        SpawnEffectVfx(active);
        OnEffectApplied?.Invoke(active);
    }

    /// <summary>
    /// Aplica múltiples efectos de una vez (ej: todos los on-hit de un ElementData).
    /// </summary>
    public void ApplyEffects(ElementEffect[] effects)
    {
        if (effects == null) return;
        foreach (var fx in effects)
            ApplyEffect(fx);
    }

    /// <summary>
    /// Elimina inmediatamente todas las instancias de un efecto.
    /// </summary>
    public void RemoveEffect(ElementEffect definition)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i].Definition == definition)
            {
                CleanupEffect(activeEffects[i]);
                activeEffects.RemoveAt(i);
            }
        }

        RecalculateSpeedMultiplier();
        UpdateVisualTint();
    }

    /// <summary>
    /// Purga todos los efectos activos.
    /// </summary>
    public void ClearAllEffects()
    {
        foreach (var effect in activeEffects)
            CleanupEffect(effect);

        activeEffects.Clear();
        SpeedMultiplier = 1f;
        UpdateVisualTint();
    }

    public bool HasEffect(ElementEffect definition)
    {
        return FindEffect(definition) != null;
    }

    // ── Internos ─────────────────────────────────────────────────────

    private ActiveEffect FindEffect(ElementEffect definition)
    {
        foreach (var effect in activeEffects)
        {
            if (effect.Definition == definition)
                return effect;
        }
        return null;
    }

    private void RemoveExpiredEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i].IsExpired)
            {
                var expired = activeEffects[i];
                CleanupEffect(expired);
                activeEffects.RemoveAt(i);
                OnEffectExpired?.Invoke(expired);
            }
        }
    }

    private void CleanupEffect(ActiveEffect effect)
    {
        if (effect.VfxInstance != null)
            Destroy(effect.VfxInstance);
    }

    private void RecalculateSpeedMultiplier()
    {
        float multiplier = 1f;
        foreach (var effect in activeEffects)
            multiplier *= effect.GetSlowMultiplier();

        SpeedMultiplier = multiplier;
    }

    // ── Visuales ─────────────────────────────────────────────────────

    private void SpawnEffectVfx(ActiveEffect effect)
    {
        if (effect.Definition.VfxPrefab == null) return;

        var vfx = Instantiate(effect.Definition.VfxPrefab, transform);
        effect.VfxInstance = vfx;
    }

    /// <summary>
    /// Mezcla los colores de todos los efectos activos como tint de emisión.
    /// Los stacks intensifican el color. Sin efectos = emisión negra (off).
    /// </summary>
    private void UpdateVisualTint()
    {
        Color blended = Color.black;

        foreach (var effect in activeEffects)
        {
            float intensity = effect.CurrentStacks * (effect.RemainingDuration > 0.3f ? 1f : 0.5f);
            blended += effect.Definition.TintColor * intensity;
        }

        foreach (var rend in tintRenderers)
        {
            if (rend == null) continue;
            rend.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(EmissionColorId, blended);
            rend.SetPropertyBlock(propertyBlock);
        }
    }

    private void OnDisable()
    {
        ClearAllEffects();
    }
}
