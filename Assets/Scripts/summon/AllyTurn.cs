using UnityEngine;

[RequireComponent(typeof(SummonedAlly))]
public class AllyTurn : MonoBehaviour
{
    private SummonedAlly ally;

    private void Awake()
    {
        ally = GetComponent<SummonedAlly>();
    }

    public void TakeTurn()
    {
        if (ally == null || ally.IsDead || ally.Data == null)
            return;

        Enemy nearestEnemy = null;
        int nearestDistance = int.MaxValue;

        Enemy[] enemies =
            FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            // Слоты врагов расположены на позициях 5–8.
            int enemyPosition = 4 + enemy.Position;
            int distance = Mathf.Abs(enemyPosition - ally.Position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy == null)
        {
            Debug.Log($"{ally.name} has no available target.");
            return;
        }

        nearestEnemy.TakeDamage(ally.Data.damage);

        CombatLog combatLog =
            FindFirstObjectByType<CombatLog>();

        combatLog?.AddLog(
            $"{ally.Data.allyName} attacks {nearestEnemy.name} " +
            $"for {ally.Data.damage} damage"
        );
    }
}