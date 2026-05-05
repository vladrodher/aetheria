using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador twin-stick shooter con perspectiva isométrica.
/// Transforma el input de ambos joysticks al espacio de la cámara
/// para que "arriba" en pantalla siempre sea "arriba" en el joystick.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class IsometricPlayerController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera isometricCamera;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float acceleration = 50f;

    [Header("Rotación")]
    [SerializeField] private float rotationSpeed = 720f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 aimInput;

    // Vectores "forward" y "right" de la cámara proyectados sobre el plano XZ.
    // Se cachean cada frame para evitar recalcularlos en FixedUpdate.
    private Vector3 camForwardFlat;
    private Vector3 camRightFlat;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Congelar rotaciones físicas para que solo rote por código
        rb.freezeRotation = true;

        if (isometricCamera == null)
            isometricCamera = Camera.main;
    }

    // ── Input: callbacks del Input System ───────────────────────────

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValue<Vector2>();
    }

    // ── Input: métodos directos para joysticks virtuales ─────────────

    public void SetMoveInput(Vector2 value) => moveInput = value;
    public void SetAimInput(Vector2 value) => aimInput = value;

    // ── Ciclo de juego ──────────────────────────────────────────────

    private void Update()
    {
        CacheIsometricAxes();
        HandleRotation();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    // ── Conversión isométrica ───────────────────────────────────────

    /// <summary>
    /// Proyecta los ejes forward y right de la cámara sobre el plano XZ (Y=0)
    /// y los normaliza. Así el input del joystick se traduce al espacio visual
    /// de la cámara isométrica sin importar su ángulo de rotación.
    /// </summary>
    private void CacheIsometricAxes()
    {
        Transform camTransform = isometricCamera.transform;

        // Tomar forward/right de la cámara y aplastarlos en Y
        camForwardFlat = camTransform.forward;
        camForwardFlat.y = 0f;
        camForwardFlat.Normalize();

        camRightFlat = camTransform.right;
        camRightFlat.y = 0f;
        camRightFlat.Normalize();
    }

    /// <summary>
    /// Convierte un Vector2 de joystick a un Vector3 en el plano XZ
    /// relativo a la orientación de la cámara isométrica.
    /// joystick.x → eje right de cámara, joystick.y → eje forward de cámara.
    /// </summary>
    private Vector3 InputToIsometricDirection(Vector2 input)
    {
        return camRightFlat * input.x + camForwardFlat * input.y;
    }

    // ── Movimiento ──────────────────────────────────────────────────

    private void HandleMovement()
    {
        Vector3 desiredVelocity = InputToIsometricDirection(moveInput) * moveSpeed;

        // Interpolar la velocidad horizontal para un arranque/frenado suaves
        Vector3 currentVel = rb.linearVelocity;
        Vector3 smoothed = Vector3.MoveTowards(
            new Vector3(currentVel.x, 0f, currentVel.z),
            desiredVelocity,
            acceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector3(smoothed.x, currentVel.y, smoothed.z);
    }

    // ── Rotación twin-stick ─────────────────────────────────────────

    private void HandleRotation()
    {
        if (aimInput.sqrMagnitude < 0.01f) return;

        // Convertir el input de apuntado al mismo espacio isométrico
        Vector3 aimDirection = InputToIsometricDirection(aimInput);
        Quaternion targetRotation = Quaternion.LookRotation(aimDirection, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
