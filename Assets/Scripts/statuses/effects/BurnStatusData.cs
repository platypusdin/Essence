using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Statuses/Burning")]

public class BurnStatusData : StatusEffectData
{
    [SerializeField, Min(0)] private int damagePerTurn = 5;

    public override void OnTurnEnd(GameObject owner)
    {
        if (owner == null || damagePerTurn <= 0)
            return;

        PlayerStats player = owner.GetComponent<PlayerStats>();

        if (player != null)
        {
            if (player.Hp <= 0)
                return;

            player.TakeDamage(damagePerTurn, false);
        }
        else
        {
            Enemy enemy = owner.GetComponent<Enemy>();

            if (enemy == null || enemy.IsDead)
                return;

            enemy.TakeDamage(damagePerTurn, false);
        }

        CombatLog combatLog =
            FindFirstObjectByType<CombatLog>();

        combatLog?.AddLog(
            $"{owner.name} takes {damagePerTurn} burning damage"
        );
    }
}
