using UnityEngine;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Button button;
    [SerializeField] private Image cooldownOverlay;

    private Ability ability;
    private PlayerAbilities playerAbilities;
    private bool targetAvailable;

    private void Awake()
    {
        if (cooldownOverlay != null)
        {
            cooldownOverlay.raycastTarget = false;
            cooldownOverlay.type = Image.Type.Filled;
            cooldownOverlay.fillMethod = Image.FillMethod.Vertical;
            cooldownOverlay.fillOrigin = (int)Image.OriginVertical.Bottom;
        }
    }

    public void Initialize(Ability newAbility, GameObject playerObject)
    {
        ability = newAbility;
        playerAbilities = playerObject.GetComponent<PlayerAbilities>();

        icon.sprite = ability.icon;
        button.interactable = false;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.sprite = ability.icon;
            cooldownOverlay.color = new Color(1f, 1f, 1f, 0.65f);
            cooldownOverlay.type = Image.Type.Filled;
            cooldownOverlay.fillMethod = Image.FillMethod.Vertical;
            cooldownOverlay.fillOrigin = (int)Image.OriginVertical.Bottom;
            cooldownOverlay.raycastTarget = false;
            cooldownOverlay.fillAmount = 0f;
            cooldownOverlay.gameObject.SetActive(false);
        }
    }

    public void SetAvailability(
        bool targetAvailable,
        int cooldownRemaining,
        int cooldownDuration)
    {
        this.targetAvailable = targetAvailable;

        bool onCooldown = cooldownRemaining > 0;

        button.interactable = targetAvailable && !onCooldown;

        if (cooldownOverlay == null)
            return;

        cooldownOverlay.gameObject.SetActive(onCooldown);

        cooldownOverlay.fillAmount =
            onCooldown && cooldownDuration > 0
                ? Mathf.Clamp01((float)cooldownRemaining / cooldownDuration)
                : 0f;
    }
    public void OnClick()
    {
        CombatTurnController turnController =
            FindFirstObjectByType<CombatTurnController>();

        if (turnController == null || !turnController.IsPlayerTurn())
            return;

        AllyPlacementController placement =
            FindFirstObjectByType<AllyPlacementController>();

        if (placement != null && placement.IsActive)
            return;

        if (ability == null || playerAbilities == null)
            return;

        if (!playerAbilities.CanUseAbility(ability))
            return;
            
        if (!targetAvailable)
            return;

        GameObject target = playerAbilities.GetTarget();

        if (target == null)
            return;

        if (!playerAbilities.IsTargetValidForAbility(ability, target))
            return;

        PlayerStats stats =
            playerAbilities.GetComponent<PlayerStats>();

        if (stats == null || stats.Ap < ability.apCost)
            return;

        AbilityExecutionResult result =
            ability.Execute(playerAbilities.gameObject, target);

        if (result == AbilityExecutionResult.Completed)
        {
            if (!stats.TrySpendAp(ability.apCost))
                return;

            playerAbilities.StartCooldown(ability);
            playerAbilities.ClearTarget();
        }
    }
}