using System.Collections.Generic;

public interface IAbilityProvider
{
    IReadOnlyList<Ability> Abilities { get; }
}