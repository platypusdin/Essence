
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterStatusUI : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private TMP_Text healthText;

    [Header("Barrier")]
    [SerializeField] private GameObject shieldInfo;
    [SerializeField] private Transform barrierBar;
    [SerializeField] private TMP_Text barrierText;

    [Header("Effects")]
    [SerializeField] private Transform effectsContainer;
    [SerializeField] private SpriteRenderer effectIconPrefab;
    [SerializeField] private TMP_Text stackCountPrefab;


    [Header("Effects Layout")]
    [SerializeField] private float startOffsetX = -1f;
    [SerializeField] private float iconSpacing = 0.5f;
    [SerializeField] private float stackCountOffsetX = -0.2f;

    private StatusController statusController;

    private readonly List<GameObject> spawnedEffects = new();

    private void OnEnable()
    {
        statusController = GetComponentInParent<StatusController>();

        if (statusController != null)
            statusController.StatusesChanged += RefreshEffects;

        RefreshEffects();
    }

    private void OnDisable()
    {
        if (statusController != null)
            statusController.StatusesChanged -= RefreshEffects;

        ClearEffects();
    }

    public void UpdateHealth(int currentHp, int maxHp)
    {
        healthBar?.SetHealth(currentHp, maxHp);

        if (healthText != null)
            healthText.text = currentHp.ToString();
    }

    private void RefreshEffects()
    {
        RefreshBarrier();
        ClearEffects();

        if (statusController == null ||
            effectsContainer == null ||
            effectIconPrefab == null)
        {
            return;
        }

        IReadOnlyList<StatusInstance> statuses =
            statusController.ActiveStatuses;

        foreach (StatusInstance status in statuses)
        {
            if (status.Data == null || status.Data.Icon == null)
                continue;

            int index = spawnedEffects.Count;

            float iconX = startOffsetX + index * iconSpacing;

            CreateIcon(status.Data.Icon, iconX);

            if (status.RemainingStacks.HasValue &&
                stackCountPrefab != null)
            {
                CreateStackCount(
                    status.RemainingStacks.Value,
                    iconX + stackCountOffsetX
                );
            }
        }
    }

    private void RefreshBarrier()
    {
        if (shieldInfo == null)
            return;

        int currentBarrier = 0;

        if (statusController != null)
        {
            foreach (StatusInstance status in statusController.ActiveStatuses)
            {
                if (status.Data is BarrierStatusData)
                    currentBarrier += status.CurrentValue;
            }
        }

        bool hasBarrier = currentBarrier > 0;
        shieldInfo.SetActive(hasBarrier);

        if (hasBarrier && barrierText != null)
            barrierText.text = currentBarrier.ToString();
    }

    private void CreateIcon(Sprite sprite, float x)
    {
        SpriteRenderer icon = Instantiate(
            effectIconPrefab,
            effectsContainer
        );

        icon.sprite = sprite;
        icon.enabled = true;
        icon.gameObject.SetActive(true);

        icon.transform.localPosition = new Vector3(x, 0f, 0f);

        spawnedEffects.Add(icon.gameObject);
    }

    private void CreateStackCount(int stacks, float x)
    {
        TMP_Text count = Instantiate(
            stackCountPrefab,
            effectsContainer
        );

        count.text = stacks.ToString();
        count.gameObject.SetActive(true);

        count.transform.localPosition = new Vector3(x, 0f, -0.1f);

        spawnedEffects.Add(count.gameObject);
    }

    private void ClearEffects()
    {
        foreach (GameObject effect in spawnedEffects)
        {
            if (effect != null)
                Destroy(effect);
        }

        spawnedEffects.Clear();
    }
}
