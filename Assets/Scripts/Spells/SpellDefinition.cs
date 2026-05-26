using UnityEngine;

public enum SpellID
{
    Fireball,
    Frostbolt,
    Levitate,
    Knockback
}

[CreateAssetMenu(
    fileName = "New Spell Definition",
    menuName = "Spells/Spell Definition"
)]
public class SpellDefinition : ScriptableObject
{
    [Header("Identity")]
    public string spellName;
    public SpellID spellID;

    [Header("UI")]
    public Sprite icon;

    [TextArea]
    public string description;
}