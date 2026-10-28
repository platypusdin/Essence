using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Statuses/Bleeding")]
public class BleedStatusData : StatusEffectData
{
    public override void OnAction(
        GameObject owner,
        StatusController statusController)
    {
        if (owner == null || statusController == null)
            return;

        int stacks = statusController.GetStacks(this);

        if (stacks <= 0)
            return;

        PlayerStats player = owner.GetComponent<PlayerStats>();

        if (player != null)
        {
            if (player.Hp <= 0)
                return;

            player.TakeDamage(stacks, false);
        }
        else
        {
            Enemy enemy = owner.GetComponent<Enemy>();

            if (enemy == null || enemy.IsDead)
                return;

            enemy.TakeDamage(stacks, false);
        }

        CombatLog combatLog =
            FindFirstObjectByType<CombatLog>();

        combatLog?.AddLog(
            $"{owner.name} takes {stacks} bleeding damage"
        );
    }
}