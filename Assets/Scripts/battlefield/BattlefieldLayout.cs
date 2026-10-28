using UnityEngine;

public class BattlefieldLayout : MonoBehaviour
{
    private const int RankCount = 4;

    [Header("Hero Formation")]
    [SerializeField] private MovementSlot slotPrefab;
    [SerializeField] private Transform heroSpawnPoint;

    [Header("Enemy Formation")]
    [SerializeField] private Transform enemySpawnPoint;

    [Header("Layout")]
    [SerializeField, Min(0f)] private float spacing = 2f;

    private MovementSlot[] heroSlots;
    private Transform[] heroPositions;
    private Transform[] enemyPositions;

    public MovementSlot[] HeroSlots => heroSlots;
    public Transform[] HeroPositions => heroPositions;
    public Transform[] EnemyPositions => enemyPositions;

    public bool Initialize()
    {
        if (heroSlots != null)
            return true;

        if (slotPrefab == null ||
            heroSpawnPoint == null ||
            enemySpawnPoint == null)
        {
            Debug.LogError(
                "BattlefieldLayout: Prefab or spawn points are missing.",
                this
            );
            return false;
        }

        GameObject heroArea = new GameObject("HeroRanks");
        heroArea.transform.SetParent(transform, false);

        GameObject enemyArea = new GameObject("EnemyRanks");
        enemyArea.transform.SetParent(transform, false);

        heroSlots = new MovementSlot[RankCount];
        heroPositions = new Transform[RankCount];
        enemyPositions = new Transform[RankCount];

        for (int i = 0; i < RankCount; i++)
        {
            // Союзники: ранг 1 находится справа, ранг 4 — слева.
            int heroIndex = RankCount - 1 - i;

            Vector3 heroPosition =
                heroSpawnPoint.position + Vector3.right * (i * spacing);

            MovementSlot slot = Instantiate(
                slotPrefab,
                heroPosition,
                Quaternion.identity,
                heroArea.transform
            );

            slot.name = $"HeroRank{heroIndex + 1}";
            // slot.Configure(heroIndex + 1);
            slot.Configure(heroIndex + 1, BattleSide.Heroes);

            heroSlots[heroIndex] = slot;
            heroPositions[heroIndex] = slot.Anchor;

            // Враги: ранг 1 находится слева, ранг 4 — справа.
            Vector3 enemyPosition =
                enemySpawnPoint.position + Vector3.right * (i * spacing);
            MovementSlot slotEnemy = Instantiate(
                slotPrefab,
                enemyPosition,
                Quaternion.identity,
                enemyArea.transform
            );

            slotEnemy.name = $"EnemyRank{i + 1}";
            slotEnemy.Configure(i + 1, BattleSide.Enemies);

            enemyPositions[i] = slotEnemy.Anchor;
        }

        return true;
    }
}