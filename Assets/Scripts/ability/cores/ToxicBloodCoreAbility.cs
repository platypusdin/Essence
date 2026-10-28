using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Core Abilities/Toxic Blood")]
public sealed class ToxicBloodCoreAbility : CoreAbility, IReactionAbility
{
    [Header("Toxic Blood")]
    [SerializeField] private PoisonStatusData poisonStatus;

    public override AbilityExecutionResult Execute(
        GameObject caster,
        GameObject targetObject)
    {
        return AbilityExecutionResult.Failed;
    }

    public void React(
        GameObject owner,
        GameObject caster,
        GameObject target,
        CombatAttackType attackType,
        DamageType damageType,
        int damage,
        ReactionAbilityContext context)
    {
        if (owner == null ||
            caster == null ||
            target != owner ||
            damage <= 0 ||
            poisonStatus == null)
        {
            return;
        }

        if (owner.GetComponent<PlayerStats>() == null &&
            owner.GetComponent<Enemy>() == null)
        {
            return;
        }

        if (owner.GetComponent<PlayerStats>() is PlayerStats player &&
            player.IsDead)
        {
            return;
        }

        if (owner.GetComponent<Enemy>() is Enemy enemy &&
            enemy.IsDead)
        {
            return;
        }

        bool casterIsPlayer = caster.GetComponent<PlayerStats>() != null;
        bool casterIsEnemy = caster.GetComponent<Enemy>() != null;

        if (!casterIsPlayer && !casterIsEnemy)
            return;

        if (!context.TryTriggerOncePerRound(owner, this))
            return;

        StatusController casterStatus =
            caster.GetComponent<StatusController>();

        Debug.Log("Кастер " + casterStatus);

        if (casterStatus == null)
            return;

        // Добавляем один стак. StatusController учитывает
        // MaxDurationStacks и модификаторы лимита стаков.
        casterStatus.ApplyStatus(poisonStatus, 1);

        CombatLog combatLog =
            Object.FindFirstObjectByType<CombatLog>();

        combatLog?.AddLog(
            $"{owner.name}'s Toxic Blood poisons {caster.name}!");
    }
}