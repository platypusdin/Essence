
using UnityEngine;
using UnityEngine.UI;

public class AbilityIconPanel : MonoBehaviour
{
    [SerializeField] private AbilityActivationType activationType;
    [SerializeField] private Image iconPrefab;

    private PlayerAbilities playerAbilities;

    private void Start()
    {
        playerAbilities = FindFirstObjectByType<PlayerAbilities>();

        if (playerAbilities == null)
            return;

        playerAbilities.SelectedAbilitiesChanged += RebuildIcons;
        RebuildIcons();
    }

    private void OnDestroy()
    {
        if (playerAbilities != null)
            playerAbilities.SelectedAbilitiesChanged -= RebuildIcons;
    }

    private void RebuildIcons()
    {
        if (playerAbilities == null || iconPrefab == null)
            return;

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        Ability[] abilities =
            playerAbilities.GetSelectedAbilities(activationType);

        foreach (Ability ability in abilities)
        {
            Image icon = Instantiate(iconPrefab, transform);
            icon.sprite = ability.icon;
            icon.raycastTarget = false;
        }
    }
}
