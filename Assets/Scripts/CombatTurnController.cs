using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CombatTurnController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RoundCounter roundCounter;
    [SerializeField] private InitiativePanel initiativePanel;


    [Header("Enemy Turns")]
    [SerializeField, Min(0f)] private float enemyAttackDelay = 2f;

    private PlayerStats playerStats;
    private PlayerMovement playerMovement;
    private PassiveAbilityController passiveAbilityController;
    private ReactionAbilityController reactionAbilityController;
    private CombatEventController combatEventController;
    public static event Action<GameObject> ActionCompleted;

    private readonly List<CombatParticipant> turnOrder = new();
    private readonly List<CombatParticipant> completedTurns = new();

    private int currentRound = 1;
    private int currentIndex;
    private bool playerTurn;
    private bool hasStarted;
    private bool combatEnded;

    public int CurrentRound => currentRound;

    private void Start()
    {
        hasStarted = true;
        CachePlayerReferences();

        if (playerStats == null || playerStats.Hp <= 0)
            return;

        passiveAbilityController = new PassiveAbilityController(this);
        passiveAbilityController.Initialize();

        reactionAbilityController = new ReactionAbilityController(this);
        reactionAbilityController.Initialize();

        combatEventController = new CombatEventController();
        combatEventController.Initialize();

        StartRound();
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.Died -= HandlePlayerDeath;
        
        passiveAbilityController?.Dispose();
        reactionAbilityController?.Dispose();
        combatEventController?.Dispose();
    }

    public static void ReportActionCompleted(GameObject actor)
    {
        if (actor != null)
            ActionCompleted?.Invoke(actor);
    }

    private void CachePlayerReferences()
    {
        if (playerStats != null)
            playerStats.Died -= HandlePlayerDeath;

        playerStats = FindFirstObjectByType<PlayerStats>();

        if (playerStats == null)
        {
            Debug.LogError("CombatTurnController: PlayerStats not found.");
            playerMovement = null;
            return;
        }

        playerMovement = playerStats.GetComponent<PlayerMovement>();
        playerStats.Died += HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        if (combatEnded)
            return;

        combatEnded = true;
        playerTurn = false;

        StopAllCoroutines();

        RefreshInitiativePanel();

        Debug.Log("Combat ended: player died.");
    }

    public void RestartCombat()
    {
        StopAllCoroutines();

        playerTurn = false;
        combatEnded = false;
        currentRound = 1;
        currentIndex = 0;

        turnOrder.Clear();
        completedTurns.Clear();

        if (!hasStarted)
            return;

        CachePlayerReferences();

        if (playerStats == null || playerStats.Hp <= 0)
            return;

        passiveAbilityController?.RecalculateAll();
        reactionAbilityController?.Reset();

        AbilityCooldownController[] cooldownControllers = FindObjectsByType<AbilityCooldownController>(
        FindObjectsSortMode.None);

        foreach (AbilityCooldownController controller in cooldownControllers)
            controller.ResetCooldowns();

        StartRound();
    }

    public void EndTurn()
    {
        if (combatEnded ||
            !playerTurn ||
            currentIndex >= turnOrder.Count)
        {
            return;
        }

        CombatParticipant currentParticipant = turnOrder[currentIndex];

        StatusController statusController = currentParticipant != null
            ? currentParticipant.GetComponent<StatusController>()
            : null;

        statusController?.ProcessTurnEnd();

        if (combatEnded || playerStats == null || playerStats.Hp <= 0)
        {
            HandlePlayerDeath();
            return;
        }

        playerTurn = false;

        completedTurns.Add(turnOrder[currentIndex]);
        currentIndex++;

        RefreshInitiativePanel();
        StartCoroutine(ProcessNextTurn());
    }

    public bool IsPlayerTurn()
    {
        return playerTurn && !combatEnded;
    }

    public void RefreshInitiativeOrder()
    {
        if (combatEnded || turnOrder.Count == 0)
        {
            RefreshInitiativePanel();
            return;
        }

        int sortStart = Mathf.Min(currentIndex + 1, turnOrder.Count);

        turnOrder.Sort(
            sortStart,
            turnOrder.Count - sortStart,
            Comparer<CombatParticipant>.Create(
                (a, b) => b.Speed.CompareTo(a.Speed)));

        RefreshInitiativePanel();
    }

    public bool RegisterSummonedAlly(CombatParticipant participant)
    {
        if (combatEnded ||
            participant == null ||
            !playerTurn)
        {
            return false;
        }

        if (turnOrder.Contains(participant))
            return false;

        CombatParticipant playerParticipant =
            playerStats != null
                ? playerStats.GetComponent<CombatParticipant>()
                : null;

        int playerIndex = turnOrder.IndexOf(playerParticipant);

        if (playerIndex < 0 || playerIndex < currentIndex)
            return false;

        int insertIndex = playerIndex + 1;

        while (insertIndex < turnOrder.Count)
        {
            CombatParticipant next = turnOrder[insertIndex];

            if (next == null ||
                next.GetComponent<SummonedAlly>() == null)
            {
                break;
            }

            insertIndex++;
        }

        turnOrder.Insert(insertIndex, participant);

        turnOrder.Insert(insertIndex, participant);
        reactionAbilityController?.RefreshReactionCache();
        RefreshInitiativePanel();

        return true;
    }

    private void StartRound()
    {
        if (combatEnded ||
            playerStats == null ||
            playerStats.Hp <= 0)
        {
            return;
        }

        turnOrder.Clear();
        completedTurns.Clear();
        currentIndex = 0;
        playerTurn = false;

        CombatParticipant[] participants =
            FindObjectsByType<CombatParticipant>(
                FindObjectsSortMode.None
            );

        foreach (CombatParticipant participant in participants)
        {
            if (IsAlive(participant))
                turnOrder.Add(participant);
        }
        reactionAbilityController?.RefreshReactionCache();

        turnOrder.Sort((a, b) => b.Speed.CompareTo(a.Speed));
        ShuffleEqualSpeeds();

        if (roundCounter != null)
            roundCounter.SetRound(currentRound);

        RefreshInitiativePanel();

        Debug.Log($"Round {currentRound} started.");

        StartCoroutine(ProcessNextTurn());
    }

    private IEnumerator ProcessNextTurn()
    {
        bool enemyHasActed = false;

        while (currentIndex < turnOrder.Count)
        {
            if (combatEnded)
                yield break;

            if (playerStats == null || playerStats.Hp <= 0)
            {
                HandlePlayerDeath();
                yield break;
            }

            CombatParticipant participant = turnOrder[currentIndex];

            if (!IsAlive(participant))
            {
                currentIndex++;
                continue;
            }

            // Обработка статусов в начале хода.
            StatusController statusController =
                participant.GetComponent<StatusController>();

            statusController?.ProcessTurnStart();

            if (combatEnded)
                yield break;

            if (playerStats == null || playerStats.Hp <= 0)
            {
                HandlePlayerDeath();
                yield break;
            }

            if (!IsAlive(participant))
            {
                completedTurns.Add(participant);
                currentIndex++;

                RefreshInitiativePanel();
                continue;
            }

            AbilityCooldownController cooldownController = participant.GetComponent<AbilityCooldownController>();
            cooldownController?.AdvanceCooldowns();

            PlayerStats player =
                participant.GetComponent<PlayerStats>();

            if (player != null)
            {
                StartPlayerTurn();
                RefreshInitiativePanel();

                Debug.Log($"Player turn. Round {currentRound}");
                yield break;
            }

            AllyTurn allyTurn =
                participant.GetComponent<AllyTurn>();

            if (allyTurn != null)
            {
                allyTurn.TakeTurn();
            }
            else
            {
                EnemyTurn enemyTurn =
                    participant.GetComponent<EnemyTurn>();

                if (enemyTurn != null)
                {
                    if (enemyHasActed)
                        yield return new WaitForSeconds(enemyAttackDelay);

                    if (combatEnded)
                        yield break;

                    if (playerStats == null || playerStats.Hp <= 0)
                    {
                        HandlePlayerDeath();
                        yield break;
                    }

                    if (IsAlive(participant))
                        enemyTurn.TakeTurn();

                    enemyHasActed = true;
                }
            }

            if (combatEnded)
                yield break;

            if (playerStats == null || playerStats.Hp <= 0)
            {
                HandlePlayerDeath();
                yield break;
            }
            // Обработка статусов в конце хода.
            statusController?.ProcessTurnEnd();

            completedTurns.Add(participant);
            currentIndex++;

            RefreshInitiativePanel();
        }

        if (combatEnded)
            yield break;

        if (playerStats == null || playerStats.Hp <= 0)
        {
            HandlePlayerDeath();
            yield break;
        }

        currentRound++;
        StartRound();
    }

    private void StartPlayerTurn()
    {
        if (combatEnded || playerStats == null || playerStats.Hp <= 0)
            return;

        playerTurn = true;

        playerStats.RestoreFullAp();
        playerMovement?.ResetMovement();
    }

    private bool IsAlive(CombatParticipant participant)
    {
        if (participant == null)
            return false;

        Enemy enemy = participant.GetComponent<Enemy>();

        if (enemy != null)
            return !enemy.IsDead;

        SummonedAlly ally = participant.GetComponent<SummonedAlly>();

        if (ally != null)
            return !ally.IsDead;

        PlayerStats player =
            participant.GetComponent<PlayerStats>();

        return player == null || player.Hp > 0;
    }

    private void ShuffleEqualSpeeds()
    {
        int start = 0;

        while (start < turnOrder.Count)
        {
            int end = start + 1;

            while (end < turnOrder.Count &&
                   turnOrder[end].Speed == turnOrder[start].Speed)
            {
                end++;
            }

            for (int i = end - 1; i > start; i--)
            {
                int j = UnityEngine.Random.Range(start, i + 1);

                (turnOrder[i], turnOrder[j]) =
                    (turnOrder[j], turnOrder[i]);
            }

            start = end;
        }
    }


    private void RefreshInitiativePanel()
    {
        if (initiativePanel == null)
            return;

        List<CombatParticipant> upcoming = new();

        for (int i = currentIndex; i < turnOrder.Count; i++)
        {
            if (IsAlive(turnOrder[i]))
                upcoming.Add(turnOrder[i]);
        }

        initiativePanel.Refresh(upcoming, completedTurns);
    }

    public void RefreshInitiative()
    {
        RefreshInitiativePanel();
    }
}