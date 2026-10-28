
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Statuses/Poison")]
public class PoisonStatusData : StatusEffectData
{
    private void PublishPoisonKill(GameObject victim, int stacks)
    {
        CombatEventBus.Publish(new CombatEvent(
            CombatEventType.StatusKilled,
            target: victim,
            status: this,
            value: stacks));
    }

    public override void OnTurnStart(GameObject owner)
    {
        if (owner == null)
            return;

        StatusController controller =
            owner.GetComponent<StatusController>();

        if (controller == null)
            return;

        int stacks = controller.GetStacks(this);

        if (stacks <= 0)
            return;

        int actualDamage = 0;

        PlayerStats player = owner.GetComponent<PlayerStats>();

        if (player != null)
        {
            if (player.Hp > 0)
                actualDamage = player.TakeDamage(stacks, false);
        }
        else
        {
            Enemy enemy = owner.GetComponent<Enemy>();

            if (enemy != null && !enemy.IsDead)
                actualDamage = enemy.TakeDamage(stacks, false);
        }

        CombatLog combatLog =
            FindFirstObjectByType<CombatLog>();

        if (actualDamage > 0)
        {
            combatLog?.AddLog(
                $"{owner.name} takes {actualDamage} poison damage");
        }

        bool diedFromPoison = false;

        if (actualDamage > 0)
        {
            Enemy enemy = owner.GetComponent<Enemy>();

            diedFromPoison =
                (player != null && player.IsDead) ||
                (enemy != null && enemy.IsDead);
        }

        if (diedFromPoison)
            PublishPoisonKill(owner, stacks);

        controller.ConsumeStack(this);
    }
}
