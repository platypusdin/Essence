using UnityEngine;
using System;

[CreateAssetMenu(
    fileName = "NewAbilitySet",
    menuName = "Combat/Ability Set"
)]
public sealed class AbilitySet : ScriptableObject
{
    [Serializable]
    public sealed class BonusThreshold
    {
        [SerializeField, Min(1)]
        private int requiredAbilityCount = 2;

        [SerializeField]
        private string id;

        [SerializeField]
        private string displayName;

        [SerializeField, TextArea]
        private string description;

        public int RequiredAbilityCount => requiredAbilityCount;
        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
    }

    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Ability[] abilities;
    [SerializeField] private BonusThreshold[] bonusThresholds;

    public string Id => id;
    public string DisplayName => displayName;

    public Ability[] Abilities =>
        abilities ?? Array.Empty<Ability>();

    public BonusThreshold[] BonusThresholds =>
        bonusThresholds ?? Array.Empty<BonusThreshold>();
}
