
using UnityEngine;

public class AbilityPanel : MonoBehaviour
{
    [SerializeField] private AbilityButton abilityButtonPrefab;

    private PlayerAbilities playerAbilities;

    private void Start()
    {
        playerAbilities = FindFirstObjectByType<PlayerAbilities>();

        if (playerAbilities == null)
            return;

        playerAbilities.ActiveAbilitiesChanged += RebuildButtons;
        RebuildButtons();
    }

    private void OnDestroy()
    {
        if (playerAbilities != null)
            playerAbilities.ActiveAbilitiesChanged -= RebuildButtons;
    }

    private void RebuildButtons()
    {
        if (playerAbilities == null || abilityButtonPrefab == null)
            return;

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        Ability[] abilities =
            playerAbilities.GetSelectedAbilities(AbilityActivationType.Active);

        AbilityButton[] newButtons = new AbilityButton[abilities.Length];

        for (int i = 0; i < abilities.Length; i++)
        {
            AbilityButton button = Instantiate(abilityButtonPrefab, transform);
            button.Initialize(abilities[i], playerAbilities.gameObject);
            newButtons[i] = button;
        }

        playerAbilities.RegisterButtons(newButtons);
    }
}
