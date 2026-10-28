using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Statuses/Shield")]
public class ShieldStatusData : StatusEffectData
{
    [SerializeField, Range(0f, 1f)]
    private float damageReduction = 0.5f;

    public override int ModifyIncomingDamage(int damage)
    {
        return Mathf.CeilToInt(damage * (1f - damageReduction));
    }

    public override bool ConsumeOnDamage => true;
}
