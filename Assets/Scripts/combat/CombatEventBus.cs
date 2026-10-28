using System;
using UnityEngine;

public static class CombatEventBus
{
    public static event Action<CombatEvent> EventRaised;

    public static void Publish(CombatEvent combatEvent)
    {
        if (combatEvent == null)
            return;

        EventRaised?.Invoke(combatEvent);
    }
}