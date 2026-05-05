using System;
using UnityEngine;

/// <summary>
/// Sistema de vida genérico. Se agrega a jugador, enemigos, y destructibles.
/// Implementa IDamageable y emite eventos C# para que otros sistemas reaccionen
/// sin acoplamiento (UI, audio, VFX, game over, loot, etc.)
/// </summary>
public class HealthSystem : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Invulnerabilidad post-daño")]
    [SerializeField] private float invincibilityDuration;

    [Header("Resistencias elementales")]
    [SerializeField, Range(0f, 1f)] private float physicalResistance;
    [SerializeField, Range(0f, 1f)] private float fireResistance;
    [SerializeField, Range(0f, 1f)] private float iceResistance;
    [SerializeField, Range(0f, 1f)] private float electricResistance;
    [SerializeField, Range(0f, 1f)] private float toxicResistance;
    [SerializeField, Range(0f, 1f)] private float radiantResistance;
    [SerializeField, Range(0f, 1f)] private float corrosiveResistance;
    [SerializeField, Range(0f, 1f)] private float explosiveResistance;

    private float invincibleUntil;

    // ── Eventos C# ───────────────────────────────────────────────────

    public event Action<DamageEvent> OnDamaged;
    public event Action<HealEvent> OnHealed;
    public event Action<DeathEvent> OnDeath;

    // ── Propiedades ──────────────────────────────────────────────────

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercent => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public bool IsAlive => currentHealth > 0f;
    public bool IsInvincible => Time.time < invincibleUntil;

    // ── Ciclo de vida ────────────────────────────────────────────────

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // ── IDamageable ──────────────────────────────────────────────────

    public void TakeDamage(float damage, DamageType damageType, ElementData sourceElement)
    {
        if (!IsAlive) return;
        if (IsInvincible) return;
        if (damage <= 0f) return;

        float resistance = GetResistance(damageType);
        float finalDamage = damage * (1f - resistance);

        currentHealth = Mathf.Max(0f, currentHealth - finalDamage);

        if (invincibilityDuration > 0f)
            invincibleUntil = Time.time + invincibilityDuration;

        var evt = new DamageEvent(
            damage, finalDamage, damageType, sourceElement, transform.position);

        OnDamaged?.Invoke(evt);

        if (!IsAlive)
            OnDeath?.Invoke(new DeathEvent(evt, gameObject));
    }

    // ── Curación ─────────────────────────────────────────────────────

    public void Heal(float amount)
    {
        if (!IsAlive) return;
        if (amount <= 0f) return;

        float before = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        float healed = currentHealth - before;

        if (healed > 0f)
            OnHealed?.Invoke(new HealEvent(healed, currentHealth, maxHealth));
    }

    public void FullHeal()
    {
        Heal(maxHealth - currentHealth);
    }

    // ── Modificadores de stats ───────────────────────────────────────

    public void SetMaxHealth(float newMax, bool healToNew = false)
    {
        maxHealth = Mathf.Max(1f, newMax);
        if (healToNew)
            currentHealth = maxHealth;
        else
            currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    // ── Resistencias ─────────────────────────────────────────────────

    private float GetResistance(DamageType type)
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
