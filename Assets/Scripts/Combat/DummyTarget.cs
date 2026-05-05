using UnityEngine;

/// <summary>
/// Diana de prueba para testear el sistema de disparo y daño.
/// Muestra la vida en consola, se destruye al morir, y opcionalmente respawnea.
/// Agregar a un cubo con Collider (is trigger) + HealthSystem + DamageFlash.
/// </summary>
[RequireComponent(typeof(HealthSystem))]
public class DummyTarget : MonoBehaviour
{
    [SerializeField] private bool respawnOnDeath = true;
    [SerializeField] private float respawnDelay = 2f;

    private HealthSystem healthSystem;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void OnEnable()
    {
        healthSystem.OnDamaged += HandleDamage;
        healthSystem.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        healthSystem.OnDamaged -= HandleDamage;
        healthSystem.OnDeath -= HandleDeath;
    }

    private void HandleDamage(DamageEvent evt)
    {
        string element = evt.SourceElement != null ? evt.SourceElement.Symbol : "?";
        Debug.Log($"[Dummy] Hit by <b>{element}</b> ({evt.DamageType}) " +
                  $"for <b>{evt.DamageDealt:F1}</b> dmg → " +
                  $"HP: {healthSystem.CurrentHealth:F0}/{healthSystem.MaxHealth:F0}");
    }

    private void HandleDeath(DeathEvent evt)
    {
        string element = evt.LastHit.SourceElement != null
            ? evt.LastHit.SourceElement.ElementName
            : "Unknown";

        Debug.Log($"[Dummy] <color=red>DESTROYED</color> by {element}!");

        if (respawnOnDeath)
            Invoke(nameof(Respawn), respawnDelay);
        else
            gameObject.SetActive(false);
    }

    private void Respawn()
    {
        transform.SetPositionAndRotation(startPosition, startRotation);
        healthSystem.FullHeal();
        gameObject.SetActive(true);
        Debug.Log("[Dummy] Respawned!");
    }
}
