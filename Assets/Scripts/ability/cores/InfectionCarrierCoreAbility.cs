using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Combat/Core Abilities/Infection Carrier")] 
public sealed class InfectionCarrierCoreAbility :
    CoreAbility, IPassiveAbility, ICombatEventReaction
{
    public void Recalculate(
        GameObject owner,
        PassiveAbilityContext context)
    {
        // Постоянных модификаторов нет.
    }

    public override AbilityExecutionResult Execute(
        GameObject caster,
        GameObject targetObject)
    {
        // Способность срабатывает автоматически.
        return AbilityExecutionResult.Failed;
    }

    public void OnCombatEvent(
        GameObject owner,
        CombatEvent combatEvent)
    {
        if (owner == null ||
            combatEvent == null ||
            activationType != AbilityActivationType.Passive ||
            combatEvent.Type != CombatEventType.StatusKilled ||
            combatEvent.Status is not PoisonStatusData poison)
        {
            return;
        }

        GameObject victim = combatEvent.Target;

        if (victim == null)
            return;

        int transferredStacks = combatEvent.Value / 2;

        if (transferredStacks <= 0)
            return;

        Battlefield battlefield =
            Object.FindFirstObjectByType<Battlefield>();

        if (battlefield == null ||
            !battlefield.TryFindRank(victim, out BattleRank victimRank) ||
            !battlefield.TryFindRank(owner, out BattleRank ownerRank))
        {
            return;
        }

        // Способность срабатывает, только если погиб противник владельца.
        if (ownerRank.Side == victimRank.Side)
            return;

        GameObject target =
            FindNearestLivingOpponent(battlefield, victimRank);

        if (target == null)
            return;

        StatusController targetStatuses =
            target.GetComponent<StatusController>();

        if (targetStatuses == null)
            return;

        targetStatuses.ApplyStatus(poison, transferredStacks);

        CombatLog log =
            Object.FindFirstObjectByType<CombatLog>();

        log?.AddLog(
            $"Переносчик заразы: {target.name} получает " +
            $"{transferredStacks} стаков яда.");
    }

    private GameObject FindNearestLivingOpponent(
        Battlefield battlefield,
        BattleRank victimRank)
    {
        int nearestDistance = int.MaxValue;
        List<GameObject> candidates = new();

        for (int position = 1; position <= 4; position++)
        {
            BattleRank candidateRank =
                new(victimRank.Side, position);

            GameObject candidate =
                battlefield.GetOccupant(candidateRank);

            if (candidate == null || !IsAlive(candidate))
                continue;

            int distance = Mathf.Abs(position - victimRank.Index);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                candidates.Clear();
                candidates.Add(candidate);
            }
            else if (distance == nearestDistance)
            {
                candidates.Add(candidate);
            }
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    private bool IsAlive(GameObject target)
    {
        Enemy enemy = target.GetComponent<Enemy>();

        if (enemy != null)
            return !enemy.IsDead;

        PlayerStats player = target.GetComponent<PlayerStats>();

        if (player != null)
            return !player.IsDead;

        SummonedAlly ally = target.GetComponent<SummonedAlly>();

        if (ally != null)
            return !ally.IsDead;

        return false;
    }
}