using System.Collections.Generic;
using UnityEngine;

public interface IPassiveAbility
{
    void Recalculate(GameObject owner, PassiveAbilityContext context);
}

public sealed class PassiveAbilityContext
{
    private readonly PassiveAbilityController controller;

    public IReadOnlyList<CombatParticipant> Participants { get; }

    internal PassiveAbilityContext(
        PassiveAbilityController controller,
        IReadOnlyList<CombatParticipant> participants)
    {
        this.controller = controller;
        Participants = participants;
    }

    public void SetSpeedModifier(
        GameObject owner,
        Ability ability,
        CombatParticipant target,
        int modifier)
    {
        controller.SetSpeedModifier(owner, ability, target, modifier);
    }
}