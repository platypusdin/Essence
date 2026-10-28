using UnityEngine;

[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(AbilityCooldownController))]
public class EnemyAI : MonoBehaviour
{
    public const int StepApCost = 1;

    private Enemy enemy;
    private EnemyData data;
    private Battlefield battlefield;
    private CombatTargetingService targetingService;
    private AbilityCooldownController cooldownController;

    private int currentAp;
    private bool mainActionUsed;

    public int CurrentAP => currentAp;
    public bool MainActionUsed => mainActionUsed;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        cooldownController = GetComponent<AbilityCooldownController>();
    }

    public void Initialize(EnemyData enemyData)
    {
        data = enemyData;
    }

    public void ConfigureBattlefield(Battlefield newBattlefield)
    {
        battlefield = newBattlefield;

        targetingService = battlefield != null
            ? new CombatTargetingService(battlefield)
            : null;
    }

    public void BeginTurn()
    {
        currentAp = data != null
            ? Mathf.Max(0, data.tacticalApPerTurn)
            : 0;

        mainActionUsed = false;
    }

    public bool CanMoveForward()
    {
        if (battlefield == null || enemy == null || enemy.IsDead)
            return false;

        if (!battlefield.TryFindRank(
                enemy.gameObject,
                out BattleRank currentRank) ||
            currentRank.Side != BattleSide.Enemies ||
            !currentRank.IsValid)
        {
            return false;
        }

        int nextIndex = currentRank.Index - 1;

        if (nextIndex < 1)
            return false;

        BattleRank nextRank = new(
            BattleSide.Enemies,
            nextIndex
        );

        return battlefield.IsValidRank(nextRank) &&
               !battlefield.IsOccupied(nextRank);
    }

    public bool TryMoveForward()
    {
        if (currentAp < StepApCost || !CanMoveForward())
            return false;

        if (!battlefield.TryFindRank(
                enemy.gameObject,
                out BattleRank currentRank))
        {
            return false;
        }

        BattleRank nextRank = new(
            BattleSide.Enemies,
            currentRank.Index - 1
        );

        Transform anchor = battlefield.GetAnchor(nextRank);

        if (anchor == null ||
            !battlefield.TryMove(enemy.gameObject, nextRank))
        {
            return false;
        }

        enemy.transform.position = anchor.position;
        currentAp -= StepApCost;

        CombatLog combatLog =
            FindFirstObjectByType<CombatLog>();

        combatLog?.AddLog($"{enemy.gameObject.name} moves forward.");

        return true;
    }

    public bool TryChooseAbility(
        PlayerStats player,
        out Ability selectedAbility,
        out GameObject targetObject)
    {
        selectedAbility = null;
        targetObject = null;

        if (mainActionUsed ||
            currentAp <= 0 ||
            player == null ||
            player.Hp <= 0 ||
            data == null ||
            data.abilities == null ||
            enemy == null ||
            enemy.IsDead ||
            cooldownController == null)
        {
            return false;
        }

        foreach (Ability ability in data.abilities)
        {
            if (ability == null ||
                ability.activationType != AbilityActivationType.Active ||
                ability.apCost > currentAp ||
                !cooldownController.CanUseAbility(ability))
            {
                continue;
            }

            if (!TryResolveTarget(ability, player, out GameObject candidate))
                continue;

            selectedAbility = ability;
            targetObject = candidate;
            return true;
        }

        return false;
    }

    public bool CommitAbility(Ability ability)
    {
        if (ability == null ||
            mainActionUsed ||
            ability.activationType != AbilityActivationType.Active ||
            ability.apCost > currentAp)
        {
            return false;
        }

        currentAp -= ability.apCost;
        mainActionUsed = true;

        cooldownController.StartCooldown(ability);

        return true;
    }

    private bool TryResolveTarget(
        Ability ability,
        PlayerStats player,
        out GameObject targetObject)
    {
        targetObject = null;

        switch (ability.target)
        {
            case AbilityTarget.Self:
                targetObject = enemy.gameObject;
                return true;

            case AbilityTarget.Enemy:
                if (player == null ||
                    targetingService == null ||
                    !CanTargetOpponent(ability, player.gameObject))
                {
                    return false;
                }

                targetObject = player.gameObject;
                return true;

            case AbilityTarget.Corpse:
                return TryFindCorpseTarget(out targetObject);

            default:
                return false;
        }
    }

    private bool CanTargetOpponent(
        Ability ability,
        GameObject target)
    {
        if (ability.attackType != CombatAttackType.Other)
        {
            return targetingService.CanTarget(
                enemy.gameObject,
                target,
                ability.attackType);
        }

        if (!battlefield.TryFindRank(
                enemy.gameObject,
                out BattleRank attackerRank) ||
            !battlefield.TryFindRank(
                target,
                out BattleRank targetRank))
        {
            return false;
        }

        return attackerRank.IsValid &&
               targetRank.IsValid &&
               attackerRank.Side != targetRank.Side;
    }

    private bool TryFindCorpseTarget(out GameObject targetObject)
    {
        targetObject = null;

        if (battlefield == null ||
            !battlefield.TryFindRank(
                enemy.gameObject,
                out BattleRank ownRank))
        {
            return false;
        }

        Enemy[] enemies =
            FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        int nearestDistance = int.MaxValue;

        foreach (Enemy candidate in enemies)
        {
            if (candidate == null || !candidate.IsDead)
                continue;

            if (!battlefield.TryFindRank(
                    candidate.gameObject,
                    out BattleRank candidateRank) ||
                candidateRank.Side != ownRank.Side)
            {
                continue;
            }

            int distance = Mathf.Abs(
                candidateRank.Index - ownRank.Index);

            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            targetObject = candidate.gameObject;
        }

        return targetObject != null;
    }
}