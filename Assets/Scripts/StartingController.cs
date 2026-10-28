using UnityEngine;


// Deprecated
public class StartingController : MonoBehaviour
{
    [SerializeField] private MovementSlot slotPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int slotCount = 4;
     [SerializeField] private int spawnSlot = 2;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private GameObject heroPrefab;

    private MovementSlot[] slots;

    private void Awake()
    {
        CreateSlots();
        SpawnHero();
    }
    private void CreateSlots()
    {
        GameObject movementArea = new GameObject("MovementArea");

        slots = new MovementSlot[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            MovementSlot slot = Instantiate(
                slotPrefab,
                spawnPoint.position + Vector3.right * (i * spacing),
                Quaternion.identity,
                movementArea.transform
            );

            slot.Configure(i + 1);
            slots[i] = slot;
        }
    }

    private void SpawnHero()
    {
        MovementSlot startSlot = slots[spawnSlot];

        GameObject hero = Instantiate(
            heroPrefab,
            startSlot.Anchor.position,
            Quaternion.identity
        );

        PlayerMovement movement = hero.GetComponent<PlayerMovement>();
        movement.Initialize(slots, startSlot);
    }
}
