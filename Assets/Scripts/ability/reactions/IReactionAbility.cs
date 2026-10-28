using UnityEngine;

public interface IReactionAbility
{
    void React(
        GameObject owner,
        GameObject caster,
        GameObject target,
        CombatAttackType attackType,
        DamageType damageType,
        int damage,
        ReactionAbilityContext context);
}