using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Core Abilities/Neurotoxin")]
public class NeurotoxinCoreAbility : CoreAbility, IPassiveAbility
{
    [Header("Neurotoxin")]
    [SerializeField, Min(1)] private int poisonThreshold = 4;
    [SerializeField, Min(0)] private int speedReduction = 2;

    public override AbilityExecutionResult Execute(
        GameObject caster,
        GameObject targetObject)
    {
        // Пассивная способность не активируется кнопкой.
        return AbilityExecutionResult.Failed;
    }

    public void Recalculate(
        GameObject owner,
        PassiveAbilityContext context)
    {
        foreach (CombatParticipant target in context.Participants)
        {
            if (target == null ||
                target.gameObject == owner ||
                !AreOpponents(owner, target))
            {
                continue;
            }

            int poisonStacks = GetPoisonStacks(target);

            int modifier = poisonStacks >= poisonThreshold
                ? -speedReduction
                : 0;

            context.SetSpeedModifier(
                owner,
                this,
                target,
                modifier);
        }
    }

    private int GetPoisonStacks(CombatParticipant target)
    {
        StatusController statuses =
            target.GetComponent<StatusController>();

        if (statuses == null)
            return 0;

        int totalStacks = 0;

        foreach (StatusInstance status in statuses.ActiveStatuses)
        {
            if (status.Data is PoisonStatusData)
                totalStacks += status.RemainingStacks ?? 0;
        }

        return totalStacks;
    }

    private bool AreOpponents(
        GameObject owner,
        CombatParticipant target)
    {
        bool ownerIsPlayer = owner.GetComponent<PlayerStats>() != null;
        bool ownerIsEnemy = owner.GetComponent<Enemy>() != null;

        bool targetIsPlayer =
            target.GetComponent<PlayerStats>() != null;

        bool targetIsEnemy =
            target.GetComponent<Enemy>() != null;

        bool targetIsAlly =
            target.GetComponent<SummonedAlly>() != null;

        if (ownerIsPlayer)
            return targetIsEnemy;

        if (ownerIsEnemy)
            return targetIsPlayer || targetIsAlly;

        return false;
    }
}