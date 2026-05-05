using UnityEngine;

/// <summary>
/// Structs inmutables para los eventos de combate.
/// Pasan por valor, cero allocations, fáciles de inspeccionar en debug.
/// </summary>
public readonly struct DamageEvent
{
    public readonly float Damage;
    public readonly float DamageDealt;
    public readonly DamageType DamageType;
    public readonly ElementData SourceElement;
    public readonly Vector3 HitPoint;

    public DamageEvent(float damage, float damageDealt, DamageType damageType,
        ElementData sourceElement, Vector3 hitPoint)
    {
        Damage = damage;
        DamageDealt = damageDealt;
        DamageType = damageType;
        SourceElement = sourceElement;
        HitPoint = hitPoint;
    }
}

public readonly struct DeathEvent
{
    public readonly DamageEvent LastHit;
    public readonly GameObject Victim;

    public DeathEvent(DamageEvent lastHit, GameObject victim)
    {
        LastHit = lastHit;
        Victim = victim;
    }
}

public readonly struct HealEvent
{
    public readonly float Amount;
    public readonly float CurrentHealth;
    public readonly float MaxHealth;

    public HealEvent(float amount, float currentHealth, float maxHealth)
    {
        Amount = amount;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
    }
}
