using UnityEngine;

[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(EnemyAI))]
public class EnemyTurn : MonoBehaviour
{
    private Enemy enemy;
    private EnemyAI enemyAI;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeTurn()
    {
        if (enemy == null || enemy.IsDead || enemy.Hp <= 0)
            return;

        PlayerStats player =
            FindFirstObjectByType<PlayerStats>();

        if (player == null || player.Hp <= 0)
            return;

        enemyAI.BeginTurn();

        // Сначала пробуем применить лучшую доступную способность.
        if (TryUseAbility(player))
        {
            // Если остался AP, враг может сделать шаг.
            enemyAI.TryMoveForward();
            return;
        }

        // Если атаковать не получилось, пробуем шагнуть.
        if (enemyAI.TryMoveForward())
        {
            // После шага повторно проверяем допустимость атаки.
            TryUseAbility(player);
            return;
        }

        Debug.Log($"{gameObject.name} has no available action");
    }

    private bool TryUseAbility(PlayerStats player)
    {
        if (!enemyAI.TryChooseAbility(
                player,
                out Ability ability,
                out GameObject targetObject))
        {
            return false;
        }

        AbilityExecutionResult result =
            ability.Execute(enemy.gameObject, targetObject);

        if (result == AbilityExecutionResult.Failed)
        {
            Debug.LogWarning(
                $"{enemy.gameObject.name} failed to execute " +
                $"{ability.abilityName}"
            );

            return false;
        }

        if (!enemyAI.CommitAbility(ability))
        {
            Debug.LogError(
                $"Could not commit ability {ability.abilityName} " +
                $"for {enemy.gameObject.name}."
            );

            return false;
        }

        return true;
    }
}