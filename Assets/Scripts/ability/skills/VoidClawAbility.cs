using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Abilities/Void Claw")]
public class VoidClawAbility : Ability
{
    [Header("Damage")]
    [SerializeField, Min(1)] private int damage = 5;

    [Header("Instability")]
    [SerializeField] private InstabilityStatusData instabilityStatus;
    [SerializeField, Min(1)] private int instabilityAmount = 20;

    private void OnValidate()
    {
        target = AbilityTarget.Enemy;
        activationType = AbilityActivationType.Active;
    }

    public override AbilityExecutionResult Execute(
        GameObject caster,
        GameObject targetObject)
    {
        if (caster == null ||
            targetObject == null ||
            instabilityStatus == null)
        {
            return AbilityExecutionResult.Failed;
        }

        StatusController casterStatusController =
            caster.GetComponent<StatusController>();

        if (casterStatusController == null)
        {
            Debug.LogError(
                "VoidClawAbility: StatusController is missing on caster."
            );

            return AbilityExecutionResult.Failed;
        }

        bool success = CombatDamageService.TryApplyDamage(
            caster,
            targetObject,
            damage,
            abilityName,
            out _,
            out _
        );

        if (!success)
            return AbilityExecutionResult.Failed;

        casterStatusController.AddInstability(
            instabilityStatus,
            instabilityAmount
        );

        return AbilityExecutionResult.Completed;
    }
}