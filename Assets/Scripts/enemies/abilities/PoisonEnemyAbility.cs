using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Enemy Abilities/Poison")]
public class PoisonEnemyAbility : EnemyAbility
{
    [Header("Rotten Spit")]
    [SerializeField, Min(0)] private int poisonStacks = 2;
    [SerializeField] private PoisonStatusData poisonStatus;

    public override void Execute(
        Enemy caster,
        PlayerStats target,
        CombatLog combatLog)
    {
        if (caster == null || target == null ||
            poisonStatus == null || target.Hp <= 0)
        {
            return;
        }

        StatusController statuses =
            target.GetComponent<StatusController>();

        if (statuses == null)
            return;

        bool attackAccepted = CombatDamageService.TryApplyDamage(
            caster.gameObject,
            target.gameObject,
            damage,
            abilityName,
            out _,
            out _,
            CombatAttackType.Ranged,
            damageType
        );

        if (!attackAccepted || target.Hp <= 0)
            return;

        for (int i = 0; i < poisonStacks; i++)
            statuses.ApplyStatus(poisonStatus, poisonStacks);

        combatLog?.AddLog(
            $"{target.gameObject.name} receives {poisonStacks} poison stacks"
        );
    }
}