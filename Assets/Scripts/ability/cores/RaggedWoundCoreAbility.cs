using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Core Abilities/Ragged Wound")]
public class RaggedWoundCoreAbility : CoreAbility
{
    [Header("Damage")]
    [SerializeField, Min(0)] private int damage = 7;
    [SerializeField] private DamageType damageType = DamageType.Physical;

    [Header("Bleeding")]
    [SerializeField] private BleedStatusData bleedStatus;
    [SerializeField, Min(1)] private int bleedStacks = 2;

    public override AbilityExecutionResult Execute(
        GameObject caster,
        GameObject targetObject)
    {
        if (caster == null || targetObject == null || bleedStatus == null)
            return AbilityExecutionResult.Failed;

        StatusController statuses =
            targetObject.GetComponent<StatusController>();

        if (statuses == null)
            return AbilityExecutionResult.Failed;

        if (!CombatDamageService.TryApplyDamage(
                caster,
                targetObject,
                damage,
                abilityName,
                out _,
                out _,
                attackType,
                damageType))
        {
            return AbilityExecutionResult.Failed;
        }

        statuses.ApplyStatus(bleedStatus, bleedStacks);

        return AbilityExecutionResult.Completed;
    }
}