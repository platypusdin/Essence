using UnityEngine;

public class AllyPlacementController : MonoBehaviour
{
    [SerializeField] private Battlefield battlefield;

    private bool isActive;

    private GameObject caster;
    private Enemy corpse;
    private RaiseVermitAbility ability;
    private SummonedAlly allyPrefab;
    private AllyData allyData;

    public bool IsActive => isActive;

    public bool HasFreeSlot(GameObject playerObject)
    {
        if (playerObject == null || battlefield == null)
            return false;

        PlayerMovement movement =
            playerObject.GetComponent<PlayerMovement>();

        if (movement == null || movement.Slots == null)
            return false;

        foreach (MovementSlot slot in movement.Slots)
        {
            if (IsSlotAvailable(slot, movement))
                return true;
        }

        return false;
    }

    public bool BeginPlacement(
        GameObject playerObject,
        Enemy targetCorpse,
        RaiseVermitAbility sourceAbility,
        SummonedAlly prefab,
        AllyData data)
    {
        if (isActive ||
            battlefield == null ||
            playerObject == null ||
            targetCorpse == null ||
            !targetCorpse.IsDead ||
            sourceAbility == null ||
            prefab == null ||
            data == null)
        {
            return false;
        }

        PlayerStats stats = playerObject.GetComponent<PlayerStats>();

        CombatTurnController turnController =
            FindFirstObjectByType<CombatTurnController>();

        if (stats == null ||
            stats.Ap < sourceAbility.apCost ||
            turnController == null ||
            !turnController.IsPlayerTurn() ||
            !HasFreeSlot(playerObject))
        {
            return false;
        }

        caster = playerObject;
        corpse = targetCorpse;
        ability = sourceAbility;
        allyPrefab = prefab;
        allyData = data;
        isActive = true;

        PlayerMovement movement =
            caster.GetComponent<PlayerMovement>();

        movement?.CancelMovement();

        RefreshHighlights();

        return true;
    }

    private void Update()
    {
        if (!isActive)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
            return;
        }

        if (!Input.GetMouseButtonDown(0))
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector2 mousePosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        PlayerMovement movement =
            caster != null ? caster.GetComponent<PlayerMovement>() : null;

        if (movement == null || movement.Slots == null)
        {
            CancelPlacement();
            return;
        }

        foreach (MovementSlot slot in movement.Slots)
        {
            if (slot != null && slot.ContainsWorldPoint(mousePosition))
            {
                TrySelectSlot(slot);
                return;
            }
        }
    }

    public bool TrySelectSlot(MovementSlot slot)
    {
        if (!isActive)
            return false;

        if (slot == null || caster == null || corpse == null)
            return true;

        PlayerMovement movement =
            caster.GetComponent<PlayerMovement>();

        if (movement == null || !IsSlotAvailable(slot, movement))
            return true;

        PlayerStats stats = caster.GetComponent<PlayerStats>();

        CombatTurnController turnController =
            FindFirstObjectByType<CombatTurnController>();

        if (stats == null ||
            stats.Ap < ability.apCost ||
            turnController == null ||
            !turnController.IsPlayerTurn() ||
            !corpse.IsDead)
        {
            CancelPlacement();
            return true;
        }

        BattleRank rank = new BattleRank(
            BattleSide.Heroes,
            slot.Position
        );

        SummonedAlly ally = Instantiate(
            allyPrefab,
            slot.Anchor.position,
            Quaternion.identity
        );

        if (ally == null)
        {
            CancelPlacement();
            return true;
        }

        ally.ConfigureBattlefield(battlefield);
        ally.Initialize(allyData, slot.Position);

        if (ally.Data == null || !battlefield.TryRegister(ally.gameObject, rank))
        {
            Destroy(ally.gameObject);
            CancelPlacement();
            return true;
        }

        if (!stats.TrySpendAp(ability.apCost))
        {
            battlefield.TryUnregister(ally.gameObject);
            Destroy(ally.gameObject);
            CancelPlacement();
            return true;
        }

        CombatParticipant participant =
            ally.GetComponent<CombatParticipant>();

        if (participant == null ||
            !turnController.RegisterSummonedAlly(participant))
        {
            stats.RestoreAp(ability.apCost);
            battlefield.TryUnregister(ally.gameObject);
            Destroy(ally.gameObject);
            CancelPlacement();
            return true;
        }

        string allyName = allyData.allyName;

        Destroy(corpse.gameObject);

        CombatLog combatLog =
            FindFirstObjectByType<CombatLog>();

        combatLog?.AddLog($"{allyName} is summoned.");

        FinishPlacement();

        return true;
    }

    private bool IsSlotAvailable(
        MovementSlot slot,
        PlayerMovement movement)
    {
        if (slot == null || slot.Position == movement.CurrentPosition)
            return false;

        BattleRank rank = new BattleRank(
            BattleSide.Heroes,
            slot.Position
        );

        return battlefield != null &&
               battlefield.IsValidRank(rank) &&
               !battlefield.IsOccupied(rank);
    }

    private void RefreshHighlights()
    {
        PlayerMovement movement =
            caster != null ? caster.GetComponent<PlayerMovement>() : null;

        if (movement == null || movement.Slots == null)
            return;

        foreach (MovementSlot slot in movement.Slots)
        {
            bool available = IsSlotAvailable(slot, movement);
            slot.SetState(available, false, false);
        }
    }

    public void CancelPlacement()
    {
        if (!isActive)
            return;

        FinishPlacement();
    }

    private void FinishPlacement()
    {
        isActive = false;

        PlayerMovement movement =
            caster != null ? caster.GetComponent<PlayerMovement>() : null;

        if (movement != null && movement.Slots != null)
        {
            foreach (MovementSlot slot in movement.Slots)
                slot.SetState(false, false, false);
        }

        caster = null;
        corpse = null;
        ability = null;
        allyPrefab = null;
        allyData = null;
    }
}