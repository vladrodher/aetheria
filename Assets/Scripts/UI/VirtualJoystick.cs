using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// Joystick virtual reutilizable para controles táctiles.
/// Arrastra dentro del área del joystick y devuelve un Vector2 normalizado.
/// </summary>
public class VirtualJoystick : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Referencias UI")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;

    [Header("Comportamiento")]
    [SerializeField, Range(0f, 1f)] private float deadZone = 0.1f;
    [SerializeField] private bool autoHide;

    [Header("Eventos")]
    [SerializeField] private UnityEvent<Vector2> onValueChanged;

    private Canvas parentCanvas;
    private Vector2 inputValue;
    private float handleRange;

    public Vector2 Value => inputValue;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        handleRange = (background.sizeDelta.x - handle.sizeDelta.x) * 0.5f;

        if (autoHide)
            SetAlpha(0.4f);
    }

    // ── Pointer Handlers ────────────────────────────────────────────

    public void OnPointerDown(PointerEventData eventData)
    {
        if (autoHide)
            SetAlpha(1f);

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Convertir la posición del toque al espacio local del background
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, eventData.position, parentCanvas.worldCamera, out Vector2 localPoint);

        // Normalizar al rango [-1, 1] dentro del radio del background
        float radius = background.sizeDelta.x * 0.5f;
        Vector2 normalized = localPoint / radius;

        // Clampear dentro del círculo unitario
        inputValue = normalized.sqrMagnitude > 1f
            ? normalized.normalized
            : normalized;

        // Aplicar dead zone
        if (inputValue.magnitude < deadZone)
            inputValue = Vector2.zero;

        // Mover el handle visualmente
        handle.anchoredPosition = inputValue * handleRange;

        onValueChanged?.Invoke(inputValue);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputValue = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;

        if (autoHide)
            SetAlpha(0.4f);

        onValueChanged?.Invoke(inputValue);
    }

    // ── Utilidades ──────────────────────────────────────────────────

    private void SetAlpha(float alpha)
    {
        if (TryGetComponent(out CanvasGroup group))
            group.alpha = alpha;
    }
}
