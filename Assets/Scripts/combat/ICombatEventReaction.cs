using UnityEngine;

public interface ICombatEventReaction
{
    void OnCombatEvent(GameObject owner, CombatEvent combatEvent);

}