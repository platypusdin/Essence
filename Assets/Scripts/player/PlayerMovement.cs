using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private const int MovementApCost = 2;

    private Battlefield battlefield;
    private MovementSlot[] slots;
    private int currentPosition;
    private bool movementMode;
    private bool firstMoveFree = true;

    public int CurrentPosition => currentPosition;
    public bool IsMovementMode => movementMode;
    public MovementSlot[] Slots => slots;

    public void Initialize(
        MovementSlot[] movementSlots,
        MovementSlot startSlot)
    {
        if (movementSlots == null || startSlot == null)
        {
            Debug.LogError("PlayerMovement: Slots are missing.");
            return;
        }

        slots = movementSlots;
        currentPosition = startSlot.Position;
    }

    public void ConfigureBattlefield(Battlefield newBattlefield)
    {
        battlefield = newBattlefield;
    }

    private void OnMouseDown()
    {
        CombatTurnController turnController =
            FindFirstObjectByType<CombatTurnController>();

        if (turnController == null || !turnController.IsPlayerTurn())
            return;

        AllyPlacementController placement =
            FindFirstObjectByType<AllyPlacementController>();

        if (placement != null && placement.IsActive)
            return;

        if (!TrySyncPosition())
            return;

        ShowAvailableSlots();
        PlayerAbilities playerAbilities =
            GetComponent<PlayerAbilities>();

        if (playerAbilities != null)
        {
            playerAbilities.SelectTarget(gameObject);

            foreach (MovementSlot slot in slots)
            {
                if (slot != null)
                    slot.SetSelected(false);
            }
        }
    }

    private bool TrySyncPosition()
    {
        if (battlefield == null ||
            !battlefield.TryFindRank(gameObject, out BattleRank rank) ||
            rank.Side != BattleSide.Heroes ||
            !rank.IsValid)
        {
            return false;
        }

        currentPosition = rank.Index;
        return true;
    }

    private void ShowAvailableSlots()
    {
        if (slots == null)
            return;

        movementMode = true;

        PlayerStats stats = GetComponent<PlayerStats>();

        bool canMove =
            firstMoveFree ||
            (stats != null && stats.Ap >= MovementApCost);

        foreach (MovementSlot slot in slots)
        {
            if (slot == null)
                continue;

            bool adjacent =
                Mathf.Abs(slot.Position - currentPosition) == 1;

            bool occupied = IsOccupiedByAlly(slot);

            slot.SetState(
                adjacent && !occupied && canMove,
                false,
                adjacent && (occupied || !canMove)
            );
        }
    }

    private void Update()
    {
        if (!movementMode)
            return;

        AllyPlacementController placement =
            FindFirstObjectByType<AllyPlacementController>();

        if (placement != null && placement.IsActive)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelMovement();
            return;
        }

        if (!Input.GetMouseButtonDown(0))
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector2 mousePosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        Collider2D playerCollider = GetComponent<Collider2D>();

        if (playerCollider != null &&
            playerCollider.OverlapPoint(mousePosition))
        {
            return;
        }

        if (slots == null)
        {
            CancelMovement();
            return;
        }

        foreach (MovementSlot slot in slots)
        {
            if (slot == null ||
                !slot.ContainsWorldPoint(mousePosition))
            {
                continue;
            }

            if (Mathf.Abs(slot.Position - currentPosition) != 1)
                continue;

            if (IsOccupiedByAlly(slot))
                return;

            MoveTo(slot);
            return;
        }

        CancelMovement();
    }

    public void ResetMovement()
    {
        firstMoveFree = true;
    }

    private void MoveTo(MovementSlot slot)
    {
        if (slot == null || !TrySyncPosition())
            return;

        if (Mathf.Abs(slot.Position - currentPosition) != 1 ||
            IsOccupiedByAlly(slot))
        {
            return;
        }

        PlayerStats stats = GetComponent<PlayerStats>();
        bool spentAp = false;

        if (!firstMoveFree)
        {
            if (stats == null || !stats.TrySpendAp(MovementApCost))
                return;

            spentAp = true;
        }

        BattleRank targetRank = new BattleRank(
            BattleSide.Heroes,
            slot.Position
        );

        if (!battlefield.TryMove(gameObject, targetRank))
        {
            if (spentAp)
                stats.RestoreAp(MovementApCost);

            return;
        }

        if (firstMoveFree)
            firstMoveFree = false;

        transform.position = slot.Anchor.position;
        currentPosition = slot.Position;

        CancelMovement();

        CombatLog combatLog =
            FindFirstObjectByType<CombatLog>();

        combatLog?.AddLog(
            $"Player moves to position {slot.Position}"
        );
    }

    public void CancelMovement()
    {
        movementMode = false;

        if (slots == null)
            return;

        foreach (MovementSlot slot in slots)
        {
            if (slot != null)
                slot.SetState(false, false, false);
        }
    }

    public void TryMoveTo(MovementSlot slot)
    {
        CombatTurnController turnController =
            FindFirstObjectByType<CombatTurnController>();

        if (turnController == null || !turnController.IsPlayerTurn())
            return;

        if (!movementMode || slot == null)
            return;

        if (Mathf.Abs(slot.Position - currentPosition) != 1)
            return;

        if (IsOccupiedByAlly(slot))
            return;

        MoveTo(slot);
    }

    private bool IsOccupiedByAlly(MovementSlot slot)
    {
        if (slot == null || battlefield == null)
            return true;

        BattleRank rank = new BattleRank(
            BattleSide.Heroes,
            slot.Position
        );

        return !battlefield.IsValidRank(rank) ||
               battlefield.IsOccupied(rank);
    }
}