using UnityEngine;

public static class CombatCalculator
{
    public static bool RollCriticalHit(CharacterAttributes attacker)
    {
        if (attacker == null)
            return false;

        float criticalChance = Mathf.Clamp(
            attacker.Luck,
            0f,
            100f
        );

        return Random.Range(0f, 100f) < criticalChance;
    }

    public static int CalculateDamage(
        int baseDamage,
        bool isCritical,
        float criticalMultiplier = 2f)
    {
        if (baseDamage <= 0)
            return 0;

        float damage = isCritical
            ? baseDamage * criticalMultiplier
            : baseDamage;

        return Mathf.RoundToInt(damage);
    }
}