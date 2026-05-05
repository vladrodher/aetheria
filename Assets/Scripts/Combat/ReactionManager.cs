using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton que gestiona reacciones elementales. Cuando un proyectil impacta,
/// el Projectile llama a TryTriggerReaction(). El manager consulta la
/// PeriodicTableDatabase, y si encuentra un par reactivo, ejecuta la reacción:
/// daño bonus, efectos, VFX, y daño en área a enemigos cercanos.
/// </summary>
public class ReactionManager : MonoBehaviour
{
    [SerializeField] private PeriodicTableDatabase database;
    [SerializeField] private LayerMask reactionAreaLayers = ~0;

    // Cooldown por reacción por entidad para evitar spam
    private readonly Dictionary<long, float> reactionCooldowns = new();

    public static ReactionManager Instance { get; private set; }

    public event Action<ReactionEvent> OnReactionTriggered;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Llamado cuando un proyectil de un elemento impacta a una entidad con ElementTracker.
    /// Revisa si el nuevo elemento puede reaccionar con alguno previamente trackeado.
    /// </summary>
    public void TryTriggerReaction(ElementData newElement, ElementTracker tracker,
        HealthSystem targetHealth, EffectReceiver targetEffects, Vector3 hitPoint)
    {
        if (database == null || newElement == null) return;

        tracker.RegisterElement(newElement, out var activeElements);

        foreach (var existing in activeElements)
        {
            var reaction = database.GetReaction(newElement, existing);
            if (reaction == null) continue;

            long cooldownKey = GetCooldownKey(reaction, tracker);
            if (IsOnCooldown(cooldownKey)) continue;

            ExecuteReaction(reaction, tracker, targetHealth, targetEffects, hitPoint);
            SetCooldown(cooldownKey, reaction.Cooldown);
            return;
        }
    }

    // ── Ejecución de reacción ────────────────────────────────────────

    private void ExecuteReaction(ElementReaction reaction, ElementTracker tracker,
        HealthSystem targetHealth, EffectReceiver targetEffects, Vector3 hitPoint)
    {
        // Consumir los elementos usados para que no reaccionen de nuevo sin re-impactar
        tracker.ConsumeReactionPair(reaction.ElementA, reaction.ElementB);

        // Daño bonus al objetivo principal
        if (targetHealth != null && targetHealth.IsAlive)
        {
            float reactionDamage =
                (reaction.ElementA.BaseDamage + reaction.ElementB.BaseDamage)
                * 0.5f * reaction.DamageMultiplier;

            targetHealth.TakeDamage(reactionDamage, DamageType.Physical, null);
        }

        // Aplicar efectos de la reacción
        if (targetEffects != null && reaction.ResultEffects != null)
            targetEffects.ApplyEffects(reaction.ResultEffects);

        // VFX
        SpawnReactionVfx(reaction, hitPoint);

        // Daño en área a entidades cercanas (si la reacción tiene radio)
        if (reaction.TriggerRadius > 0f)
            ApplyAreaDamage(reaction, hitPoint, targetHealth);

        // Evento global
        OnReactionTriggered?.Invoke(new ReactionEvent(reaction, hitPoint));
    }

    // ── Daño en área ─────────────────────────────────────────────────

    private void ApplyAreaDamage(ElementReaction reaction, Vector3 center,
        HealthSystem exclude)
    {
        float radius = reaction.TriggerRadius * reaction.AreaMultiplier;
        var colliders = Physics.OverlapSphere(center, radius, reactionAreaLayers);

        float areaDamage =
            (reaction.ElementA.BaseDamage + reaction.ElementB.BaseDamage)
            * 0.25f * reaction.DamageMultiplier;

        foreach (var col in colliders)
        {
            if (!col.TryGetComponent(out HealthSystem health)) continue;
            if (health == exclude) continue;
            if (!health.IsAlive) continue;

            health.TakeDamage(areaDamage, DamageType.Physical, null);

            if (reaction.ResultEffects != null
                && col.TryGetComponent(out EffectReceiver receiver))
            {
                receiver.ApplyEffects(reaction.ResultEffects);
            }
        }
    }

    // ── VFX ──────────────────────────────────────────────────────────

    private void SpawnReactionVfx(ElementReaction reaction, Vector3 position)
    {
        if (reaction.ReactionVfxPrefab == null) return;

        var vfx = Instantiate(reaction.ReactionVfxPrefab, position, Quaternion.identity);
        float scale = reaction.AreaMultiplier;
        vfx.transform.localScale = Vector3.one * scale;
        Destroy(vfx, 3f);
    }

    // ── Cooldowns ────────────────────────────────────────────────────

    private long GetCooldownKey(ElementReaction reaction, ElementTracker tracker)
    {
        int entityId = tracker.GetInstanceID();
        return ((long)entityId << 32) | (uint)reaction.ReactionKey;
    }

    private bool IsOnCooldown(long key)
    {
        return reactionCooldowns.TryGetValue(key, out float until) && Time.time < until;
    }

    private void SetCooldown(long key, float duration)
    {
        if (duration <= 0f) return;
        reactionCooldowns[key] = Time.time + duration;
    }
}

/// <summary>
/// Datos de un evento de reacción para listeners externos (UI, audio, cámara shake, etc.)
/// </summary>
public readonly struct ReactionEvent
{
    public readonly ElementReaction Reaction;
    public readonly Vector3 Position;

    public ReactionEvent(ElementReaction reaction, Vector3 position)
    {
        Reaction = reaction;
        Position = position;
    }
}
