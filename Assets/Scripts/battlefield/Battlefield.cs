using UnityEngine;

public class Battlefield : MonoBehaviour
{
    private const int RankCount = 4;

    private Transform[] heroPositions;
    private Transform[] enemyPositions;

    private GameObject[] heroOccupants = new GameObject[RankCount];
    private GameObject[] enemyOccupants = new GameObject[RankCount];

    private bool initialized;

    public bool Initialize(
        Transform[] newHeroPositions,
        Transform[] newEnemyPositions)
    {
        if (initialized)
            return true;

        if (!AreValidPositions(newHeroPositions) ||
            !AreValidPositions(newEnemyPositions))
        {
            Debug.LogError(
                "Battlefield: Exactly four valid positions per side are required.",
                this
            );
            return false;
        }

        heroPositions = newHeroPositions;
        enemyPositions = newEnemyPositions;

        initialized = true;
        return true;
    }

    public bool IsValidRank(BattleRank rank)
    {
        return initialized && rank.IsValid;
    }

    public bool IsOccupied(BattleRank rank)
    {
        if (!IsValidRank(rank))
            return false;

        return GetOccupants(rank.Side)[rank.Index - 1] != null;
    }

    public GameObject GetOccupant(BattleRank rank)
    {
        if (!IsValidRank(rank))
            return null;

        return GetOccupants(rank.Side)[rank.Index - 1];
    }

    public Transform GetAnchor(BattleRank rank)
    {
        if (!IsValidRank(rank))
            return null;

        return GetPositions(rank.Side)[rank.Index - 1];
    }

    public bool TryRegister(GameObject participant, BattleRank rank)
    {
        if (participant == null ||
            !IsValidRank(rank) ||
            IsOccupied(rank) ||
            TryFindRank(participant, out _))
        {
            return false;
        }

        GetOccupants(rank.Side)[rank.Index - 1] = participant;
        return true;
    }

    public bool TryMove(GameObject participant, BattleRank destination)
    {
        if (participant == null ||
            !IsValidRank(destination) ||
            IsOccupied(destination))
        {
            return false;
        }

        if (!TryFindRank(participant, out BattleRank current))
            return false;

        if (current.Side != destination.Side)
            return false;

        GetOccupants(current.Side)[current.Index - 1] = null;
        GetOccupants(destination.Side)[destination.Index - 1] = participant;

        Transform anchor = GetAnchor(destination);

        if (anchor != null)
            participant.transform.position = anchor.position;

        return true;
    }

    public bool TrySwap(GameObject first, GameObject second)
    {
        if (first == null || second == null || first == second)
            return false;

        if (!TryFindRank(first, out BattleRank firstRank) ||
            !TryFindRank(second, out BattleRank secondRank))
        {
            return false;
        }

        if (firstRank.Side != secondRank.Side)
            return false;

        GetOccupants(firstRank.Side)[firstRank.Index - 1] = second;
        GetOccupants(secondRank.Side)[secondRank.Index - 1] = first;

        Transform firstAnchor = GetAnchor(firstRank);
        Transform secondAnchor = GetAnchor(secondRank);

        if (firstAnchor != null)
            second.transform.position = firstAnchor.position;

        if (secondAnchor != null)
            first.transform.position = secondAnchor.position;

        return true;
    }

    public bool TryUnregister(GameObject participant)
    {
        if (participant == null ||
            !TryFindRank(participant, out BattleRank rank))
        {
            return false;
        }

        GetOccupants(rank.Side)[rank.Index - 1] = null;
        return true;
    }

    public bool TryFindRank(GameObject participant, out BattleRank rank)
    {
        rank = default;

        if (participant == null || !initialized)
            return false;

        foreach (BattleSide side in new[] { BattleSide.Heroes, BattleSide.Enemies })
        {
            GameObject[] occupants = GetOccupants(side);

            for (int i = 0; i < occupants.Length; i++)
            {
                if (occupants[i] != participant)
                    continue;

                rank = new BattleRank(side, i + 1);
                return true;
            }
        }

        return false;
    }

    private GameObject[] GetOccupants(BattleSide side)
    {
        return side == BattleSide.Heroes
            ? heroOccupants
            : enemyOccupants;
    }

    private Transform[] GetPositions(BattleSide side)
    {
        return side == BattleSide.Heroes
            ? heroPositions
            : enemyPositions;
    }

    private bool AreValidPositions(Transform[] positions)
    {
        if (positions == null || positions.Length != RankCount)
            return false;

        foreach (Transform position in positions)
        {
            if (position == null)
                return false;
        }

        return true;
    }
}