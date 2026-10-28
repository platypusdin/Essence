using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class AbilitySelectionMenu : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private AbilitySelectionList availableList;
    [SerializeField] private AbilitySelectionList activeList;
    [SerializeField] private AbilitySelectionItem itemPrefab;
    [SerializeField] private Transform dragLayer;

    private PlayerAbilities playerAbilities;

    private void Start()
    {
        playerAbilities = FindFirstObjectByType<PlayerAbilities>();

        if (menuPanel != null)
            menuPanel.SetActive(false);

        if (playerAbilities == null ||
            menuPanel == null ||
            availableList == null ||
            activeList == null ||
            itemPrefab == null ||
            dragLayer == null)
        {
            Debug.LogError(
                "AbilitySelectionMenu: Assign all required references.",
                this
            );

            enabled = false;
            return;
        }

        availableList.Initialize(this, itemPrefab, dragLayer);
        activeList.Initialize(this, itemPrefab, dragLayer);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E))
            return;

        if (menuPanel.activeSelf)
            CloseMenu();
        else
            OpenMenu();
    }

    private void OpenMenu()
    {
        Ability[] active = new List<Ability>(playerAbilities.Abilities).ToArray();

        Ability[] available = playerAbilities.AvailableAbilities
            .Where(ability => ability != null && !active.Contains(ability))
            .ToArray();

        availableList.SetAbilities(available);
        activeList.SetAbilities(active);

        menuPanel.SetActive(true);
    }

    private void CloseMenu()
    {
        menuPanel.SetActive(false);
    }

    public void TryMoveAbility(
        AbilitySelectionItem item,
        AbilitySelectionList target)
    {
        if (item == null || target == null)
            return;

        AbilitySelectionList source = item.OwnerList;

        if (source == null || source == target)
            return;

        if (target.Contains(item.Ability))
            return;

        if (target.IsActiveList &&
            activeList.Abilities.Count >= PlayerAbilities.MaxSelectedAbilities)
        {
            Debug.Log("Maximum of 3 active abilities reached.");
            return;
        }

        source.TransferItemTo(item, target);
    }

    public void ConfirmSelection()
    {
        if (playerAbilities == null)
            return;

        if (playerAbilities.SetSelectedAbilities(activeList.Abilities.ToList()))
            CloseMenu();
    }

    public void CancelSelection()
    {
        CloseMenu();
    }
}