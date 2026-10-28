using System.Collections.Generic;
using UnityEngine;

public class InitiativePanel : MonoBehaviour
{
    [Header("Icon containers")]
    [SerializeField] private Transform upcomingContainer;
    [SerializeField] private Transform completedContainer;

    [Header("Icon prefab")]
    [SerializeField] private InitiativeIconView iconPrefab;

    [Header("Player")]
    [SerializeField] private Sprite playerIcon;

    public void Refresh(
        IReadOnlyList<CombatParticipant> upcoming,
        IReadOnlyList<CombatParticipant> completed)
    {
        ClearContainer(upcomingContainer);
        ClearContainer(completedContainer);

        foreach (CombatParticipant participant in upcoming)
        {
            if (IsDeadParticipant(participant))
                continue;

            CreateIcon(participant, upcomingContainer, false);
        }

        foreach (CombatParticipant participant in completed)
        {
            if (IsDeadParticipant(participant))
                continue;

            CreateIcon(participant, completedContainer, true);
        }
    }

    private void CreateIcon(
        CombatParticipant participant,
        Transform container,
        bool completed)
    {
        if (participant == null)
            return;

        InitiativeIconView view =
            Instantiate(iconPrefab, container);

        SummonedAlly ally = participant.GetComponent<SummonedAlly>();

        if (ally != null)
        {
            AllyData data = ally.Data;

            view.Setup(
                data != null ? data.icon : null,
                data != null ? data.allyName : ally.gameObject.name,
                completed
            );

            return;
        }

        Enemy enemy = participant.GetComponent<Enemy>();

        if (enemy != null)
        {
            EnemyData data = enemy.Data;

            view.Setup(
                data != null ? data.icon : null,
                data != null ? data.enemyName : enemy.gameObject.name,
                completed
            );

            return;
        }

        PlayerStats player =
            participant.GetComponent<PlayerStats>();

        if (player != null)
        {
            view.Setup(playerIcon, "Player", completed);
            return;
        }

        view.Setup(null, participant.gameObject.name, completed);
    }

    private bool IsDeadParticipant(CombatParticipant participant)
    {
        if (participant == null)
            return true;

        Enemy enemy = participant.GetComponent<Enemy>();

        if (enemy != null && enemy.IsDead)
            return true;

        SummonedAlly ally = participant.GetComponent<SummonedAlly>();

        return ally != null && ally.IsDead;
    }

    private void ClearContainer(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }
}