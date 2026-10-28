using UnityEngine;

public enum StatusDurationMode
{
    Timed,
    UntilRemoved
}

public enum StatusEffectTiming
{
    None,
    TurnStart,
    TurnEnd,
    Both
}

public enum StatusReapplicationPolicy
{
    Ignore,
    RefreshDuration,
    AddDuration,
    Replace
}

public abstract class StatusEffectData : ScriptableObject
{
    [Header("Display")]
    [SerializeField] private string effectName;
    [SerializeField] private Sprite icon;

    [Header("Duration")]
    [SerializeField] private StatusDurationMode durationMode = StatusDurationMode.Timed;

    [SerializeField, Min(1)] private int defaultDurationStacks = 1;
    [SerializeField, Min(1)] private int maxDurationStacks = 5;

    public int MaxDurationStacks => maxDurationStacks;

    [Header("Execution")]
    [SerializeField]
    private StatusEffectTiming timing = StatusEffectTiming.TurnStart;

    [Header("Reapplication")]
    [SerializeField]
    private StatusReapplicationPolicy reapplicationPolicy = StatusReapplicationPolicy.Ignore;

    [Header("Removal")]
    [SerializeField] private bool consumeOnDamage;

    public string EffectName => effectName;
    public Sprite Icon => icon;

    public StatusDurationMode DurationMode => durationMode;
    public int DefaultDurationStacks => defaultDurationStacks;

    public StatusEffectTiming Timing => timing;

    public StatusReapplicationPolicy ReapplicationPolicy => reapplicationPolicy;

    public virtual bool ConsumeOnDamage => consumeOnDamage;

    public virtual int ModifyIncomingDamage(int damage)
    {
        return damage;
    }

    public virtual void OnTurnStart(GameObject owner)
    {
    }

    public virtual void OnTurnEnd(GameObject owner)
    {
    }

    public virtual void OnAction(GameObject owner, StatusController statusController)
    {
    }
}