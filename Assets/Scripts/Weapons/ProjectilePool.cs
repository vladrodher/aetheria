using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object pool de proyectiles optimizado para móvil.
/// Evita Instantiate/Destroy durante gameplay. Se expande automáticamente si es necesario.
/// </summary>
public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private Projectile prefab;
    [SerializeField] private int initialSize = 40;

    private readonly Queue<Projectile> available = new();

    public static ProjectilePool Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        Prewarm();
    }

    private void Prewarm()
    {
        for (int i = 0; i < initialSize; i++)
            available.Enqueue(CreateInstance());
    }

    private Projectile CreateInstance()
    {
        var proj = Instantiate(prefab, transform);
        proj.gameObject.SetActive(false);
        proj.SetPool(this);
        return proj;
    }

    public Projectile Get(Vector3 position, Quaternion rotation)
    {
        var proj = available.Count > 0 ? available.Dequeue() : CreateInstance();

        proj.transform.SetPositionAndRotation(position, rotation);
        proj.gameObject.SetActive(true);
        return proj;
    }

    public void Return(Projectile proj)
    {
        proj.gameObject.SetActive(false);
        available.Enqueue(proj);
    }
}
