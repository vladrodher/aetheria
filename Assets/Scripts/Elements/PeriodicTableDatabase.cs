using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base de datos central de la tabla periódica.
/// Contiene todos los elementos y reacciones, y expone métodos de búsqueda O(1).
/// Crear un solo asset de este tipo: Assets/Data/PeriodicTable.asset
/// </summary>
[CreateAssetMenu(fileName = "PeriodicTable", menuName = "Aetheria/Periodic Table Database")]
public class PeriodicTableDatabase : ScriptableObject
{
    [Header("Datos")]
    [SerializeField] private ElementData[] elements;
    [SerializeField] private ElementReaction[] reactions;

    // Diccionarios de lookup, se construyen al cargar el asset
    private Dictionary<int, ElementData> byAtomicNumber;
    private Dictionary<string, ElementData> bySymbol;
    private Dictionary<long, ElementReaction> reactionMap;
    private Dictionary<ElementCategory, List<ElementData>> byCategory;

    public IReadOnlyList<ElementData> AllElements => elements;
    public IReadOnlyList<ElementReaction> AllReactions => reactions;

    // ── Inicialización ───────────────────────────────────────────────

    private void OnEnable()
    {
        BuildLookups();
    }

    private void BuildLookups()
    {
        byAtomicNumber = new Dictionary<int, ElementData>(elements.Length);
        bySymbol = new Dictionary<string, ElementData>(elements.Length);
        byCategory = new Dictionary<ElementCategory, List<ElementData>>();
        reactionMap = new Dictionary<long, ElementReaction>();

        foreach (var el in elements)
        {
            if (el == null) continue;

            byAtomicNumber[el.AtomicNumber] = el;
            bySymbol[el.Symbol] = el;

            if (!byCategory.TryGetValue(el.Category, out var list))
            {
                list = new List<ElementData>();
                byCategory[el.Category] = list;
            }
            list.Add(el);
        }

        foreach (var rx in reactions)
        {
            if (rx == null || rx.ElementA == null || rx.ElementB == null) continue;
            reactionMap[rx.ReactionKey] = rx;
        }
    }

    // ── Búsquedas de elementos ───────────────────────────────────────

    public ElementData GetElement(int atomicNumber)
    {
        EnsureLookups();
        byAtomicNumber.TryGetValue(atomicNumber, out var result);
        return result;
    }

    public ElementData GetElement(string symbol)
    {
        EnsureLookups();
        bySymbol.TryGetValue(symbol, out var result);
        return result;
    }

    public IReadOnlyList<ElementData> GetElementsByCategory(ElementCategory category)
    {
        EnsureLookups();
        return byCategory.TryGetValue(category, out var list)
            ? list
            : new List<ElementData>();
    }

    // ── Búsquedas de reacciones ──────────────────────────────────────

    /// <summary>
    /// Busca la reacción entre dos elementos (el orden no importa).
    /// Retorna null si no existe reacción definida entre ellos.
    /// </summary>
    public ElementReaction GetReaction(ElementData a, ElementData b)
    {
        if (a == null || b == null) return null;
        EnsureLookups();

        long key = ElementReaction.GetReactionKey(a, b);
        reactionMap.TryGetValue(key, out var result);
        return result;
    }

    /// <summary>
    /// Retorna todas las reacciones en las que participa un elemento.
    /// </summary>
    public List<ElementReaction> GetReactionsFor(ElementData element)
    {
        EnsureLookups();
        var result = new List<ElementReaction>();

        foreach (var rx in reactions)
        {
            if (rx == null) continue;
            if (rx.ElementA == element || rx.ElementB == element)
                result.Add(rx);
        }

        return result;
    }

    /// <summary>
    /// Verifica si dos elementos pueden reaccionar entre sí.
    /// </summary>
    public bool CanReact(ElementData a, ElementData b)
    {
        return GetReaction(a, b) != null;
    }

    // ── Internos ─────────────────────────────────────────────────────

    private void EnsureLookups()
    {
        if (byAtomicNumber == null || byAtomicNumber.Count == 0)
            BuildLookups();
    }
}
