using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyAI))]
public class Enemy : MonoBehaviour, IDamageable, IAbilityProvider
{
    [SerializeField] private int fallbackMaxHp = 50;

    private SpriteRenderer spriteRenderer;
    private bool isDead;
    private int position;
    private int maxHp;
    private EnemyData data;
    private CharacterStatusUI statusUI;
    private StatusController statusController;
    private Battlefield battlefield;

    public bool IsDead => isDead;
    public EnemyData Data => data;

    public IReadOnlyList<Ability> Abilities => data != null
        ? data.abilities ?? System.Array.Empty<Ability>()
        : System.Array.Empty<Ability>();
    public int Hp { get; private set; }

    public int Position
    {
        get
        {
            if (battlefield != null &&
                battlefield.TryFindRank(
                    gameObject,
                    out BattleRank rank) &&
                rank.Side == BattleSide.Enemies)
            {
                return rank.Index;
            }

            return position;
        }
    }

    public event Action<Enemy> Died;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        statusUI = GetComponentInChildren<CharacterStatusUI>();
        statusController = GetComponent<StatusController>();
    }

    public void Initialize(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            Debug.LogError(
                $"Enemy: EnemyData is missing on {gameObject.name}."
            );
            return;
        }

        data = enemyData;
        maxHp = data.maxHp > 0 ? data.maxHp : fallbackMaxHp;
        Hp = maxHp;

        statusUI?.UpdateHealth(Hp, maxHp);

        if (spriteRenderer != null && data.sprite != null)
            spriteRenderer.sprite = data.sprite;

        EnemyAI ai = GetComponent<EnemyAI>();
        ai?.Initialize(data);

        CombatParticipant participant =
            GetComponent<CombatParticipant>();

        if (participant != null)
            participant.SetSpeed(data.speed);
    }

    public void ConfigurePosition(int newPosition)
    {
        position = newPosition;
    }

    public void ConfigureBattlefield(Battlefield newBattlefield)
    {
        battlefield = newBattlefield;

        EnemyAI ai = GetComponent<EnemyAI>();
        ai?.ConfigureBattlefield(newBattlefield);
    }

    public int TakeDamage(int amount, bool processStatuses = true)
    {
        if (isDead || amount <= 0)
            return 0;

        if (processStatuses && statusController != null)
            amount = statusController.ProcessIncomingDamage(amount);

        amount = Mathf.Max(0, amount);

        int actualDamage = Mathf.Min(Hp, amount);
        Hp -= actualDamage;

        Debug.Log($"{gameObject.name} HP: {Hp}/{maxHp}");

        statusUI?.UpdateHealth(Hp, maxHp);

        if (Hp == 0)
            Die();

        return actualDamage;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (spriteRenderer != null &&
            data != null &&
            data.corpseSprite != null)
        {
            spriteRenderer.sprite = data.corpseSprite;
        }

        CombatTurnController turnController =
            FindFirstObjectByType<CombatTurnController>();

        turnController?.RefreshInitiative();

        Died?.Invoke(this);

        Debug.Log($"{gameObject.name} died.");
    }

    private void OnMouseDown()
    {
        PlayerAbilities playerAbilities =
            FindFirstObjectByType<PlayerAbilities>();

        playerAbilities?.SelectTarget(gameObject);
    }

    public void ReleaseBattlefieldRank()
    {
        if (battlefield == null)
            return;

        battlefield.TryUnregister(gameObject);
        battlefield = null;
    }

    private void OnDestroy()
    {
        ReleaseBattlefieldRank();
    }
}