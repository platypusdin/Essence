using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class AbilitySetBonusController : MonoBehaviour
{
    [SerializeField] private AbilitySetCatalog catalog;

    private PlayerAbilities playerAbilities;

    private readonly Dictionary<AbilitySet, int> selectedCounts = new();

    public event Action SetBonusesChanged;

    public void RegisterPlayer(PlayerAbilities newPlayerAbilities)
    {
        if (playerAbilities == newPlayerAbilities)
            return;

        if (playerAbilities != null)
        {
            playerAbilities.SelectedAbilitiesChanged -= Recalculate;
        }

        playerAbilities = newPlayerAbilities;

        if (playerAbilities != null)
        {
            playerAbilities.SelectedAbilitiesChanged += Recalculate;
        }

        Recalculate();
    }

    private void OnDestroy()
    {
        if (playerAbilities != null)
        {
            playerAbilities.SelectedAbilitiesChanged -= Recalculate;
        }
    }

    private void Recalculate()
    {
        selectedCounts.Clear();

        if (catalog != null && playerAbilities != null)
        {
            foreach (AbilitySet abilitySet in catalog.AbilitySets)
            {
                if (abilitySet == null)
                    continue;

                int count =
                    playerAbilities.GetSelectedAbilityCountInSet(abilitySet);

                selectedCounts[abilitySet] = count;
            }
        }

        SetBonusesChanged?.Invoke();
    }

    public int GetSelectedCount(AbilitySet abilitySet)
    {
        if (abilitySet == null)
            return 0;

        return selectedCounts.TryGetValue(abilitySet, out int count)
            ? count
            : 0;
    }

    public bool IsBonusActive(
        AbilitySet abilitySet,
        AbilitySet.BonusThreshold bonus)
    {
        if (abilitySet == null || bonus == null)
            return false;

        return GetSelectedCount(abilitySet) >= bonus.RequiredAbilityCount;
    }
}