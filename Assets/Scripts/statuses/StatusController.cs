using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusController : MonoBehaviour
{
    private readonly List<StatusInstance> activeStatuses = new();
    private readonly List<StackLimitModifier> stackLimitModifiers = new();

    private int nextModifierId = 1;

    public IReadOnlyList<StatusInstance> ActiveStatuses => activeStatuses;

    public event Action StatusesChanged;

    private sealed class StackLimitModifier
    {
        public int Id;
        public StatusEffectData Data;
        public int Bonus;
        public int? RemainingTurns;
    }

    /// <summary>
    /// Добавляет модификатор максимума стаков для этого персонажа.
    /// durationOwnerTurns: положительное число — длительность в ходах владельца;
    /// -1 — бессрочный модификатор.
    /// Возвращает ID, нужный для досрочного снятия.
    /// </summary>
    public int AddStackLimitModifier(
        StatusEffectData data,
        int bonus,
        int durationOwnerTurns)
    {
        if (data == null || bonus == 0 || durationOwnerTurns == 0 ||
            durationOwnerTurns < -1)
        {
            return 0;
        }

        int id = nextModifierId++;

        stackLimitModifiers.Add(new StackLimitModifier
        {
            Id = id,
            Data = data,
            Bonus = bonus,
            RemainingTurns = durationOwnerTurns > 0
                ? durationOwnerTurns
                : null
        });

        return id;
    }

    public bool RemoveStackLimitModifier(int modifierId)
    {
        int removed = stackLimitModifiers.RemoveAll(
            modifier => modifier.Id == modifierId
        );

        return removed > 0;
    }

    public int GetMaxStacks(StatusEffectData data)
    {
        if (data == null)
            return 0;

        int maxStacks = data.MaxDurationStacks;

        foreach (StackLimitModifier modifier in stackLimitModifiers)
        {
            if (modifier.Data == data)
                maxStacks += modifier.Bonus;
        }

        return Mathf.Max(1, maxStacks);
    }

    private void OnEnable()
    {
        CombatTurnController.ActionCompleted += HandleActionCompleted;
    }

    private void OnDisable()
    {
        CombatTurnController.ActionCompleted -= HandleActionCompleted;
    }

    private void HandleActionCompleted(GameObject actor)
    {
        if (actor != gameObject)
            return;

        StatusInstance[] snapshot = activeStatuses.ToArray();

        foreach (StatusInstance status in snapshot)
        {
            if (status.Data == null ||
                !activeStatuses.Contains(status))
            {
                continue;
            }

            status.Data.OnAction(gameObject, this);

            if (status.Data is BleedStatusData &&
                activeStatuses.Contains(status))
            {
                ConsumeStack(status.Data);
            }
        }
    }

    public void ApplyStatus(StatusEffectData data)
    {
        ApplyStatus(data, -1);
    }

    public bool ApplyStatus(
        StatusEffectData data,
        int durationStacks)
    {
        if (data == null)
            return false;

        if (data is PoisonStatusData)
        {
            if (durationStacks <= 0)
                durationStacks = data.DefaultDurationStacks;

            StatusInstance poison = activeStatuses.Find(
                status => status.Data == data
            );

            if (poison != null)
            {
                poison.AddStacks(durationStacks, GetMaxStacks(data));
            }
            else
            {
                activeStatuses.Add(
                    new StatusInstance(
                        data,
                        durationStacks,
                        GetMaxStacks(data)
                    )
                );
            }

            StatusesChanged?.Invoke();
            return true;
        }

        if (data.DurationMode == StatusDurationMode.Timed)
        {
            if (durationStacks <= 0)
                durationStacks = data.DefaultDurationStacks;

            if (durationStacks <= 0)
                return false;
        }

        StatusInstance existing = activeStatuses.Find(
            status => status.Data == data
        );

        int maxStacks = GetMaxStacks(data);

        if (existing != null)
        {
            switch (data.ReapplicationPolicy)
            {
                case StatusReapplicationPolicy.Ignore:
                    return false;

                case StatusReapplicationPolicy.RefreshDuration:
                    existing.RefreshDuration(durationStacks, maxStacks);
                    break;

                case StatusReapplicationPolicy.AddDuration:
                    existing.AddDuration(durationStacks, maxStacks);
                    break;

                case StatusReapplicationPolicy.Replace:
                    activeStatuses.Remove(existing);
                    activeStatuses.Add(
                        new StatusInstance(data, durationStacks, maxStacks)
                    );
                    break;
            }

            StatusesChanged?.Invoke();
            return true;
        }

        activeStatuses.Add(
            new StatusInstance(data, durationStacks, maxStacks)
        );

        StatusesChanged?.Invoke();
        return true;
    }

    public int GetStacks(StatusEffectData data)
    {
        StatusInstance status = activeStatuses.Find(
            item => item.Data == data
        );

        return status?.RemainingStacks ?? 0;
    }

    public bool ConsumeStack(StatusEffectData data)
    {
        StatusInstance status = activeStatuses.Find(
            item => item.Data == data
        );

        if (status == null)
            return false;

        bool expired = status.ConsumeStack();

        if (expired)
            activeStatuses.Remove(status);

        StatusesChanged?.Invoke();

        return true;
    }

    public bool TryApplyStatusIfAbsent(
        StatusEffectData data,
        int durationStacks = -1)
    {
        if (data == null)
            return false;

        if (activeStatuses.Exists(status => status.Data == data))
            return false;

        return ApplyStatus(data, durationStacks);
    }

    public bool HasStatus(StatusEffectData data)
    {
        if (data == null)
            return false;

        return activeStatuses.Exists(
            status => status.Data == data
        );
    }

    public bool RemoveStatus(StatusEffectData data)
    {
        if (data == null)
            return false;

        int removed = activeStatuses.RemoveAll(
            status => status.Data == data
        );

        if (removed > 0)
            StatusesChanged?.Invoke();

        return removed > 0;
    }

    public void ProcessTurnStart()
    {
        ProcessTiming(StatusEffectTiming.TurnStart);
    }

    public void ProcessTurnEnd()
    {
        ProcessTiming(StatusEffectTiming.TurnEnd);

        StatusInstance[] snapshot = activeStatuses.ToArray();
        bool statusesChanged = false;

        foreach (StatusInstance status in snapshot)
        {
            if (!activeStatuses.Contains(status) ||
                status.Data == null ||
                !status.HasDuration)
            {
                continue;
            }

            if (status.Data is PoisonStatusData)
                continue;

            if (status.AdvanceDuration())
            {
                activeStatuses.Remove(status);
                statusesChanged = true;
            }
        }

        ProcessStackLimitModifierDurations();

        if (statusesChanged)
            StatusesChanged?.Invoke();
    }

    private void ProcessStackLimitModifierDurations()
    {
        for (int i = stackLimitModifiers.Count - 1; i >= 0; i--)
        {
            StackLimitModifier modifier = stackLimitModifiers[i];

            if (!modifier.RemainingTurns.HasValue)
                continue;

            modifier.RemainingTurns--;

            if (modifier.RemainingTurns <= 0)
                stackLimitModifiers.RemoveAt(i);
        }
    }

    private void ProcessTiming(StatusEffectTiming timing)
    {
        StatusInstance[] snapshot = activeStatuses.ToArray();

        foreach (StatusInstance status in snapshot)
        {
            if (status.Data == null ||
                !activeStatuses.Contains(status))
            {
                continue;
            }

            StatusEffectTiming effectTiming = status.Data.Timing;

            bool shouldExecute =
                effectTiming == StatusEffectTiming.Both ||
                effectTiming == timing;

            if (!shouldExecute)
                continue;

            if (timing == StatusEffectTiming.TurnStart)
                status.Data.OnTurnStart(gameObject);
            else if (timing == StatusEffectTiming.TurnEnd)
                status.Data.OnTurnEnd(gameObject);
        }
    }

    public int ProcessIncomingDamage(int damage)
    {
        if (damage <= 0)
            return damage;

        bool statusesChanged = false;
        StatusInstance[] snapshot = activeStatuses.ToArray();

        foreach (StatusInstance status in snapshot)
        {
            if (status.Data == null ||
                !activeStatuses.Contains(status))
            {
                continue;
            }

            damage = status.Data.ModifyIncomingDamage(damage);
        }

        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            StatusEffectData data = activeStatuses[i].Data;

            if (data != null && data.ConsumeOnDamage)
            {
                activeStatuses.RemoveAt(i);
                statusesChanged = true;
            }
        }

        if (statusesChanged)
            StatusesChanged?.Invoke();

        return Mathf.Max(0, damage);
    }

    public int AddBarrier(BarrierStatusData data, int amount)
    {
        if (data == null || amount <= 0)
            return 0;

        StatusInstance barrier = activeStatuses.Find(
            status => status.Data == data
        );

        if (barrier == null)
        {
            barrier = new StatusInstance(data, -1, 1);
            activeStatuses.Add(barrier);
        }

        int previousValue = barrier.CurrentValue;
        barrier.AddValue(amount, data.MaxBarrier);

        int gained = barrier.CurrentValue - previousValue;

        if (gained > 0)
            StatusesChanged?.Invoke();

        return gained;
    }

    public int AbsorbDamageWithBarrier(int damage)
    {
        if (damage <= 0)
            return 0;

        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            StatusInstance status = activeStatuses[i];

            if (status.Data is not BarrierStatusData)
                continue;

            int absorbed = status.ConsumeValue(damage);

            if (status.CurrentValue <= 0)
                activeStatuses.RemoveAt(i);

            if (absorbed > 0)
                StatusesChanged?.Invoke();

            return absorbed;
        }

        return 0;
    }

    public int AddInstability(InstabilityStatusData data, int amount)
    {
        if (data == null || amount <= 0)
            return 0;

        StatusInstance instability = activeStatuses.Find(
            status => status.Data == data
        );

        if (instability == null)
        {
            instability = new StatusInstance(data, -1, 1);
            activeStatuses.Add(instability);
        }

        int previousValue = instability.CurrentValue;

        instability.AddValue(amount, data.Threshold);

        int gained = instability.CurrentValue - previousValue;

        if (gained <= 0)
            return 0;

        StatusesChanged?.Invoke();

        if (previousValue < data.Threshold &&
            instability.CurrentValue >= data.Threshold)
        {
            CombatLog combatLog =
                FindFirstObjectByType<CombatLog>();

            combatLog?.AddLog(
                $"{gameObject.name} reaches {data.Threshold} instability!"
            );
        }

        return gained;
    }
}