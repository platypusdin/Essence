using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Abilities/Corpse Burst")]
public class CorpseBurstAbility : Ability
{
    [Header("Damage")]
    [Min(0)] public int power = 10;

    private void OnValidate()
    {
        target = AbilityTarget.Corpse;
    }

    public override AbilityExecutionResult Execute(
        GameObject caster,
        GameObject targetObject)
    {
        if (caster == null || targetObject == null)
            return AbilityExecutionResult.Failed;

        Enemy corpse = targetObject.GetComponent<Enemy>();

        if (corpse == null || !corpse.IsDead)
            return AbilityExecutionResult.Failed;

        int corpsePosition = 4 + corpse.Position;

        Enemy[] enemies =
            FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            int enemyPosition = 4 + enemy.Position;

            if (Mathf.Abs(enemyPosition - corpsePosition) != 1)
                continue;

            CombatDamageService.TryApplyDamage(
                caster,
                enemy.gameObject,
                power,
                abilityName,
                out _,
                out _
            );
        }

        PlayerStats player =
            FindFirstObjectByType<PlayerStats>();

        PlayerMovement movement =
            player != null
                ? player.GetComponent<PlayerMovement>()
                : null;

        if (player != null && movement != null &&
            Mathf.Abs(movement.CurrentPosition - corpsePosition) == 1)
        {
            player.TakeDamage(power);
        }

        SummonedAlly[] allies =
            FindObjectsByType<SummonedAlly>(FindObjectsSortMode.None);

        foreach (SummonedAlly ally in allies)
        {
            if (ally == null || ally.IsDead)
                continue;

            if (Mathf.Abs(ally.Position - corpsePosition) == 1)
                ally.TakeDamage(power);
        }

        CombatLog combatLog =
            FindFirstObjectByType<CombatLog>();

        combatLog?.AddLog($"{abilityName} activated");

        Destroy(corpse.gameObject);

        return AbilityExecutionResult.Completed;
    }
}