using UnityEngine;
using TMPro;

/// <summary>
/// Popup flotante que aparece en la posición de una reacción elemental.
/// Muestra el nombre de la reacción, sube flotando, y se desvanece.
/// Se instancia desde un pool simple por el HUDManager.
/// </summary>
public class ReactionPopupUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI reactionText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float floatSpeed = 80f;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float fadeStartPercent = 0.4f;

    private RectTransform rectTransform;
    private float spawnTime;
    private Camera mainCamera;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Show(string text, Color color, Vector3 worldPosition, Camera cam)
    {
        mainCamera = cam;
        spawnTime = Time.time;

        if (reactionText != null)
        {
            reactionText.text = text;
            reactionText.color = color;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        UpdateScreenPosition(worldPosition);
        gameObject.SetActive(true);
    }

    private void Update()
    {
        float elapsed = Time.time - spawnTime;
        float t = elapsed / lifetime;

        if (t >= 1f)
        {
            gameObject.SetActive(false);
            return;
        }

        rectTransform.anchoredPosition += Vector2.up * (floatSpeed * Time.deltaTime);

        float fadeStart = 1f - fadeStartPercent;
        if (canvasGroup != null && t > fadeStart)
        {
            float fadeT = (t - fadeStart) / fadeStartPercent;
            canvasGroup.alpha = 1f - fadeT;
        }

        float scale = t < 0.1f ? Mathf.Lerp(0.5f, 1f, t / 0.1f) : 1f;
        rectTransform.localScale = Vector3.one * scale;
    }

    private void UpdateScreenPosition(Vector3 worldPosition)
    {
        if (mainCamera == null) return;
        Vector2 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
        rectTransform.position = screenPos;
    }
}
