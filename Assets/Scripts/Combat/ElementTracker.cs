using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rastrea qué elementos han impactado recientemente a esta entidad.
/// Cada marca elemental persiste durante una ventana de tiempo configurable.
/// Cuando un nuevo elemento llega, ReactionManager revisa los pares para reacciones.
/// </summary>
public class ElementTracker : MonoBehaviour
{
    [SerializeField] private float elementDecayTime = 3f;

    // Elemento → timestamp del último impacto
    private readonly Dictionary<ElementData, float> trackedElements = new();
    private readonly List<ElementData> expiredKeys = new();

    public IReadOnlyDictionary<ElementData, float> TrackedElements => trackedElements;
    public float DecayTime => elementDecayTime;

    /// <summary>
    /// Registra un nuevo impacto elemental. Si el elemento ya está trackeado,
    /// refresca su timestamp. Retorna la lista de elementos activos previos
    /// contra los cuales se debe chequear reacciones.
    /// </summary>
    public void RegisterElement(ElementData element, out List<ElementData> activeElements)
    {
        CleanupExpired();

        activeElements = new List<ElementData>(trackedElements.Count);
        foreach (var kvp in trackedElements)
        {
            if (kvp.Key != element)
                activeElements.Add(kvp.Key);
        }

        trackedElements[element] = Time.time;
    }

    /// <summary>
    /// Consume (elimina) los dos elementos de una reacción que acaba de dispararse.
    /// Evita que el mismo par reaccione múltiples veces sin volver a impactar.
    /// </summary>
    public void ConsumeReactionPair(ElementData a, ElementData b)
    {
        trackedElements.Remove(a);
        trackedElements.Remove(b);
    }

    public bool HasElement(ElementData element)
    {
        if (!trackedElements.TryGetValue(element, out float timestamp))
            return false;

        return Time.time - timestamp < elementDecayTime;
    }

    public void ClearAll()
    {
        trackedElements.Clear();
    }

    private void CleanupExpired()
    {
        expiredKeys.Clear();

        foreach (var kvp in trackedElements)
        {
            if (Time.time - kvp.Value >= elementDecayTime)
                expiredKeys.Add(kvp.Key);
        }

        foreach (var key in expiredKeys)
            trackedElements.Remove(key);
    }
}
