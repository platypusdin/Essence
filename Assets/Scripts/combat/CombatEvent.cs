using UnityEngine;

public enum CombatEventType
{
    StatusApplied,
    StatusRemoved,
    StatusKilled,
    UnitDied,
    DamageDealt,
    DamageReceived,
    TurnStarted,
    TurnEnded,
    RoundStarted
}

public sealed class CombatEvent
{
    public CombatEventType Type { get; }
    public GameObject Source { get; }
    public GameObject Target { get; }
    public StatusEffectData Status { get; }
    public int Value { get; }

    public CombatEvent(
        CombatEventType type,
        GameObject source = null,
        GameObject target = null,
        StatusEffectData status = null,
        int value = 0)
    {
        Type = type;
        Source = source;
        Target = target;
        Status = status;
        Value = value;
    }
}