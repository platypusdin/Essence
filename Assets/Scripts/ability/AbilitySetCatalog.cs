using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "AbilitySetCatalog",
    menuName = "Combat/Abilities/Ability Set Catalog"
)]
public sealed class AbilitySetCatalog : ScriptableObject
{
    [SerializeField] private AbilitySet[] abilitySets;

    public AbilitySet[] AbilitySets =>
        abilitySets ?? Array.Empty<AbilitySet>();
}