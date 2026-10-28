
using UnityEngine;

public class StatusInstance
{
    public StatusEffectData Data { get; }

    public int? RemainingStacks { get; private set; }

    public int CurrentValue { get; private set; }

    public bool HasDuration => RemainingStacks.HasValue;

    public StatusInstance(
        StatusEffectData data,
        int durationStacks,
        int maxDurationStacks)
    {
        Data = data;

        RemainingStacks =
            data.DurationMode == StatusDurationMode.Timed
                ? ClampDuration(durationStacks, maxDurationStacks)
                : null;
    }

    public void AddValue(int amount, int maxValue)
    {
        if (amount <= 0)
            return;

        CurrentValue = Mathf.Min(maxValue, CurrentValue + amount);
    }

    public int ConsumeValue(int amount)
    {
        if (amount <= 0 || CurrentValue <= 0)
            return 0;

        int consumed = Mathf.Min(CurrentValue, amount);
        CurrentValue -= consumed;

        return consumed;
    }

    public void RefreshDuration(
        int durationStacks,
        int maxDurationStacks)
    {
        if (HasDuration)
        {
            RemainingStacks = ClampDuration(
                durationStacks,
                maxDurationStacks
            );
        }
    }

    public void AddDuration(
        int durationStacks,
        int maxDurationStacks)
    {
        if (!HasDuration || durationStacks <= 0)
            return;

        int newDuration = RemainingStacks.Value + durationStacks;
        int effectiveLimit = Mathf.Max(
            RemainingStacks.Value,
            maxDurationStacks
        );

        RemainingStacks = ClampDuration(newDuration, effectiveLimit);
    }

    public void AddStacks(int amount, int maxStacks)
    {
        if (!HasDuration || amount <= 0)
            return;

        RemainingStacks = Mathf.Min(
            maxStacks,
            RemainingStacks.Value + amount
        );
    }

    public bool ConsumeStack()
    {
        if (!HasDuration)
            return false;

        RemainingStacks--;

        return RemainingStacks <= 0;
    }

    public bool AdvanceDuration()
    {
        return ConsumeStack();
    }

    private static int ClampDuration(
        int durationStacks,
        int maxDurationStacks)
    {
        return Mathf.Clamp(
            durationStacks,
            1,
            Mathf.Max(1, maxDurationStacks)
        );
    }
}
