using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CombatEventController
{
    private bool isInitialized;

    public void Initialize()
    {
        if (isInitialized)
            return;

        CombatEventBus.EventRaised += HandleCombatEvent;
        isInitialized = true;
    }

    public void Dispose()
    {
        if (!isInitialized)
            return;

        CombatEventBus.EventRaised -= HandleCombatEvent;
        isInitialized = false;
    }

    private void HandleCombatEvent(CombatEvent combatEvent)
    {
        if (combatEvent == null)
            return;

        CombatParticipant[] participants =
            UnityEngine.Object.FindObjectsByType<CombatParticipant>(
                FindObjectsSortMode.None);

        foreach (CombatParticipant participant in participants)
        {
            if (!(participant != null && participant.IsAlive))
                continue;

            IAbilityProvider provider = participant.GetComponent<IAbilityProvider>();
            if (provider == null)
                continue;

            foreach (Ability ability in provider.Abilities)
            {
                if (ability == null ||
                    ability.activationType == AbilityActivationType.Active ||
                    ability is not ICombatEventReaction reaction)
                {
                    continue;
                }

                reaction.OnCombatEvent(
                    participant.gameObject,
                    combatEvent);
            }
        }
    }
}