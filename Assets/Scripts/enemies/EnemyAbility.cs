using UnityEngine;

public abstract class EnemyAbility : Ability
{

    [Min(0)] public int damage = 5;
    public DamageType damageType = DamageType.Physical;

    protected virtual void OnValidate()
    {
        target = AbilityTarget.Enemy;
    }

    public sealed override AbilityExecutionResult Execute(
        GameObject caster,
        GameObject targetObject)
    {
        if (caster == null || targetObject == null)
            return AbilityExecutionResult.Failed;

        Enemy enemy = caster.GetComponent<Enemy>();
        PlayerStats player = targetObject.GetComponent<PlayerStats>();

        if (enemy == null || player == null)
            return AbilityExecutionResult.Failed;

        Execute(enemy, player, FindFirstObjectByType<CombatLog>());

        return AbilityExecutionResult.Completed;
    }

    public virtual void Execute(
        Enemy caster,
        PlayerStats target,
        CombatLog combatLog)
    {
        if (caster == null || target == null)
            return;

        CombatDamageService.TryApplyDamage(
            caster.gameObject,
            target.gameObject,
            damage,
            abilityName,
            out _,
            out _,
            attackType,
            damageType
        );
    }
}