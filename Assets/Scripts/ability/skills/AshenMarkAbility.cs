using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Abilities/Ashen Mark")]
public class AshenMarkAbility : Ability
{
    [Header("Mark")]
    [SerializeField] private MarkStatusData markStatus;

    [SerializeField, Min(1)] private int durationStacks = 2;

    private void OnValidate()
    {
        activationType = AbilityActivationType.Active;
        target = AbilityTarget.Enemy;
    }

    public override AbilityExecutionResult Execute(
        GameObject caster,
        GameObject targetObject)
    {
        if (caster == null ||
            targetObject == null ||
            markStatus == null ||
            durationStacks <= 0)
        {
            return AbilityExecutionResult.Failed;
        }

        Enemy enemy = targetObject.GetComponent<Enemy>();

        if (enemy == null || enemy.IsDead)
            return AbilityExecutionResult.Failed;

        StatusController statusController =
            targetObject.GetComponent<StatusController>();

        if (statusController == null)
            return AbilityExecutionResult.Failed;

        bool applied = statusController.ApplyStatus(
            markStatus,
            durationStacks
        );

        if (!applied)
            return AbilityExecutionResult.Failed;

        CombatLog combatLog =
            FindFirstObjectByType<CombatLog>();

        combatLog?.AddLog(
            $"{caster.name} marks {targetObject.name} with Ashen Mark"
        );

        return AbilityExecutionResult.Completed;
    }
}