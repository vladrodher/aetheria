using UnityEngine;

/// <summary>
/// Flash visual al recibir daño. Cambia el color del material brevemente
/// al color del elemento que impactó. Se suscribe al evento OnDamaged del HealthSystem.
/// </summary>
[RequireComponent(typeof(HealthSystem))]
public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Renderer[] targetRenderers;
    [SerializeField] private Color defaultFlashColor = Color.white;
    [SerializeField] private float flashDuration = 0.12f;
    [SerializeField] private float flashIntensity = 3f;

    private HealthSystem healthSystem;
    private MaterialPropertyBlock propertyBlock;
    private float flashTimer;
    private Color currentFlashColor;
    private bool isFlashing;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        propertyBlock = new MaterialPropertyBlock();

        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>();
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
        currentFlashColor = evt.SourceElement != null
            ? evt.SourceElement.PrimaryColor
            : defaultFlashColor;

        flashTimer = flashDuration;
        isFlashing = true;
    }

    private void HandleDeath(DeathEvent evt)
    {
        isFlashing = false;
        ClearFlash();
    }

    private void Update()
    {
        if (!isFlashing) return;

        flashTimer -= Time.deltaTime;

        if (flashTimer <= 0f)
        {
            isFlashing = false;
            ClearFlash();
            return;
        }

        float t = flashTimer / flashDuration;
        Color emission = currentFlashColor * (t * flashIntensity);
        ApplyEmission(emission);
    }

    private void ApplyEmission(Color emission)
    {
        foreach (var rend in targetRenderers)
        {
            if (rend == null) continue;
            rend.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(EmissionColorId, emission);
            rend.SetPropertyBlock(propertyBlock);
        }
    }

    private void ClearFlash()
    {
        ApplyEmission(Color.black);
    }
}
