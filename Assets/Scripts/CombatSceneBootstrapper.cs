using UnityEngine;

public class CombatSceneBootstrapper : MonoBehaviour
{
    [SerializeField] private BattlefieldLayout battlefieldLayout;
    [SerializeField] private Battlefield battlefield;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private AbilitySetBonusController abilitySetBonusController;

    private void Awake()
    {
        if (battlefieldLayout == null ||
            battlefield == null ||
            playerSpawner == null ||
            abilitySetBonusController == null)
        {
            Debug.LogError(
                "CombatSceneBootstrapper: Required references are missing.",
                this
            );

            enabled = false;
            return;
        }

        battlefieldLayout.Initialize();

        battlefield.Initialize(
            battlefieldLayout.HeroPositions,
            battlefieldLayout.EnemyPositions
        );

        GameObject hero = playerSpawner.SpawnHero(
            battlefieldLayout.HeroSlots,
            battlefield
        );

        if (hero == null)
        {
            Debug.LogError(
                "CombatSceneBootstrapper: Hero spawning failed.",
                this
            );

            enabled = false;
            return;
        }

        PlayerAbilities playerAbilities =
            hero.GetComponent<PlayerAbilities>();

        if (playerAbilities == null)
        {
            Debug.LogError(
                "CombatSceneBootstrapper: PlayerAbilities is missing on the hero.",
                this
            );

            enabled = false;
            return;
        }

        abilitySetBonusController.RegisterPlayer(playerAbilities);
    }
}