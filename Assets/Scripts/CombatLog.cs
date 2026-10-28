using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CombatLog : MonoBehaviour
{

     [SerializeField] private TMP_Text logText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;

    private PlayerStats playerStats;
    private int previousAp;

    private void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();

        if (playerStats == null)
            return;

        previousAp = playerStats.Ap;

        playerStats.ApChanged += OnApChanged;
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.ApChanged -= OnApChanged;
    }

    private void OnApChanged(int currentAp, int maxAp)
    {
        int spent = previousAp - currentAp;

        if (spent <= 0)
        {
            previousAp = currentAp;
            return;
        }

        AddLog($"Player spends {spent} AP");

        previousAp = currentAp;
    }

    public void AddLog(string message)
    {
        logText.text += message + "\n";

        Canvas.ForceUpdateCanvases();

        float height = logText.preferredHeight;

        content.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            height + 10f
        );

        scrollRect.verticalNormalizedPosition = 0f;
    }
}
