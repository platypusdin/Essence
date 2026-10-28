using UnityEngine;

public enum AbilityTarget
{
    Enemy,
    Corpse,
    Self
}

public enum AbilityExecutionResult
{
    Failed,
    Completed,
    Pending
}

public enum AbilityActivationType
{
    Active,
    Passive,
    Reaction
}

public enum DamageType
{
    Physical,
    Magical
}


public abstract class Ability : ScriptableObject
{
     [Header("Display")]
    public string abilityName;
    public Sprite icon;

    [Header("Activation")]
    public AbilityActivationType activationType;

    [Header("Usage")]
    [Min(0)] public int apCost;
    
    [Header("Targeting")]
    public CombatAttackType attackType = CombatAttackType.Other;
    public AbilityTarget target;
    [Min(0)] public int cooldownTurns;

    public virtual int AISelectionPriority => 0;

    public abstract AbilityExecutionResult Execute(
        GameObject caster,
        GameObject targetObject);
}
