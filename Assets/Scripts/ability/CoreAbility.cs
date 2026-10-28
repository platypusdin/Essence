using UnityEngine;

public abstract class CoreAbility : Ability
{
    [Header("Enemy AI")]
    [Min(0)]
    public int aiSelectionPriority = 5;

    public override int AISelectionPriority => aiSelectionPriority;
}