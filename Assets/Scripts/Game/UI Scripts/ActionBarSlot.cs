using UnityEngine;
using UnityEngine.UI;

public class ActionBarSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay;

    public SpellDefinition AssignedSpell { get; private set; }

    public bool IsEmpty => AssignedSpell == null;

    public void SetSpell(SpellDefinition spell)
    {
        AssignedSpell = spell;

        Debug.Log("Setting slot spell to: " + spell.spellName);

        Refresh();
        SetCooldown(0.75f);
    }

    public void Clear()
    {
        AssignedSpell = null;
        Refresh();
        SetCooldown(0.75f);
    }

    public void SetCooldown(float fillAmount)
    {
        if (cooldownOverlay == null) return;

        cooldownOverlay.fillAmount = fillAmount;
        cooldownOverlay.gameObject.SetActive(fillAmount > 0f);
    }

    private void Refresh()
    {
        if (iconImage == null)
        {
            Debug.LogError(name + " has no Icon Image assigned.");
            return;
        }

        if (AssignedSpell == null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            return;
        }

        Debug.Log("Icon sprite is: " + AssignedSpell.icon);

        iconImage.sprite = AssignedSpell.icon;
        iconImage.color = Color.white;
        iconImage.enabled = true;
        iconImage.gameObject.SetActive(true);
    }
}