using UnityEngine;

public sealed class CombatTargetingService
{
    private readonly Battlefield battlefield;

    public CombatTargetingService(Battlefield battlefield)
    {
        this.battlefield = battlefield;
    }

    public bool CanTarget(
        GameObject attacker,
        GameObject target,
        CombatAttackType attackType)
    {
        if (attacker == null ||
            target == null ||
            attacker == target ||
            battlefield == null)
        {
            return false;
        }

        if (!battlefield.TryFindRank(attacker, out BattleRank attackerRank) ||
            !battlefield.TryFindRank(target, out BattleRank targetRank))
        {
            return false;
        }

        if (attackerRank.Side == targetRank.Side)
            return false;

        switch (attackType)
        {
            case CombatAttackType.Melee:
                return CanUseMelee(attackerRank, targetRank);

            case CombatAttackType.Ranged:
                return true;

            default:
                return false;
        }
    }

    private bool CanUseMelee(
        BattleRank attackerRank,
        BattleRank targetRank)
    {
        int distance =
            attackerRank.Index + targetRank.Index - 1;

        if (distance > 4)
            return false;

        return !HasLivingUnitAhead(targetRank);
    }

    private bool HasLivingUnitAhead(BattleRank rank)
    {
        for (int index = 1; index < rank.Index; index++)
        {
            var frontRank = new BattleRank(rank.Side, index);

            if (IsLivingOccupant(frontRank))
                return true;
        }

        return false;
    }

    private bool IsLivingOccupant(BattleRank rank)
    {
        GameObject occupant = battlefield.GetOccupant(rank);

        if (occupant == null)
            return false;

        IDamageable damageable =
            occupant.GetComponent<IDamageable>();

        return damageable != null && !damageable.IsDead;
    }
}