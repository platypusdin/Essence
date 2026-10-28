using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int maxAp = 4;
    [SerializeField, Min(0)] private int maxBarrier = 40;

    private CharacterStatusUI statusUI;
    private StatusController statusController;

    public int MaxHp => maxHp;
    public int MaxAp => maxAp;
    public int MaxBarrier => maxBarrier;

    public int Hp { get; private set; }
    public int Ap { get; private set; }
    public int Barrier { get; private set; }

    public bool IsDead => Hp <= 0;

    public event Action<int, int> HpChanged;
    public event Action<int, int> ApChanged;
    public event Action<int, int> BarrierChanged;
    public event Action<int> ApSpent;
    public event Action Died;

    private void Awake()
    {
        statusUI = GetComponentInChildren<CharacterStatusUI>();
        statusController = GetComponent<StatusController>();

        Hp = maxHp;
        Ap = maxAp;
        Barrier = 0;

        UpdateHealthUI();
    }

    public int AddBarrier(int amount)
    {
        if (amount <= 0 || IsDead)
            return 0;

        int previousBarrier = Barrier;
        Barrier = Mathf.Min(maxBarrier, Barrier + amount);

        int gainedBarrier = Barrier - previousBarrier;

        if (gainedBarrier > 0)
            BarrierChanged?.Invoke(Barrier, MaxBarrier);

        return gainedBarrier;
    }

    public int TakeDamage(int amount, bool processStatuses = true)
    {
        if (IsDead || amount <= 0)
            return 0;
        if (processStatuses && statusController != null)
        {
            amount = statusController.ProcessIncomingDamage(amount);
            amount = Mathf.Max(0, amount);

            int absorbed = statusController.AbsorbDamageWithBarrier(amount);
            amount -= absorbed;
        }

        amount = Mathf.Max(0, amount);

        // Barrier absorbs incoming damage before HP is reduced.
        int absorbedDamage = Mathf.Min(Barrier, amount);

        if (absorbedDamage > 0)
        {
            Barrier -= absorbedDamage;
            amount -= absorbedDamage;

            BarrierChanged?.Invoke(Barrier, MaxBarrier);
        }

        int actualDamage = Mathf.Min(Hp, amount);
        Hp -= actualDamage;

        UpdateHealthUI();
        HpChanged?.Invoke(Hp, MaxHp);

        if (IsDead)
            Died?.Invoke();

        return actualDamage;
    }

    public void RestoreHp(int amount)
    {
        if (amount <= 0 || IsDead)
            return;

        Hp = Mathf.Min(maxHp, Hp + amount);

        UpdateHealthUI();
        HpChanged?.Invoke(Hp, MaxHp);
    }

    public bool TrySpendAp(int amount)
    {
        if (Ap < amount)
            return false;

        Ap -= amount;
        ApSpent?.Invoke(amount);
        ApChanged?.Invoke(Ap, MaxAp);

        return true;
    }

    public void RestoreAp(int amount)
    {
        Ap = Mathf.Min(maxAp, Ap + amount);
        ApChanged?.Invoke(Ap, MaxAp);
    }

    public void RestoreFullAp()
    {
        Ap = MaxAp;
        ApChanged?.Invoke(Ap, MaxAp);
    }

    private void UpdateHealthUI()
    {
        statusUI?.UpdateHealth(Hp, MaxHp);
    }
}