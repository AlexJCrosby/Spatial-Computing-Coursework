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
        Refresh();
    }

    public void Clear()
    {
        AssignedSpell = null;
        Refresh();
    }

    public void SetCooldown(float fillAmount)
    {
        if (cooldownOverlay == null) return;

        cooldownOverlay.fillAmount = fillAmount;
        cooldownOverlay.gameObject.SetActive(fillAmount > 0f);
    }

    private void Refresh()
    {
        if (iconImage == null) return;

        if (AssignedSpell == null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
        else
        {
            iconImage.sprite = AssignedSpell.icon;
            iconImage.enabled = AssignedSpell.icon != null;
        }
    }
}