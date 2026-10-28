using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Statuses/Barrier")]
public class BarrierStatusData : StatusEffectData
{
    [SerializeField, Min(1)] private int maxBarrier = 40;

    public int MaxBarrier => maxBarrier;
}