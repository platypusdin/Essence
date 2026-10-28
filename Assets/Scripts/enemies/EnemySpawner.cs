using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Battlefield")]
    [SerializeField] private BattlefieldLayout battlefieldLayout;
    [SerializeField] private Battlefield battlefield;

    public Enemy[] SpawnEnemies(EncounterPool encounterPool)
    {
        if (encounterPool == null)
        {
            Debug.LogError("EnemySpawner: Encounter pool is missing.");
            return null;
        }

        if (enemyPrefab == null ||
            battlefieldLayout == null ||
            battlefield == null)
        {
            Debug.LogError("EnemySpawner: Required references are missing.");
            return null;
        }

        Transform[] positions = battlefieldLayout.EnemyPositions;

        if (positions == null || positions.Length == 0)
        {
            Debug.LogError("EnemySpawner: No enemy positions available.");
            return null;
        }

        List<EnemyData> validEnemyTypes = new();

        if (encounterPool.EnemyPool != null)
        {
            foreach (EnemyData data in encounterPool.EnemyPool)
            {
                if (data != null)
                    validEnemyTypes.Add(data);
            }
        }

        if (validEnemyTypes.Count == 0)
        {
            Debug.LogError("EnemySpawner: Encounter pool contains no enemies.");
            return null;
        }

        int minCount = Mathf.Clamp(
            encounterPool.MinEnemyCount, 1, positions.Length);

        int maxCount = Mathf.Clamp(
            Mathf.Max(minCount, encounterPool.MaxEnemyCount),
            minCount,
            positions.Length);

        int enemyCount = Random.Range(minCount, maxCount + 1);
        List<Enemy> spawnedEnemies = new();

        for (int i = 0; i < enemyCount; i++)
        {
            if (positions[i] == null)
                continue;

            EnemyData data =
                validEnemyTypes[Random.Range(0, validEnemyTypes.Count)];

            GameObject instance = Instantiate(
                enemyPrefab,
                positions[i].position,
                Quaternion.identity);

            Enemy enemy = instance.GetComponent<Enemy>();

            if (enemy == null)
            {
                Destroy(instance);
                continue;
            }

            enemy.ConfigurePosition(i + 1);
            enemy.Initialize(data);
            enemy.ConfigureBattlefield(battlefield);

            BattleRank rank =
                new BattleRank(BattleSide.Enemies, i + 1);

            if (!battlefield.TryRegister(instance, rank))
            {
                Destroy(instance);
                continue;
            }

            spawnedEnemies.Add(enemy);
        }

        Debug.Log(
            $"EnemySpawner: Spawned {spawnedEnemies.Count} enemies.");

        return spawnedEnemies.ToArray();
    }
}