using System.Collections.Generic;
using UnityEngine;

public sealed class PassiveAbilityController
{
    private readonly CombatTurnController turnController;


    private readonly Dictionary<PassiveKey, object> modifierSources = new();

    private readonly Dictionary<StatusController, System.Action> statusHandlers = new();

    private readonly Dictionary<PlayerAbilities, System.Action> abilityHandlers = new();

    private bool isRecalculating;

    private readonly struct PassiveKey
    {
        public readonly GameObject Owner;
        public readonly Ability Ability;

        public PassiveKey(GameObject owner, Ability ability)
        {
            Owner = owner;
            Ability = ability;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Owner != null ? Owner.GetHashCode() : 0) * 397 ^
                       (Ability != null ? Ability.GetHashCode() : 0);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is PassiveKey other &&
                   Owner == other.Owner &&
                   Ability == other.Ability;
        }
    }

    public PassiveAbilityController(CombatTurnController turnController)
    {
        this.turnController = turnController;
    }

    public void Initialize()
    {
        RefreshSubscriptions();
        RecalculateAll();
    }

    public void Dispose()
    {
        foreach (var entry in statusHandlers)
        {
            if (entry.Key != null)
                entry.Key.StatusesChanged -= entry.Value;
        }

        statusHandlers.Clear();

        foreach (var entry in abilityHandlers)
        {
            if (entry.Key != null)
                entry.Key.ActiveAbilitiesChanged -= entry.Value;
        }

        abilityHandlers.Clear();

        CombatParticipant[] participants =
            Object.FindObjectsByType<CombatParticipant>(
                FindObjectsSortMode.None);

        RemoveAllModifiers(participants);
        modifierSources.Clear();
    }

    private void RefreshSubscriptions()
    {
        StatusController[] statuses =
            Object.FindObjectsByType<StatusController>(
                FindObjectsSortMode.None);

        HashSet<StatusController> currentStatuses = new(statuses);

        List<StatusController> removedStatuses = new();

        foreach (var entry in statusHandlers)
        {
            if (entry.Key == null ||
                !currentStatuses.Contains(entry.Key))
            {
                if (entry.Key != null)
                    entry.Key.StatusesChanged -= entry.Value;

                removedStatuses.Add(entry.Key);
            }
        }

        foreach (StatusController status in removedStatuses)
            statusHandlers.Remove(status);

        foreach (StatusController status in statuses)
        {
            if (status == null || statusHandlers.ContainsKey(status))
                continue;

            System.Action handler = RecalculateAll;

            statusHandlers.Add(status, handler);
            status.StatusesChanged += handler;
        }

        PlayerAbilities[] players =
            Object.FindObjectsByType<PlayerAbilities>(
                FindObjectsSortMode.None);

        HashSet<PlayerAbilities> currentPlayers = new(players);

        List<PlayerAbilities> removedPlayers = new();

        foreach (var entry in abilityHandlers)
        {
            if (entry.Key == null ||
                !currentPlayers.Contains(entry.Key))
            {
                if (entry.Key != null)
                    entry.Key.ActiveAbilitiesChanged -= entry.Value;

                removedPlayers.Add(entry.Key);
            }
        }

        foreach (PlayerAbilities player in removedPlayers)
            abilityHandlers.Remove(player);

        foreach (PlayerAbilities player in players)
        {
            if (player == null || abilityHandlers.ContainsKey(player))
                continue;

            System.Action handler = RecalculateAll;

            abilityHandlers.Add(player, handler);
            player.ActiveAbilitiesChanged += handler;
        }
    }

    public void RecalculateAll()
    {
        if (isRecalculating)
            return;

        isRecalculating = true;

        try
        {
            RefreshSubscriptions();

            CombatParticipant[] participants =
                Object.FindObjectsByType<CombatParticipant>(
                    FindObjectsSortMode.None);

            Dictionary<CombatParticipant, int> previousSpeeds = new();

            foreach (CombatParticipant participant in participants)
            {
                if (participant != null)
                    previousSpeeds[participant] = participant.Speed;
            }

            // Удаляем предыдущие результаты расчёта пассивок.
            RemoveAllModifiers(participants);

            HashSet<PassiveKey> activeKeys = new();

            foreach (CombatParticipant owner in participants)
            {
                if (owner == null || !owner.IsAlive)
                    continue;
                
                IAbilityProvider provider =
                    owner.GetComponent<IAbilityProvider>();

                if (provider == null)
                    continue;

                foreach (Ability ability in provider.Abilities)
                {
                    if (ability == null ||
                        ability.activationType != AbilityActivationType.Passive ||
                        ability is not IPassiveAbility)
                    {
                        continue;
                    }

                    activeKeys.Add(new PassiveKey(owner.gameObject, ability));
                }
            }

            List<PassiveKey> staleKeys = new();

            foreach (PassiveKey key in modifierSources.Keys)
            {
                if (!activeKeys.Contains(key))
                    staleKeys.Add(key);
            }

            foreach (PassiveKey key in staleKeys)
                modifierSources.Remove(key);

            PassiveAbilityContext context =
                new(this, participants);

            foreach (CombatParticipant owner in participants)
            {
                if (owner == null || !owner.IsAlive)
                    continue;

                IAbilityProvider provider =
                    owner.GetComponent<IAbilityProvider>();

                if (provider == null)
                    continue;

                foreach (Ability ability in provider.Abilities)
                {
                    if (ability is not IPassiveAbility passive ||
                        ability.activationType != AbilityActivationType.Passive)
                    {
                        continue;
                    }

                    passive.Recalculate(owner.gameObject, context);
                }
            }

            bool speedChanged = false;

            foreach (CombatParticipant participant in participants)
            {
                if (participant == null ||
                    !previousSpeeds.TryGetValue(
                        participant, out int previousSpeed))
                {
                    continue;
                }

                if (participant.Speed != previousSpeed)
                {
                    speedChanged = true;
                    break;
                }
            }

            if (speedChanged)
            {
                // INITIATIVE: немедленный пересчёт оставшейся очереди
                // при изменении скорости. Уже начавшийся ход сохраняется.
                turnController.RefreshInitiativeOrder();
            }
        }
        finally
        {
            isRecalculating = false;
        }
    }

    internal void SetSpeedModifier(
        GameObject owner,
        Ability ability,
        CombatParticipant target,
        int modifier)
    {
        if (owner == null || ability == null || target == null)
            return;

        PassiveKey key = new(owner, ability);

        if (!modifierSources.TryGetValue(key, out object source))
        {
            source = new object();
            modifierSources.Add(key, source);
        }

        target.SetSpeedModifier(source, modifier);
    }

    private void RemoveAllModifiers(CombatParticipant[] participants)
    {
        foreach (object source in modifierSources.Values)
        {
            foreach (CombatParticipant participant in participants)
                participant?.RemoveSpeedModifier(source);
        }
    }

}