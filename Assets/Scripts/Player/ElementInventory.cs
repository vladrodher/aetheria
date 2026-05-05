using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inventario de elementos del jugador. Almacena elementos descubiertos,
/// gestiona slots rápidos para cambiar en combate, y conecta con WeaponController.
/// Máximo de slots configurables (ej: 3 slots = 3 elementos disponibles al mismo tiempo).
/// </summary>
public class ElementInventory : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int maxSlots = 3;
    [SerializeField] private WeaponController weaponController;

    [Header("Slots iniciales")]
    [SerializeField] private ElementData[] startingElements;

    private readonly List<ElementData> equippedSlots = new();
    private readonly HashSet<int> discoveredElements = new();
    private int activeSlotIndex;

    // ── Eventos ──────────────────────────────────────────────────────

    public event Action<int, ElementData> OnSlotChanged;
    public event Action<int> OnActiveSlotChanged;
    public event Action<ElementData> OnElementDiscovered;

    // ── Propiedades ──────────────────────────────────────────────────

    public int MaxSlots => maxSlots;
    public int ActiveSlotIndex => activeSlotIndex;
    public IReadOnlyList<ElementData> EquippedSlots => equippedSlots;
    public int DiscoveredCount => discoveredElements.Count;

    public ElementData ActiveElement =>
        activeSlotIndex >= 0 && activeSlotIndex < equippedSlots.Count
            ? equippedSlots[activeSlotIndex]
            : null;

    // ── Inicialización ───────────────────────────────────────────────

    private void Awake()
    {
        if (weaponController == null)
            weaponController = GetComponent<WeaponController>();
    }

    private void Start()
    {
        if (startingElements != null)
        {
            foreach (var element in startingElements)
            {
                if (element != null)
                    AddElement(element);
            }
        }

        if (equippedSlots.Count > 0)
            SwitchToSlot(0);
    }

    // ── Agregar elementos ────────────────────────────────────────────

    /// <summary>
    /// Agrega un elemento al inventario. Si hay slot libre, lo equipa automáticamente.
    /// Si no, lo marca como descubierto para acceso desde el menú de tabla periódica.
    /// Retorna true si se equipó en un slot, false si solo se descubrió.
    /// </summary>
    public bool AddElement(ElementData element)
    {
        if (element == null) return false;

        DiscoverElement(element);

        if (IsInSlots(element)) return false;

        if (equippedSlots.Count < maxSlots)
        {
            int index = equippedSlots.Count;
            equippedSlots.Add(element);
            OnSlotChanged?.Invoke(index, element);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Reemplaza el elemento del slot especificado con uno nuevo.
    /// Útil para el menú de tabla periódica o al recoger con slots llenos.
    /// </summary>
    public void SetSlot(int slotIndex, ElementData element)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots) return;
        if (element == null) return;

        DiscoverElement(element);

        while (equippedSlots.Count <= slotIndex)
            equippedSlots.Add(null);

        equippedSlots[slotIndex] = element;
        OnSlotChanged?.Invoke(slotIndex, element);

        if (slotIndex == activeSlotIndex)
            weaponController.EquipElement(element);
    }

    /// <summary>
    /// Agrega un elemento. Si los slots están llenos, reemplaza el slot activo.
    /// Comportamiento típico de pickup en gameplay rápido.
    /// </summary>
    public void ForceAddElement(ElementData element)
    {
        if (element == null) return;

        if (!AddElement(element))
            SetSlot(activeSlotIndex, element);
    }

    // ── Cambio de slot activo ────────────────────────────────────────

    public void SwitchToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSlots.Count) return;
        if (equippedSlots[slotIndex] == null) return;

        activeSlotIndex = slotIndex;
        weaponController.EquipElement(equippedSlots[slotIndex]);
        OnActiveSlotChanged?.Invoke(slotIndex);
    }

    public void SwitchToNext()
    {
        if (equippedSlots.Count <= 1) return;

        int next = (activeSlotIndex + 1) % equippedSlots.Count;
        SwitchToSlot(next);
    }

    public void SwitchToPrevious()
    {
        if (equippedSlots.Count <= 1) return;

        int prev = (activeSlotIndex - 1 + equippedSlots.Count) % equippedSlots.Count;
        SwitchToSlot(prev);
    }

    // ── Descubrimiento ───────────────────────────────────────────────

    public void DiscoverElement(ElementData element)
    {
        if (element == null) return;

        if (discoveredElements.Add(element.AtomicNumber))
            OnElementDiscovered?.Invoke(element);
    }

    public bool IsDiscovered(ElementData element)
    {
        return element != null && discoveredElements.Contains(element.AtomicNumber);
    }

    public bool IsInSlots(ElementData element)
    {
        foreach (var slot in equippedSlots)
        {
            if (slot != null && slot.AtomicNumber == element.AtomicNumber)
                return true;
        }
        return false;
    }
}
