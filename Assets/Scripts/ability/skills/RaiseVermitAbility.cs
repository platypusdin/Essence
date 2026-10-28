using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Abilities/Raise Vermit")]
public class RaiseVermitAbility : Ability
{
    [Header("Summon")]
    [SerializeField] private SummonedAlly allyPrefab;
    [SerializeField] private AllyData allyData;

    public SummonedAlly AllyPrefab => allyPrefab;
    public AllyData AllyData => allyData;

    private void OnValidate()
    {
        target = AbilityTarget.Corpse;
    }

    public override AbilityExecutionResult Execute(
        GameObject caster,
        GameObject targetObject)
    {
        if (caster == null || targetObject == null)
            return AbilityExecutionResult.Failed;

        Enemy corpse = targetObject.GetComponent<Enemy>();

        if (corpse == null || !corpse.IsDead)
            return AbilityExecutionResult.Failed;

        AllyPlacementController placement =
            FindFirstObjectByType<AllyPlacementController>();

        if (placement == null)
            return AbilityExecutionResult.Failed;

        if (allyPrefab == null || allyData == null)
            return AbilityExecutionResult.Failed;

        if (!placement.BeginPlacement(
                caster, corpse, this, allyPrefab, allyData))
        {
            return AbilityExecutionResult.Failed;
        }

        return AbilityExecutionResult.Pending;
    }
}
