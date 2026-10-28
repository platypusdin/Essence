using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Transform fill;
    [SerializeField, Min(0.01f)] private float fullWidth = 1f;

    private Vector3 initialScale;
    private float initialPositionX;
    private bool initialized;

    private void Awake()
    {
        CacheInitialState();
    }

    private void CacheInitialState()
    {
        if (initialized || fill == null)
            return;

        initialScale = fill.localScale;
        initialPositionX = fill.localPosition.x;
        initialized = true;
    }

    public void SetHealth(int currentHp, int maxHp)
    {
        CacheInitialState();

        if (!initialized || maxHp <= 0)
            return;

        float percent = Mathf.Clamp01((float)currentHp / maxHp);

        Vector3 scale = initialScale;
        scale.x = initialScale.x * percent;
        fill.localScale = scale;

        Vector3 position = fill.localPosition;
        position.x = initialPositionX
            - fullWidth * (1f - percent) / 2f;

        fill.localPosition = position;
    }
}