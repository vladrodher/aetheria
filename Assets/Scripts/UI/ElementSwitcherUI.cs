using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI de slots de elementos equipados. Muestra hasta N botones con el símbolo
/// y color de cada elemento. El slot activo se resalta. Tocar un slot lo activa.
/// Integrado con swipe horizontal para cambio rápido en combate.
/// </summary>
public class ElementSwitcherUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ElementInventory inventory;

    [Header("Slot Template")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotsParent;

    [Header("Visuales")]
    [SerializeField] private Color activeSlotColor = Color.white;
    [SerializeField] private Color inactiveSlotColor = new(0.4f, 0.4f, 0.4f, 0.6f);
    [SerializeField] private float activeScale = 1.2f;
    [SerializeField] private float scaleSpeed = 8f;

    private SlotUI[] slots;

    private struct SlotUI
    {
        public RectTransform Root;
        public Image Background;
        public TextMeshProUGUI SymbolText;
        public Button Button;
        public float TargetScale;
    }

    private void Start()
    {
        BuildSlots();

        if (inventory != null)
        {
            inventory.OnSlotChanged += HandleSlotChanged;
            inventory.OnActiveSlotChanged += HandleActiveSlotChanged;

            for (int i = 0; i < inventory.EquippedSlots.Count; i++)
                HandleSlotChanged(i, inventory.EquippedSlots[i]);

            HandleActiveSlotChanged(inventory.ActiveSlotIndex);
        }
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnSlotChanged -= HandleSlotChanged;
            inventory.OnActiveSlotChanged -= HandleActiveSlotChanged;
        }
    }

    private void BuildSlots()
    {
        if (slotPrefab == null || slotsParent == null || inventory == null) return;

        slots = new SlotUI[inventory.MaxSlots];

        for (int i = 0; i < inventory.MaxSlots; i++)
        {
            var go = Instantiate(slotPrefab, slotsParent);
            go.SetActive(true);

            int slotIndex = i;
            var slot = new SlotUI
            {
                Root = go.GetComponent<RectTransform>(),
                Background = go.GetComponent<Image>(),
                SymbolText = go.GetComponentInChildren<TextMeshProUGUI>(),
                Button = go.GetComponent<Button>(),
                TargetScale = 1f
            };

            if (slot.Button != null)
                slot.Button.onClick.AddListener(() => inventory.SwitchToSlot(slotIndex));

            if (slot.SymbolText != null)
                slot.SymbolText.text = "";

            if (slot.Background != null)
                slot.Background.color = inactiveSlotColor;

            slots[i] = slot;
        }
    }

    private void HandleSlotChanged(int index, ElementData element)
    {
        if (slots == null || index < 0 || index >= slots.Length) return;

        ref var slot = ref slots[index];

        if (element != null)
        {
            if (slot.SymbolText != null)
            {
                slot.SymbolText.text = element.Symbol;
                slot.SymbolText.color = element.PrimaryColor;
            }
        }
        else
        {
            if (slot.SymbolText != null)
            {
                slot.SymbolText.text = "";
                slot.SymbolText.color = Color.white;
            }
        }
    }

    private void HandleActiveSlotChanged(int activeIndex)
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            bool isActive = i == activeIndex;
            ref var slot = ref slots[i];

            slot.TargetScale = isActive ? activeScale : 1f;

            if (slot.Background != null)
            {
                ElementData element = i < inventory.EquippedSlots.Count
                    ? inventory.EquippedSlots[i]
                    : null;

                if (isActive && element != null)
                {
                    Color bg = element.PrimaryColor;
                    bg.a = 0.5f;
                    slot.Background.color = bg;
                }
                else
                {
                    slot.Background.color = inactiveSlotColor;
                }
            }
        }
    }

    private void Update()
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            ref var slot = ref slots[i];
            if (slot.Root == null) continue;

            float current = slot.Root.localScale.x;
            float target = slot.TargetScale;

            if (!Mathf.Approximately(current, target))
            {
                float s = Mathf.Lerp(current, target, scaleSpeed * Time.deltaTime);
                slot.Root.localScale = Vector3.one * s;
            }
        }
    }
}
