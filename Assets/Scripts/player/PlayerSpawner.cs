using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject heroPrefab;
    
    [SerializeField, Min(1)] private int spawnRank = 2;

    public GameObject SpawnHero(
        MovementSlot[] slots,
        Battlefield battlefield)
    {
        if (heroPrefab == null ||
            slots == null ||
            slots.Length == 0 ||
            battlefield == null)
        {
            Debug.LogError(
                "PlayerSpawner: Hero prefab, slots or Battlefield is missing."
            );
            return null;
        }

        MovementSlot startSlot = slots[spawnRank - 1];

        if (startSlot == null)
        {
            Debug.LogError("PlayerSpawner: Start slot is missing.");
            return null;
        }

        GameObject hero = Instantiate(
            heroPrefab,
            startSlot.Anchor.position,
            Quaternion.identity
        );

        PlayerMovement movement = hero.GetComponent<PlayerMovement>();

        if (movement == null)
        {
            Debug.LogError(
                "PlayerSpawner: PlayerMovement is missing on the hero prefab."
            );

            Destroy(hero);
            return null;
        }

        movement.Initialize(slots, startSlot);
        movement.ConfigureBattlefield(battlefield);

        PlayerAbilities abilities = hero.GetComponent<PlayerAbilities>();
        abilities?.ConfigureBattlefield(battlefield);

        BattleRank rank = new BattleRank(
            BattleSide.Heroes,
            startSlot.Position
        );

        if (!battlefield.TryRegister(hero, rank))
        {
            Debug.LogError(
                $"PlayerSpawner: Could not register the hero at rank {startSlot.Position}."
            );

            Destroy(hero);
            return null;
        }

        return hero;
    }
}