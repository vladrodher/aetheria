using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida con doble fill: la barra principal sigue la vida actual,
/// y una barra secundaria "trailing" muestra el daño reciente con delay.
/// Se suscribe a los eventos de HealthSystem.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private HealthSystem targetHealth;
    [SerializeField] private Image fillBar;
    [SerializeField] private Image trailingBar;
    [SerializeField] private Image backgroundBar;

    [Header("Configuración")]
    [SerializeField] private float trailingSpeed = 2f;
    [SerializeField] private float trailingDelay = 0.5f;
    [SerializeField] private Gradient healthGradient;

    private float trailingTarget;
    private float trailingDelayTimer;

    private void Start()
    {
        if (healthGradient.colorKeys.Length == 0)
        {
            healthGradient = new Gradient();
            healthGradient.SetKeys(
                new[] {
                    new GradientColorKey(new Color(0.8f, 0.1f, 0.1f), 0f),
                    new GradientColorKey(new Color(0.9f, 0.7f, 0.1f), 0.5f),
                    new GradientColorKey(new Color(0.2f, 0.9f, 0.2f), 1f)
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            );
        }

        UpdateBar(targetHealth.HealthPercent);

        if (trailingBar != null)
        {
            trailingBar.fillAmount = targetHealth.HealthPercent;
            trailingTarget = targetHealth.HealthPercent;
        }
    }

    private void OnEnable()
    {
        if (targetHealth == null) return;
        targetHealth.OnDamaged += OnHealthChanged;
        targetHealth.OnHealed += OnHealChanged;
    }

    private void OnDisable()
    {
        if (targetHealth == null) return;
        targetHealth.OnDamaged -= OnHealthChanged;
        targetHealth.OnHealed -= OnHealChanged;
    }

    private void OnHealthChanged(DamageEvent evt)
    {
        UpdateBar(targetHealth.HealthPercent);
        trailingDelayTimer = trailingDelay;
    }

    private void OnHealChanged(HealEvent evt)
    {
        float percent = targetHealth.HealthPercent;
        UpdateBar(percent);

        if (trailingBar != null)
        {
            trailingBar.fillAmount = percent;
            trailingTarget = percent;
        }
    }

    private void Update()
    {
        if (trailingBar == null) return;

        trailingTarget = targetHealth.HealthPercent;

        if (trailingDelayTimer > 0f)
        {
            trailingDelayTimer -= Time.deltaTime;
            return;
        }

        trailingBar.fillAmount = Mathf.MoveTowards(
            trailingBar.fillAmount, trailingTarget, trailingSpeed * Time.deltaTime);
    }

    private void UpdateBar(float percent)
    {
        if (fillBar != null)
        {
            fillBar.fillAmount = percent;
            fillBar.color = healthGradient.Evaluate(percent);
        }
    }

    public void SetTarget(HealthSystem health)
    {
        if (targetHealth != null)
        {
            targetHealth.OnDamaged -= OnHealthChanged;
            targetHealth.OnHealed -= OnHealChanged;
        }

        targetHealth = health;

        if (targetHealth != null)
        {
            targetHealth.OnDamaged += OnHealthChanged;
            targetHealth.OnHealed += OnHealChanged;
            UpdateBar(targetHealth.HealthPercent);
        }
    }
}
