using UnityEngine;

public class SummonedAlly : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private CharacterStatusUI statusUI;
    private AllyData data;
    private Battlefield battlefield;
    private int position;
    private bool initialized;

    public AllyData Data => data;
    public int Position => position;
    public int Hp { get; private set; }
    public bool IsDead => Hp <= 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        statusUI = GetComponentInChildren<CharacterStatusUI>();
    }

    public void ConfigureBattlefield(Battlefield newBattlefield)
    {
        battlefield = newBattlefield;
    }

    public void Initialize(AllyData allyData, int slotPosition)
    {
        if (allyData == null)
        {
            Debug.LogError("SummonedAlly: AllyData is missing.");
            return;
        }

        data = allyData;
        position = slotPosition;
        Hp = data.maxHp;
        initialized = true;

        if (spriteRenderer != null && data.sprite != null)
            spriteRenderer.sprite = data.sprite;

        statusUI?.UpdateHealth(Hp, data.maxHp);

        CombatParticipant participant =
            GetComponent<CombatParticipant>();

        if (participant != null)
            participant.SetSpeed(data.speed);
    }

    public void TakeDamage(int amount)
    {
        if (!initialized || IsDead || amount <= 0)
            return;

        Hp = Mathf.Max(0, Hp - amount);
        statusUI?.UpdateHealth(Hp, data.maxHp);

        if (IsDead)
            Die();
    }

    private void Die()
    {
        CombatTurnController turnController =
            FindFirstObjectByType<CombatTurnController>();

        turnController?.RefreshInitiative();

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (battlefield != null)
            battlefield.TryUnregister(gameObject);
    }
}