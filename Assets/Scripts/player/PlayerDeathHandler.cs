using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Death Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite corpseSprite;

    private PlayerStats playerStats;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (playerStats != null)
            playerStats.Died += HandleDeath;
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.Died -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (spriteRenderer != null && corpseSprite != null)
            spriteRenderer.sprite = corpseSprite;

        GameOverController gameOver =
            FindFirstObjectByType<GameOverController>();

        if (gameOver != null)
            gameOver.Show();
        else
            Debug.LogWarning(
                "PlayerDeathHandler: GameOverController was not found."
            );
    }
}