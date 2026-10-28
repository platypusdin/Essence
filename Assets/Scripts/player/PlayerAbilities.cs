
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour, IAbilityProvider
{
    public const int MaxSelectedAbilities = 6;

    [Header("Ability Loadout")]
    [SerializeField] private Ability[] availableAbilities;
    [SerializeField] private Ability[] abilities;

    private AbilityButton[] buttons = Array.Empty<AbilityButton>();
    private GameObject currentTarget;
    private Battlefield battlefield;
    private CombatTargetingService targetingService;
    private AbilityCooldownController cooldownController;

    public IReadOnlyList<Ability> Abilities =>
        abilities ?? Array.Empty<Ability>();

    public Ability[] AvailableAbilities =>
        availableAbilities ?? Array.Empty<Ability>();

    public event Action ActiveAbilitiesChanged;
    public event Action SelectedAbilitiesChanged;

    private void Awake()
    {
        cooldownController = GetComponent<AbilityCooldownController>();
    }

    private void OnEnable()
    {
        if (cooldownController == null)
            cooldownController = GetComponent<AbilityCooldownController>();

        cooldownController.CooldownsChanged += UpdateButtons;
    }

    private void OnDisable()
    {
        if (cooldownController != null)
            cooldownController.CooldownsChanged -= UpdateButtons;
    }

    public void ConfigureBattlefield(Battlefield newBattlefield)
    {
        battlefield = newBattlefield;

        targetingService = battlefield != null
            ? new CombatTargetingService(battlefield)
            : null;

        UpdateButtons();
    }

    public bool SetSelectedAbilities(IList<Ability> selectedAbilities)
    {
        if (selectedAbilities == null ||
            selectedAbilities.Count > MaxSelectedAbilities)
        {
            return false;
        }

        List<Ability> validated = new();

        foreach (Ability ability in selectedAbilities)
        {
            if (ability == null ||
                Array.IndexOf(AvailableAbilities, ability) < 0 ||
                validated.Contains(ability))
            {
                return false;
            }

            validated.Add(ability);
        }

        abilities = validated.ToArray();

        ActiveAbilitiesChanged?.Invoke();
        SelectedAbilitiesChanged?.Invoke();
        UpdateButtons();

        return true;
    }

    public void RegisterButtons(AbilityButton[] abilityButtons)
    {
        buttons = abilityButtons ?? Array.Empty<AbilityButton>();
        UpdateButtons();
    }

    public void SelectTarget(GameObject target)
    {
        currentTarget = target;
        UpdateButtons();
    }

    public void ClearTarget()
    {
        currentTarget = null;
        UpdateButtons();
    }

    public GameObject GetTarget()
    {
        return currentTarget;
    }

    public bool CanUseAbility(Ability ability)
    {
        return cooldownController != null &&
               cooldownController.CanUseAbility(ability);
    }

    public void StartCooldown(Ability ability)
    {
        cooldownController?.StartCooldown(ability);
    }

    public bool TryAddAvailableAbility(Ability ability)
    {
        if (ability == null ||
            Array.IndexOf(AvailableAbilities, ability) >= 0)
        {
            return false;
        }

        List<Ability> updatedAbilities = new(AvailableAbilities)
        {
            ability
        };

        availableAbilities = updatedAbilities.ToArray();

        Debug.Log($"New ability unlocked: {ability.abilityName}");
        return true;
    }

    public int GetSelectedAbilityCountInSet(AbilitySet abilitySet)
    {
        if (abilitySet == null || abilitySet.Abilities == null)
            return 0;

        int count = 0;

        foreach (Ability ability in Abilities)
        {
            if (ability != null &&
                Array.IndexOf(abilitySet.Abilities, ability) >= 0)
            {
                count++;
            }
        }

        return count;
    }

    public Ability[] GetSelectedAbilities(AbilityActivationType type)
    {
        List<Ability> result = new();

        foreach (Ability ability in Abilities)
        {
            if (ability != null && ability.activationType == type)
                result.Add(ability);
        }

        return result.ToArray();
    }

    private void UpdateButtons()
    {
        if (buttons == null)
            return;

        Ability[] activeAbilities =
            GetSelectedAbilities(AbilityActivationType.Active);

        for (int i = 0; i < buttons.Length; i++)
        {
            AbilityButton abilityButton = buttons[i];

            if (abilityButton == null)
                continue;

            if (i >= activeAbilities.Length)
            {
                abilityButton.SetAvailability(false, 0, 0);
                continue;
            }

            Ability ability = activeAbilities[i];
            int cooldownRemaining =
                cooldownController != null
                    ? cooldownController.GetCooldownRemaining(ability)
                    : 0;

            bool available =
                currentTarget != null &&
                CanUseAbility(ability) &&
                IsTargetValidForAbility(ability, currentTarget);

            if (available && ability is RaiseVermitAbility raiseAbility)
            {
                AllyPlacementController placement =
                    FindFirstObjectByType<AllyPlacementController>();

                available =
                    placement != null &&
                    raiseAbility.AllyPrefab != null &&
                    raiseAbility.AllyData != null &&
                    placement.HasFreeSlot(gameObject);
            }

            abilityButton.SetAvailability(
                available,
                cooldownRemaining,
                ability.cooldownTurns
            );
        }
    }

    public bool IsTargetValidForAbility(
        Ability ability,
        GameObject target)
    {
        if (ability == null || target == null)
            return false;

        switch (ability.target)
        {
            case AbilityTarget.Self:
                return target == gameObject;

            case AbilityTarget.Enemy:
            {
                Enemy enemy = target.GetComponent<Enemy>();

                return enemy != null &&
                       !enemy.IsDead &&
                       targetingService != null &&
                       targetingService.CanTarget(
                           gameObject,
                           target,
                           ability.attackType);
            }

            case AbilityTarget.Corpse:
            {
                Enemy enemy = target.GetComponent<Enemy>();

                return enemy != null &&
                       enemy.IsDead &&
                       battlefield != null &&
                       battlefield.TryFindRank(
                           target,
                           out BattleRank rank) &&
                       rank.Side == BattleSide.Enemies;
            }

            default:
                return false;
        }
    }
}