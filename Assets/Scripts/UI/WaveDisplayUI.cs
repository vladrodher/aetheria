using UnityEngine;
using TMPro;

/// <summary>
/// Muestra la oleada actual y la cuenta de enemigos vivos.
/// Animación de entrada al iniciar cada oleada.
/// </summary>
public class WaveDisplayUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI enemyCountText;

    [Header("Animación")]
    [SerializeField] private CanvasGroup waveAnnouncementGroup;
    [SerializeField] private float announcementDuration = 2f;
    [SerializeField] private float fadeSpeed = 3f;

    private float announcementTimer;

    private void OnEnable()
    {
        if (spawner == null) return;
        spawner.OnWaveStarted += HandleWaveStarted;
        spawner.OnWaveCompleted += HandleWaveCompleted;
    }

    private void OnDisable()
    {
        if (spawner == null) return;
        spawner.OnWaveStarted -= HandleWaveStarted;
        spawner.OnWaveCompleted -= HandleWaveCompleted;
    }

    private void HandleWaveStarted(int wave)
    {
        if (waveText != null)
            waveText.text = $"WAVE {wave}";

        if (waveAnnouncementGroup != null)
        {
            waveAnnouncementGroup.alpha = 1f;
            announcementTimer = announcementDuration;
        }
    }

    private void HandleWaveCompleted(int wave)
    {
        if (enemyCountText != null)
            enemyCountText.text = "WAVE CLEAR!";
    }

    private void Update()
    {
        if (spawner != null && enemyCountText != null)
        {
            int count = spawner.ActiveEnemyCount;
            if (count > 0)
                enemyCountText.text = $"x{count}";
        }

        if (waveAnnouncementGroup != null && announcementTimer > 0f)
        {
            announcementTimer -= Time.deltaTime;
            if (announcementTimer <= 0f)
                waveAnnouncementGroup.alpha = 0f;
            else if (announcementTimer < 1f)
                waveAnnouncementGroup.alpha = Mathf.MoveTowards(
                    waveAnnouncementGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
        }
    }
}
