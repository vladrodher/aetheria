using UnityEngine;

/// <summary>
/// Componente genérico que inflige daño por contacto físico.
/// Usado por enemigos cuerpo a cuerpo, hazards ambientales, trampas, etc.
/// Respeta un cooldown para no aplicar daño cada frame.
/// </summary>
public class ContactDamage : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private DamageType damageType = DamageType.Physical;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private LayerMask targetLayers = ~0;

    private ElementData sourceElement;
    private float lastDamageTime = float.NegativeInfinity;

    public void Configure(float dmg, DamageType type, float cd, ElementData element = null)
    {
        damage = dmg;
        damageType = type;
        cooldown = cd;
        sourceElement = element;
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamage(other.gameObject);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryDamage(collision.gameObject);
    }

    private void TryDamage(GameObject target)
    {
        if (Time.time - lastDamageTime < cooldown) return;
        if ((targetLayers & (1 << target.layer)) == 0) return;

        if (target.TryGetComponent(out IDamageable damageable) && damageable.IsAlive)
        {
            damageable.TakeDamage(damage, damageType, sourceElement);
            lastDamageTime = Time.time;
        }
    }
}
