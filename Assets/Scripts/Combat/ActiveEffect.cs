using UnityEngine;

/// <summary>
/// Instancia runtime de un ElementEffect activo sobre una entidad.
/// Trackea duración restante, stacks actuales, y timer de tick para DoT.
/// No es MonoBehaviour: lo gestiona EffectReceiver como dato puro.
/// </summary>
public class ActiveEffect
{
    public ElementEffect Definition { get; }
    public int CurrentStacks { get; private set; }
    public float RemainingDuration { get; private set; }
    public bool IsExpired => RemainingDuration <= 0f;
    public GameObject VfxInstance { get; set; }

    private float tickAccumulator;
    private const float TickInterval = 0.5f;

    public ActiveEffect(ElementEffect definition)
    {
        Definition = definition;
        CurrentStacks = 1;
        RemainingDuration = definition.Duration;
    }

    /// <summary>
    /// Intenta agregar un stack o refrescar duración al reaplicar el mismo efecto.
    /// Retorna true si el stack fue aceptado.
    /// </summary>
    public bool TryStack()
    {
        if (Definition.RefreshDurationOnReapply)
            RemainingDuration = Definition.Duration;

        if (CurrentStacks < Definition.MaxStacks)
        {
            CurrentStacks++;
            return true;
        }

        return Definition.RefreshDurationOnReapply;
    }

    /// <summary>
    /// Avanza el efecto un frame. Retorna el daño DoT acumulado en este frame
    /// (puede ser 0 si no tocó tick). El daño escala con los stacks actuales.
    /// </summary>
    public float Tick(float deltaTime)
    {
        RemainingDuration -= deltaTime;
        if (Definition.DamagePerSecond <= 0f) return 0f;

        tickAccumulator += deltaTime;

        float totalDamage = 0f;
        while (tickAccumulator >= TickInterval)
        {
            tickAccumulator -= TickInterval;
            totalDamage += Definition.DamagePerSecond * TickInterval * CurrentStacks;
        }

        return totalDamage;
    }

    /// <summary>
    /// Multiplicador de velocidad actual. Con múltiples stacks se acumula
    /// multiplicativamente: 0.4 × 0.4 = 0.16 (muy lento con 2 stacks de Freeze).
    /// </summary>
    public float GetSlowMultiplier()
    {
        if (Definition.SlowMultiplier >= 1f) return 1f;
        return Mathf.Pow(Definition.SlowMultiplier, CurrentStacks);
    }
}
