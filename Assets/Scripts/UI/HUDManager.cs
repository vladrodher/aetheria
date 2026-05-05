using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Punto central del HUD. Conecta los sistemas de gameplay con los widgets de UI.
/// Gestiona el pool de popups de reacción y coordina las referencias.
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("Referencias de sistemas")]
    [SerializeField] private HealthSystem playerHealth;
    [SerializeField] private WeaponController playerWeapon;
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Widgets")]
    [SerializeField] private HealthBarUI healthBar;
    [SerializeField] private ElementDisplayUI elementDisplay;
    [SerializeField] private WaveDisplayUI waveDisplay;

    [Header("Popups de reacción")]
    [SerializeField] private ReactionPopupUI popupPrefab;
    [SerializeField] private Transform popupParent;
    [SerializeField] private int popupPoolSize = 5;

    private Camera mainCamera;
    private readonly Queue<ReactionPopupUI> popupPool = new();

    private void Awake()
    {
        mainCamera = Camera.main;
        InitializePopupPool();
    }

    private void OnEnable()
    {
        if (ReactionManager.Instance != null)
            ReactionManager.Instance.OnReactionTriggered += HandleReaction;
    }

    private void OnDisable()
    {
        if (ReactionManager.Instance != null)
            ReactionManager.Instance.OnReactionTriggered -= HandleReaction;
    }

    private void Start()
    {
        // Reconectar si los widgets no tienen referencia (ej: asignación dinámica)
        if (healthBar != null && playerHealth != null)
            healthBar.SetTarget(playerHealth);
    }

    // ── Popups de reacción ───────────────────────────────────────────

    private void InitializePopupPool()
    {
        if (popupPrefab == null) return;

        Transform parent = popupParent != null ? popupParent : transform;

        for (int i = 0; i < popupPoolSize; i++)
        {
            var popup = Instantiate(popupPrefab, parent);
            popup.gameObject.SetActive(false);
            popupPool.Enqueue(popup);
        }
    }

    private void HandleReaction(ReactionEvent evt)
    {
        ShowReactionPopup(evt.Reaction.ReactionName, evt.Reaction.ReactionColor, evt.Position);
    }

    private void ShowReactionPopup(string text, Color color, Vector3 worldPos)
    {
        ReactionPopupUI popup = GetPopup();
        if (popup == null) return;

        popup.Show(text, color, worldPos, mainCamera);
    }

    private ReactionPopupUI GetPopup()
    {
        // Buscar uno inactivo en el pool
        for (int i = 0; i < popupPool.Count; i++)
        {
            var candidate = popupPool.Dequeue();
            popupPool.Enqueue(candidate);

            if (!candidate.gameObject.activeInHierarchy)
                return candidate;
        }

        // Pool lleno: reciclar el más antiguo
        if (popupPool.Count > 0)
        {
            var oldest = popupPool.Dequeue();
            oldest.gameObject.SetActive(false);
            popupPool.Enqueue(oldest);
            return oldest;
        }

        return null;
    }
}
