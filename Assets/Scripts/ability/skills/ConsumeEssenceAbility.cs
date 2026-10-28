using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Abilities/Consume Essence")]
public class ConsumeEssenceAbility : Ability
{
    [Header("Healing")]
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

        PlayerStats player = caster.GetComponent<PlayerStats>();

        if (player == null)
            return AbilityExecutionResult.Failed;

        player.RestoreHp(power);

        CombatLog combatLog =
            FindFirstObjectByType<CombatLog>();

        combatLog?.AddLog(
            $"{caster.name} uses {abilityName} and restores {power} HP"
        );

        Destroy(corpse.gameObject);

        return AbilityExecutionResult.Completed;
    }
}