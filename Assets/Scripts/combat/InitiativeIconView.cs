using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class InitiativeIconView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;

    [SerializeField, Range(0f, 1f)]
    private float completedBrightness = 0.55f;

    public void Setup(
        Sprite sprite,
        string displayName,
        bool completed = false)
    {
        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;

            icon.color = completed
                ? Color.gray * new Color(1f, 1f, 1f, 1f)
                : Color.white;

            if (completed)
                icon.color = new Color(
                    completedBrightness,
                    completedBrightness,
                    completedBrightness,
                    1f
                );
        }

        if (label != null)
        {
            label.text = displayName;
            label.color = completed
                ? Color.gray
                : Color.white;
        }
    }
}
