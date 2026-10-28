using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class AbilityCooldownController : MonoBehaviour
{
    private readonly Dictionary<Ability, int> cooldowns = new();

    public event Action CooldownsChanged;

    public int GetCooldownRemaining(Ability ability)
    {
        if (ability == null)
            return 0;

        return cooldowns.TryGetValue(ability, out int remaining)
            ? remaining
            : 0;
    }

    public bool CanUseAbility(Ability ability)
    {
        return ability != null &&
               GetCooldownRemaining(ability) == 0;
    }

    public void StartCooldown(Ability ability)
    {
        if (ability == null)
            return;

        if (ability.cooldownTurns <= 0)
        {
            if (cooldowns.Remove(ability))
                CooldownsChanged?.Invoke();

            return;
        }

        cooldowns[ability] = ability.cooldownTurns;
        CooldownsChanged?.Invoke();
    }

    public void AdvanceCooldowns()
    {
        if (cooldowns.Count == 0)
            return;

        List<Ability> abilitiesOnCooldown = new(cooldowns.Keys);

        foreach (Ability ability in abilitiesOnCooldown)
        {
            int remaining = cooldowns[ability] - 1;

            if (remaining <= 0)
                cooldowns.Remove(ability);
            else
                cooldowns[ability] = remaining;
        }

        CooldownsChanged?.Invoke();
    }

    public void ResetCooldowns()
    {
        if (cooldowns.Count == 0)
            return;

        cooldowns.Clear();
        CooldownsChanged?.Invoke();
    }
}