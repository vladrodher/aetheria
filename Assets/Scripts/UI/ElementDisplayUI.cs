using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Muestra el elemento equipado actualmente: símbolo grande, nombre,
/// número atómico, color de fondo por categoría, y stats resumidos.
/// Se suscribe a WeaponController.OnElementChanged.
/// </summary>
public class ElementDisplayUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private WeaponController weaponController;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI symbolText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI atomicNumberText;
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private Image elementIcon;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI fireRateText;

    private void OnEnable()
    {
        if (weaponController == null) return;
        weaponController.OnElementChanged += UpdateDisplay;

        if (weaponController.CurrentElement != null)
            UpdateDisplay(weaponController.CurrentElement);
    }

    private void OnDisable()
    {
        if (weaponController == null) return;
        weaponController.OnElementChanged -= UpdateDisplay;
    }

    private void Start()
    {
        if (weaponController != null && weaponController.CurrentElement != null)
            UpdateDisplay(weaponController.CurrentElement);
    }

    private void UpdateDisplay(ElementData element)
    {
        if (element == null)
        {
            ClearDisplay();
            return;
        }

        if (symbolText != null)
        {
            symbolText.text = element.Symbol;
            symbolText.color = element.PrimaryColor;
        }

        if (nameText != null)
            nameText.text = element.ElementName;

        if (atomicNumberText != null)
            atomicNumberText.text = element.AtomicNumber.ToString();

        if (backgroundPanel != null)
        {
            Color bg = element.PrimaryColor;
            bg.a = 0.25f;
            backgroundPanel.color = bg;
        }

        if (elementIcon != null && element.Icon != null)
        {
            elementIcon.sprite = element.Icon;
            elementIcon.color = element.PrimaryColor;
            elementIcon.enabled = true;
        }
        else if (elementIcon != null)
        {
            elementIcon.enabled = false;
        }

        if (damageText != null)
        {
            float dmg = element.BaseDamage * element.DamageMultiplier;
            damageText.text = $"DMG {dmg:F0}";
        }

        if (fireRateText != null)
            fireRateText.text = $"RPS {element.FireRate:F1}";
    }

    private void ClearDisplay()
    {
        if (symbolText != null) symbolText.text = "?";
        if (nameText != null) nameText.text = "No Element";
        if (atomicNumberText != null) atomicNumberText.text = "-";
        if (backgroundPanel != null) backgroundPanel.color = new Color(0.2f, 0.2f, 0.2f, 0.25f);
        if (damageText != null) damageText.text = "DMG -";
        if (fireRateText != null) fireRateText.text = "RPS -";
    }
}
