using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomCompletionController : MonoBehaviour
{
    [Header("Victory UI")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text experienceRewardText;
    [SerializeField] private TMP_Text abilityRewardText;

    [Header("Room")]
    [SerializeField] private RoomController roomController;

    private PlayerExperience playerExperience;
    private bool victoryShown;

    private void Awake()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (abilityRewardText != null)
            abilityRewardText.gameObject.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
    }

    private void FindPlayerExperience()
    {
        if (playerExperience != null)
            return;

        playerExperience = FindFirstObjectByType<PlayerExperience>();
    }

    public void ShowVictory(int experienceReward, Ability droppedAbility)
    {
        if (victoryShown)
            return;

        victoryShown = true;

        FindPlayerExperience();

        if (playerExperience != null)
        {
            playerExperience.AddExperience(experienceReward);
        }
        else
        {
            Debug.LogError(
                "RoomCompletionController: PlayerExperience was not found."
            );
        }

        if (experienceRewardText != null)
        {
            experienceRewardText.text =
                $"Получено опыта: {experienceReward}";
        }

        bool hasAbilityReward = droppedAbility != null;

        if (abilityRewardText != null)
        {
            abilityRewardText.gameObject.SetActive(hasAbilityReward);

            if (hasAbilityReward)
            {
                abilityRewardText.text =
                    $"Выпала новая способность: {droppedAbility.abilityName}";
            }
        }

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    public void HideVictory()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    private void OnContinueClicked()
    {
        if (roomController == null)
        {
            Debug.LogError(
                "RoomCompletionController: RoomController is missing."
            );
            return;
        }

        HideVictory();
        victoryShown = false;

        roomController.GenerateRoom();
    }
}