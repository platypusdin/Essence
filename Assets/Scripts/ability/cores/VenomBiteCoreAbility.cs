using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Core Abilities/Venom Bite")]
public class VenomBiteCoreAbility : CoreAbility
{
    [Header("Venom Bite")]
    [SerializeField, Min(0)] private int damage = 5;
    [SerializeField, Min(1)] private int poisonStacks = 3;
    [SerializeField] private PoisonStatusData poisonStatus;

    [Header("Damage")]
    [SerializeField] private DamageType damageType = DamageType.Physical;

    public override AbilityExecutionResult Execute(
        GameObject caster,
        GameObject targetObject)
    {
        if (caster == null || targetObject == null || poisonStatus == null)
            return AbilityExecutionResult.Failed;

        StatusController statuses =
            targetObject.GetComponent<StatusController>();

        IDamageable damageable =
            targetObject.GetComponent<IDamageable>();

        if (statuses == null || damageable == null || damageable.IsDead)
            return AbilityExecutionResult.Failed;

        bool attackAccepted = CombatDamageService.TryApplyDamage(
            caster,
            targetObject,
            damage,
            abilityName,
            out _,
            out _,
            attackType,
            damageType
        );

        if (!attackAccepted)
            return AbilityExecutionResult.Failed;

        if (!damageable.IsDead)
        {
            statuses.ApplyStatus(poisonStatus, poisonStacks);

            CombatLog combatLog =
                FindFirstObjectByType<CombatLog>();

            combatLog?.AddLog($"{targetObject.name} is poisoned");
        }

        return AbilityExecutionResult.Completed;
    }
}