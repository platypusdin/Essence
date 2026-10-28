using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Abilities/Damage")]
public class DamageAbility : Ability
{
    [Header("Damage")]
    [Min(0)] public int power = 10;

    private void OnValidate()
    {
        target = AbilityTarget.Enemy;
    }

    public override AbilityExecutionResult Execute(
        GameObject caster,
        GameObject targetObject)
    {
        bool success = CombatDamageService.TryApplyDamage(
            caster,
            targetObject,
            power,
            abilityName,
            out _,
            out _
        );

        return success
            ? AbilityExecutionResult.Completed
            : AbilityExecutionResult.Failed;
    }
}