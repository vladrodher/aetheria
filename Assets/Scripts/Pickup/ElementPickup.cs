using UnityEngine;

/// <summary>
/// Item recolectable que otorga un elemento al jugador al contacto.
/// Flota, rota, y tiene un efecto de atracción magnética cuando el jugador está cerca.
/// Gestionado por un pool simple o Destroy con delay.
/// </summary>
public class ElementPickup : MonoBehaviour
{
    [Header("Datos")]
    [SerializeField] private ElementData element;

    [Header("Comportamiento")]
    [SerializeField] private float magnetRange = 3f;
    [SerializeField] private float magnetSpeed = 12f;
    [SerializeField] private float pickupRange = 0.8f;
    [SerializeField] private float lifetime = 15f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Visuales")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float rotateSpeed = 90f;

    private Transform playerTransform;
    private Vector3 startPosition;
    private float spawnTime;
    private bool collected;

    private MaterialPropertyBlock propertyBlock;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    public ElementData Element => element;

    public void Initialize(ElementData elementData, Transform player)
    {
        element = elementData;
        playerTransform = player;
        startPosition = transform.position;
        spawnTime = Time.time;
        collected = false;

        ApplyVisuals();
    }

    private void Start()
    {
        if (startPosition == Vector3.zero)
            startPosition = transform.position;

        if (playerTransform == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        propertyBlock = new MaterialPropertyBlock();
        ApplyVisuals();
    }

    private void Update()
    {
        if (collected) return;

        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
            return;
        }

        Animate();

        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance < pickupRange)
        {
            Collect();
            return;
        }

        if (distance < magnetRange)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            float speed = magnetSpeed * (1f - distance / magnetRange);
            transform.position += direction * (speed * Time.deltaTime);
            startPosition = transform.position - Vector3.up * GetBobOffset();
        }
    }

    private void Animate()
    {
        float bob = GetBobOffset();
        transform.position = new Vector3(
            startPosition.x, startPosition.y + bob, startPosition.z);

        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        // Parpadeo cuando está por expirar
        float remaining = lifetime - (Time.time - spawnTime);
        if (remaining < 3f && bodyRenderer != null)
        {
            float alpha = Mathf.PingPong(Time.time * 5f, 1f) * 0.5f + 0.5f;
            bodyRenderer.GetPropertyBlock(propertyBlock);
            Color color = element != null ? element.PrimaryColor : Color.white;
            propertyBlock.SetColor(EmissionColorId, color * alpha * 2f);
            bodyRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    private float GetBobOffset()
    {
        return Mathf.Sin((Time.time - spawnTime) * bobSpeed) * bobHeight;
    }

    private void Collect()
    {
        collected = true;

        if (playerTransform != null
            && playerTransform.TryGetComponent(out ElementInventory inventory))
        {
            inventory.ForceAddElement(element);
        }

        Destroy(gameObject);
    }

    private void ApplyVisuals()
    {
        if (bodyRenderer == null || element == null) return;
        if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();

        bodyRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(BaseColorId, element.PrimaryColor);
        propertyBlock.SetColor(EmissionColorId, element.PrimaryColor * 1.5f);
        bodyRenderer.SetPropertyBlock(propertyBlock);
    }
}
