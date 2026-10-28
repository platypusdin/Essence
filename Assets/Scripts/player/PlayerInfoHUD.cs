using UnityEngine;
using TMPro;

public class PlayerInfoHUD : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField] private TMP_Text hpValue;
    [SerializeField] private TMP_Text apValue;
    [SerializeField] private TMP_Text xpValue;

    [Header("Attributes")]
    [SerializeField] private TMP_Text luckValue;
    [SerializeField] private TMP_Text agilityValue;
    [SerializeField] private TMP_Text vitalityValue;

    private PlayerStats playerStats;
    private PlayerExperience playerExperience;
    private CharacterAttributes characterAttributes;

    private void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        playerExperience = FindFirstObjectByType<PlayerExperience>();
        characterAttributes = FindFirstObjectByType<CharacterAttributes>();

        if (playerStats != null)
        {
            playerStats.HpChanged += UpdateHp;
            playerStats.ApChanged += UpdateAp;

            UpdateHp(playerStats.Hp, playerStats.MaxHp);
            UpdateAp(playerStats.Ap, playerStats.MaxAp);
        }

        if (playerExperience != null)
        {
            playerExperience.ExperienceChanged += UpdateXp;
            UpdateXp(playerExperience.TotalExperience);
        }

        if (characterAttributes != null)
        {
            characterAttributes.AttributesChanged += UpdateAttributes;
            UpdateAttributes();
        }
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.HpChanged -= UpdateHp;
            playerStats.ApChanged -= UpdateAp;
        }

        if (playerExperience != null)
            playerExperience.ExperienceChanged -= UpdateXp;

        if (characterAttributes != null)
            characterAttributes.AttributesChanged -= UpdateAttributes;
    }

    private void UpdateHp(int current, int max)
    {
        if (hpValue != null)
            hpValue.text = $"{current}/{max}";
    }

    private void UpdateAp(int current, int max)
    {
        if (apValue != null)
            apValue.text = $"{current}/{max}";
    }

    private void UpdateXp(int current)
    {
        if (xpValue != null)
            xpValue.text = current.ToString();
    }

    private void UpdateAttributes()
    {
        if (luckValue != null)
            luckValue.text = $"{characterAttributes.Luck:0.#}%";

        if (agilityValue != null)
            agilityValue.text = $"{characterAttributes.Agility:0.#}%";

        if (vitalityValue != null)
            vitalityValue.text = $"{characterAttributes.Vitality:0.#}%";
    }
}