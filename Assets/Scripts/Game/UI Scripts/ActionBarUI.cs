using UnityEngine;

public class ActionBarUI : MonoBehaviour
{
    [System.Serializable]
    public class StartingSlot
    {
        public int slotIndex;
        public SpellDefinition spell;
    }

    [Header("Casting")]
    [SerializeField] private SpellCastingController spellCastingController;

    [Header("Slots")]
    [SerializeField] private ActionBarSlot[] slots;

    [Header("Starting Layout")]
    [SerializeField] private StartingSlot[] startingSlots;

    private void Start()
    {
        ApplyStartingLayout();
    }
    
    private void Update()
    {
        UpdateCooldowns();
    }

    private void UpdateCooldowns()
    {
        if (spellCastingController == null) return;

        foreach (ActionBarSlot slot in slots)
        {
            if (slot == null) continue;

            float cooldownFill =
                spellCastingController.GetCooldownFillAmount(slot.AssignedSpell);

            slot.SetCooldown(cooldownFill);
        }
    }

    private void ApplyStartingLayout()
    {
        ClearAllSlots();

        foreach (StartingSlot startingSlot in startingSlots)
        {
            if (startingSlot.slotIndex < 0 || startingSlot.slotIndex >= slots.Length)
            {
                Debug.LogWarning("Invalid action bar slot index: " + startingSlot.slotIndex);
                continue;
            }
            Debug.Log("Applying starting action bar layout.");
            slots[startingSlot.slotIndex].SetSpell(startingSlot.spell);
        }
    }

    public SpellDefinition GetSpellInSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return null;
        return slots[index].AssignedSpell;
    }

    public void SetSpellInSlot(int index, SpellDefinition spell)
    {
        if (index < 0 || index >= slots.Length) return;

        slots[index].SetSpell(spell);
    }

    public void ClearSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        slots[index].Clear();
    }

    private void ClearAllSlots()
    {
        foreach (ActionBarSlot slot in slots)
        {
            if (slot != null)
            {
                slot.Clear();
            }
        }
    }
}