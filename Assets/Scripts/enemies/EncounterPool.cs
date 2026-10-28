using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Encounter Pool")]
public class EncounterPool : ScriptableObject
{
    [System.Serializable]
    public class AbilityDropEntry
    {
        public Ability ability;

        [Range(0f, 100f)]
        public float dropChance;
    }

    [Header("Enemies")]
    [SerializeField] private EnemyData[] enemyPool;
    [SerializeField, Min(1)] private int minEnemyCount = 2;
    [SerializeField, Min(1)] private int maxEnemyCount = 4;

    [Header("Rewards")]
    [SerializeField, Min(0)] private int experienceReward = 10;
    [SerializeField] private AbilityDropEntry[] abilityDrops;

    public EnemyData[] EnemyPool => enemyPool;
    public int MinEnemyCount => minEnemyCount;
    public int MaxEnemyCount => maxEnemyCount;
    public int ExperienceReward => experienceReward;

    public Ability RollAbilityDrop()
    {
        float roll = Random.Range(0f, 100f);
        float cumulativeChance = 0f;

        if (abilityDrops == null)
            return null;

        foreach (AbilityDropEntry entry in abilityDrops)
        {
            if (entry == null || entry.ability == null)
                continue;

            cumulativeChance += entry.dropChance;

            if (roll < cumulativeChance)
                return entry.ability;
        }

        return null;
    }

    private void OnValidate()
    {
        minEnemyCount = Mathf.Max(1, minEnemyCount);
        maxEnemyCount = Mathf.Max(minEnemyCount, maxEnemyCount);
    }
}