using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Rooms")]
    [SerializeField] private RoomData[] availableRooms;

    [Header("Environment")]
    [SerializeField] private SpriteRenderer backgroundRenderer;

    [Header("Spawning")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Combat")]
    [SerializeField] private CombatTurnController turnController;

    [Header("Completion")]
    [SerializeField] private RoomCompletionController completionController;

    private RoomData currentRoom;
    private Enemy[] currentEnemies;
    private int remainingEnemies;
    private bool roomCompleted;

    public RoomData CurrentRoom => currentRoom;

    private void Start()
    {
        GenerateRoom();
    }

    public void GenerateRoom()
    {
        if (availableRooms == null || availableRooms.Length == 0)
        {
            Debug.LogError("RoomController: No rooms assigned.");
            return;
        }

        if (backgroundRenderer == null ||
            enemySpawner == null ||
            turnController == null)
        {
            Debug.LogError("RoomController: Required references are missing.");
            return;
        }

        RoomData nextRoom = GetRandomRoom();

        if (nextRoom == null || nextRoom.Background == null)
        {
            Debug.LogError(
                "RoomController: Selected room or its background is missing."
            );
            return;
        }

        ClearCurrentEnemies();

        roomCompleted = false;
        remainingEnemies = 0;

        completionController?.HideVictory();

        currentRoom = nextRoom;
        backgroundRenderer.sprite = currentRoom.Background;

        Enemy[] spawnedEnemies = enemySpawner.SpawnEnemies(currentRoom.EncounterPool);

        if (spawnedEnemies == null || spawnedEnemies.Length == 0)
        {
            Debug.LogError(
                $"RoomController: No enemies spawned for '{currentRoom.RoomName}'."
            );
            return;
        }

        currentEnemies = spawnedEnemies;
        remainingEnemies = currentEnemies.Length;

        foreach (Enemy enemy in currentEnemies)
        {
            if (enemy != null)
                enemy.Died += OnEnemyDied;
        }

        turnController.RestartCombat();

        Debug.Log($"Room generated: {currentRoom.RoomName}");
    }

    private Ability GrantRoomRewards()
    {
        EncounterPool pool = currentRoom != null
            ? currentRoom.EncounterPool
            : null;

        if (pool == null)
            return null;

        Ability droppedAbility = pool.RollAbilityDrop();

        if (droppedAbility == null)
        {
            Debug.Log("No ability dropped.");
            return null;
        }

        PlayerAbilities playerAbilities =
            FindFirstObjectByType<PlayerAbilities>();

        if (playerAbilities == null)
            return null;

        if (!playerAbilities.TryAddAvailableAbility(droppedAbility))
        {
            Debug.Log(
                $"Ability not added (already unlocked): {droppedAbility.abilityName}"
            );

            return null;
        }

        return droppedAbility;
    }
    private void ClearCurrentEnemies()
    {
        if (currentEnemies == null)
            return;

        foreach (Enemy enemy in currentEnemies)
        {
            if (enemy == null)
                continue;

            enemy.Died -= OnEnemyDied;

            // Сразу исключаем старого врага из поиска участников боя.
            enemy.ReleaseBattlefieldRank();
            enemy.gameObject.SetActive(false);
            Destroy(enemy.gameObject);
        }

        currentEnemies = null;
    }

    private void OnEnemyDied(Enemy enemy)
    {
        if (enemy == null || roomCompleted)
            return;

        enemy.Died -= OnEnemyDied;

        remainingEnemies = Mathf.Max(0, remainingEnemies - 1);

        Debug.Log($"Enemies remaining: {remainingEnemies}");

        if (remainingEnemies != 0)
            return;

        roomCompleted = true;

        Ability droppedAbility = GrantRoomRewards();

        int experienceReward = currentRoom.EncounterPool.ExperienceReward;

        completionController?.ShowVictory(
            experienceReward,
            droppedAbility
        );
    }

    private RoomData GetRandomRoom()
    {
        int validCount = 0;

        foreach (RoomData room in availableRooms)
        {
            if (room != null)
                validCount++;
        }

        if (validCount == 0)
            return null;

        RoomData[] validRooms = new RoomData[validCount];
        int index = 0;

        foreach (RoomData room in availableRooms)
        {
            if (room != null)
                validRooms[index++] = room;
        }

        if (validRooms.Length == 1)
            return validRooms[0];

        int currentIndex = System.Array.IndexOf(validRooms, currentRoom);
        int randomIndex = Random.Range(0, validRooms.Length - 1);

        if (currentIndex >= 0 && randomIndex >= currentIndex)
            randomIndex++;

        return validRooms[randomIndex];
    }
}