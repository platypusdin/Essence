using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Abilities/Guard")]
public class GuardAbility : Ability
{
    [SerializeField] private BarrierStatusData barrierStatus;
    [SerializeField, Min(0)] private int barrierAmount = 8;

    private void OnValidate()
    {
        target = AbilityTarget.Self;
    }

    public override AbilityExecutionResult Execute(
        GameObject caster,
        GameObject targetObject)
    {
        if (caster == null || targetObject == null ||
            barrierStatus == null)
        {
            return AbilityExecutionResult.Failed;
        }

        StatusController statusController =
            caster.GetComponent<StatusController>();

        if (statusController == null)
        {
            Debug.LogError(
                "GuardAbility: StatusController is missing on caster."
            );

            return AbilityExecutionResult.Failed;
        }

        int gained = statusController.AddBarrier(
            barrierStatus,
            barrierAmount
        );

        CombatLog combatLog = FindFirstObjectByType<CombatLog>();

        combatLog?.AddLog(
            $"{caster.name} uses {abilityName} and gains {gained} barrier"
        );

        return AbilityExecutionResult.Completed;
    }
}