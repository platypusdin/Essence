using UnityEngine;

public class MovementSlot : MonoBehaviour
{
    [SerializeField] private Vector2 clickableSize = new Vector2(1.4f, 2.6f);

    [SerializeField] private Color validColor = new Color(0.25f, 1f, 0.48f, 0.95f);
    [SerializeField] private Color selectedColor = new Color(0.35f, 0.78f, 1f, 0.95f);
    [SerializeField] private Color blockedColor = new Color(0.35f, 0.35f, 0.35f, 0.12f);

    [SerializeField] private SpriteRenderer highlight;

    private int position;
    private bool targetSelected;
    private bool movementValid;
    private bool movementSelected;
    private bool movementDimmed;

    public int Position => position;
    public Transform Anchor => transform;
    private BattleSide side;

    public BattleSide Side => side;

    private void Awake()
    {
        SetState(false, false, false);
    }
    private void OnMouseDown()
    {
        AllyPlacementController placement =
            FindFirstObjectByType<AllyPlacementController>();

        if (placement != null && placement.IsActive)
        {
            placement.TrySelectSlot(this);
            return;
        }

        PlayerMovement playerMovement =
            FindFirstObjectByType<PlayerMovement>();

        if (playerMovement != null && playerMovement.IsMovementMode)
        {
            playerMovement.TryMoveTo(this);
            return;
        }

        PlayerAbilities playerAbilities = FindFirstObjectByType<PlayerAbilities>();
        playerAbilities?.SelectTargetAtRank(side, position);
    }

    public void Configure(int newPosition, BattleSide newSide)
    {
        position = Mathf.Clamp(newPosition, 1, 4);
        side = newSide;
    }

    public bool ContainsWorldPoint(Vector2 worldPoint)
    {
        Vector3 local = transform.InverseTransformPoint(worldPoint);

        return Mathf.Abs(local.x) <= clickableSize.x * 0.5f
            && local.y >= 0f
            && local.y <= clickableSize.y;
    }

    public void SetSelected(bool selected)
    {
        targetSelected = selected;
        RefreshHighlight();
    }


    public void SetState(bool valid, bool selected, bool dimmed)
    {
        movementValid = valid;
        movementSelected = selected;
        movementDimmed = dimmed;

        RefreshHighlight();
    }

    private void RefreshHighlight()
    {
        if (highlight == null)
            return;

        highlight.enabled =
            targetSelected ||
            movementValid ||
            movementSelected ||
            movementDimmed;

        if (targetSelected || movementSelected)
            highlight.color = selectedColor;
        else if (movementValid)
            highlight.color = validColor;
        else if (movementDimmed)
            highlight.color = blockedColor;
    }
}
