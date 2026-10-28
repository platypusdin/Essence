using UnityEngine;

public class MovementSlot : MonoBehaviour
{
    [SerializeField] private Vector2 clickableSize = new Vector2(1.4f, 2.6f);

    [SerializeField] private Color validColor = new Color(0.25f, 1f, 0.48f, 0.95f);
    [SerializeField] private Color selectedColor = new Color(0.35f, 0.78f, 1f, 0.95f);
    [SerializeField] private Color blockedColor = new Color(0.35f, 0.35f, 0.35f, 0.12f);

    [SerializeField] private SpriteRenderer highlight;

    private int position;

    public int Position => position;
    public Transform Anchor => transform;

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

        playerMovement?.TryMoveTo(this);
    }

    public void Configure(int newPosition)
    {
        position = Mathf.Clamp(newPosition, 1, 4);
    }

    public bool ContainsWorldPoint(Vector2 worldPoint)
    {
        Vector3 local = transform.InverseTransformPoint(worldPoint);

        return Mathf.Abs(local.x) <= clickableSize.x * 0.5f
            && local.y >= 0f
            && local.y <= clickableSize.y;
    }

    public void SetState(bool valid, bool selected, bool dimmed)
    {
        if (highlight == null)
            return;

        highlight.enabled = valid || selected || dimmed;

        if (valid)
            highlight.color = validColor;
        else if (selected)
            highlight.color = selectedColor;
        else if (dimmed)
            highlight.color = blockedColor;
    }
}
