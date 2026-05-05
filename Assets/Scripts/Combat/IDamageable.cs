/// <summary>
/// Interfaz para cualquier entidad que pueda recibir daño.
/// Implementada por jugador, enemigos, objetos destructibles, etc.
/// </summary>
public interface IDamageable
{
    void TakeDamage(float damage, DamageType damageType, ElementData sourceElement);
    bool IsAlive { get; }
}
