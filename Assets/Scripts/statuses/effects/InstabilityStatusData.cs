using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Statuses/Instability")]
public class InstabilityStatusData : StatusEffectData
{
    [SerializeField, Min(1)] private int threshold = 100;

    public int Threshold => threshold;
}