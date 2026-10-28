using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ReactionAbilityController
{
    private readonly CombatTurnController turnController;
    private readonly ReactionAbilityContext context;

    private readonly List<ReactionEntry> reactions = new();
    private readonly Dictionary<PlayerAbilities, Action> abilityHandlers = new();

    private readonly struct ReactionEntry
    {
        public readonly CombatParticipant Owner;
        public readonly IReactionAbility Reaction;

        public ReactionEntry(
            CombatParticipant owner,
            IReactionAbility reaction)
        {
            Owner = owner;
            Reaction = reaction;
        }
    }

    public ReactionAbilityController(CombatTurnController turnController)
    {
        this.turnController = turnController;
        context = new ReactionAbilityContext(turnController.CurrentRound);
    }

    public void Initialize()
    {
        CombatDamageService.DamageResolved += HandleDamageResolved;
        RefreshReactionCache();
    }

    public void Dispose()
    {
        CombatDamageService.DamageResolved -= HandleDamageResolved;

        foreach (var entry in abilityHandlers)
        {
            if (entry.Key != null)
                entry.Key.ActiveAbilitiesChanged -= entry.Value;
        }

        abilityHandlers.Clear();
        reactions.Clear();
    }

    public void Reset()
    {
        context.Reset();
    }

    public void RefreshReactionCache()
    {
        RefreshAbilitySubscriptions();
        reactions.Clear();

        CombatParticipant[] participants =
            UnityEngine.Object.FindObjectsByType<CombatParticipant>(
                FindObjectsSortMode.None);

        foreach (CombatParticipant participant in participants)
        {
            if (participant == null)
                continue;

            IAbilityProvider provider =
                participant.GetComponent<IAbilityProvider>();

            if (provider == null)
                continue;

            foreach (Ability ability in provider.Abilities)
            {
                if (ability == null ||
                    ability.activationType != AbilityActivationType.Reaction ||
                    ability is not IReactionAbility reaction)
                {
                    continue;
                }

                reactions.Add(new ReactionEntry(participant, reaction));
            }
        }
    }

    private void RefreshAbilitySubscriptions()
    {
        PlayerAbilities[] players =
            UnityEngine.Object.FindObjectsByType<PlayerAbilities>(
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

            Action handler = RefreshReactionCache;

            abilityHandlers.Add(player, handler);
            player.ActiveAbilitiesChanged += handler;
        }
    }

    private void HandleDamageResolved(CombatDamageService.DamageResult result)
    {
        if (result == null ||
            result.Caster == null ||
            result.Target == null ||
            result.FinalDamage <= 0)
        {
            return;
        }

        ReactionEntry[] snapshot = reactions.ToArray();

        foreach (ReactionEntry entry in snapshot)
        {
            CombatParticipant owner = entry.Owner;

            if (owner == null || !owner.IsAlive)
                continue;

            entry.Reaction.React(
                owner.gameObject,
                result.Caster,
                result.Target,
                result.AttackType,
                result.DamageType,
                result.FinalDamage,
                context);
        }
    }
}