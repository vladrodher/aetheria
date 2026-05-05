using UnityEngine;

/// <summary>
/// Sistema de disparo del jugador. Lee el elemento equipado, controla la cadencia,
/// y dispara proyectiles desde el pool cuando el joystick de apuntado está activo.
/// En twin-stick shooters, apuntar = disparar automáticamente.
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Elemento equipado")]
    [SerializeField] private ElementData currentElement;

    [Header("Configuración")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float firePointOffset = 0.8f;

    private IsometricPlayerController playerController;
    private float nextFireTime;

    public ElementData CurrentElement => currentElement;

    private void Awake()
    {
        playerController = GetComponent<IsometricPlayerController>();
    }

    private void Update()
    {
        if (!playerController.IsAiming) return;
        if (currentElement == null) return;
        if (Time.time < nextFireTime) return;

        Fire();
        nextFireTime = Time.time + 1f / currentElement.FireRate;
    }

    private void Fire()
    {
        Vector3 direction = transform.forward;
        Vector3 spawnPos = GetFirePointPosition(direction);

        var projectile = ProjectilePool.Instance.Get(spawnPos, Quaternion.LookRotation(direction));
        float damage = currentElement.BaseDamage * currentElement.DamageMultiplier;

        projectile.Initialize(currentElement, direction, damage);
    }

    private Vector3 GetFirePointPosition(Vector3 direction)
    {
        if (firePoint != null)
            return firePoint.position;

        return transform.position + direction * firePointOffset + Vector3.up * 0.5f;
    }

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>
    /// Cambia el elemento equipado en runtime (al recoger uno nuevo, abrir menú, etc.)
    /// </summary>
    public void EquipElement(ElementData newElement)
    {
        currentElement = newElement;
        nextFireTime = 0f;
    }
}
