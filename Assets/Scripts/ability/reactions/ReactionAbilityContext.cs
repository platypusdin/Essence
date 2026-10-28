using System.Collections.Generic;
using UnityEngine;

public sealed class ReactionAbilityContext
{
    private readonly Dictionary<ReactionKey, int> lastTriggeredRounds = new();

    public int CurrentRound { get; }

    public ReactionAbilityContext(int currentRound)
    {
        CurrentRound = currentRound;
    }

    public bool TryTriggerOncePerRound(GameObject owner, Ability ability)
    {
        if (owner == null || ability == null)
            return false;

        ReactionKey key = new(owner, ability);

        if (lastTriggeredRounds.TryGetValue(key, out int lastRound) &&
            lastRound == CurrentRound)
        {
            return false;
        }

        lastTriggeredRounds[key] = CurrentRound;
        return true;
    }

    public void Reset()
    {
        lastTriggeredRounds.Clear();
    }

    private readonly struct ReactionKey
    {
        private readonly GameObject owner;
        private readonly Ability ability;

        public ReactionKey(GameObject owner, Ability ability)
        {
            this.owner = owner;
            this.ability = ability;
        }

        public override bool Equals(object obj)
        {
            return obj is ReactionKey other &&
                   owner == other.owner &&
                   ability == other.ability;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (owner != null ? owner.GetHashCode() : 0) * 397 ^
                       (ability != null ? ability.GetHashCode() : 0);
            }
        }
    }
}