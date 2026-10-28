using System;
using UnityEngine;

public static class CombatDamageService
{
    public sealed class DamageResult
    {
        public GameObject Caster { get; }
        public GameObject Target { get; }
        public CombatAttackType AttackType { get; }
        public DamageType DamageType { get; }
        public int FinalDamage { get; }
        public bool IsCritical { get; }
        public bool IsDodged { get; }
        public bool WasBlockedByCover { get; }

        public DamageResult(
            GameObject caster,
            GameObject target,
            CombatAttackType attackType,
            DamageType damageType,
            int finalDamage,
            bool isCritical,
            bool isDodged,
            bool wasBlockedByCover)
        {
            Caster = caster;
            Target = target;
            AttackType = attackType;
            DamageType = damageType;
            FinalDamage = finalDamage;
            IsCritical = isCritical;
            IsDodged = isDodged;
            WasBlockedByCover = wasBlockedByCover;
        }
    }

    // Единое событие с полной информацией о результате атаки.
    public static event Action<DamageResult> DamageResolved;

    private static CombatLog cachedCombatLog;
    private static Battlefield cachedBattlefield;

    public static bool TryApplyDamage(
        GameObject caster,
        GameObject targetObject,
        int baseDamage,
        string abilityName,
        out int finalDamage,
        out bool isCritical,
        CombatAttackType attackType = CombatAttackType.Other,
        DamageType damageType = DamageType.Physical)
    {
        finalDamage = 0;
        isCritical = false;

        if (caster == null ||
            targetObject == null ||
            baseDamage <= 0)
        {
            return false;
        }

        IDamageable target =
            targetObject.GetComponent<IDamageable>();

        if (target == null || target.IsDead)
            return false;

        CombatLog combatLog = GetCombatLog();

        bool isPhysical = damageType == DamageType.Physical;
        bool isDodged = false;
        bool wasBlockedByCover = false;

        CharacterAttributes targetAttributes =
            targetObject.GetComponent<CharacterAttributes>();

        // Уклонение доступно только против физического урона.
        if (isPhysical)
        {
            float dodgeChance = targetAttributes != null
                ? Mathf.Clamp(targetAttributes.Agility, 0f, 100f)
                : 0f;

            isDodged =
                UnityEngine.Random.Range(0f, 100f) < dodgeChance;
        }

        if (isDodged)
        {
            combatLog?.AddLog(
                $"{targetObject.name} dodges {abilityName}!");

            DamageResolved?.Invoke(new DamageResult(
                caster,
                targetObject,
                attackType,
                damageType,
                0,
                false,
                true,
                false));

            return true;
        }

        // Критические удары доступны только для физического урона.
        if (isPhysical)
        {
            CharacterAttributes attackerAttributes =
                caster.GetComponent<CharacterAttributes>();

            isCritical =
                CombatCalculator.RollCriticalHit(attackerAttributes);
        }

        int calculatedDamage =
            CombatCalculator.CalculateDamage(
                baseDamage,
                isCritical);

        // Укрытие действует на melee-атаки независимо от типа урона.
        if (attackType == CombatAttackType.Melee &&
            HasCover(caster, targetObject))
        {
            calculatedDamage = (calculatedDamage + 1) / 2;
            wasBlockedByCover = true;
        }

        finalDamage = target.TakeDamage(calculatedDamage);

        combatLog?.AddLog(
            $"{caster.name} uses {abilityName} on {targetObject.name}");

        string criticalMessage = isCritical
            ? " (critical hit!)"
            : "";

        string coverMessage = wasBlockedByCover
            ? " (damage reduced by cover)"
            : "";

        combatLog?.AddLog(
            $"{targetObject.name} takes {finalDamage} damage" +
            $"{criticalMessage}{coverMessage}");

        DamageResolved?.Invoke(new DamageResult(
            caster,
            targetObject,
            attackType,
            damageType,
            finalDamage,
            isCritical,
            false,
            wasBlockedByCover));

        return true;
    }

    private static CombatLog GetCombatLog()
    {
        if (cachedCombatLog == null)
        {
            cachedCombatLog =
                UnityEngine.Object.FindFirstObjectByType<CombatLog>();
        }

        return cachedCombatLog;
    }

    private static Battlefield GetBattlefield()
    {
        if (cachedBattlefield == null)
        {
            cachedBattlefield =
                UnityEngine.Object.FindFirstObjectByType<Battlefield>();
        }

        return cachedBattlefield;
    }

    private static bool HasCover(
        GameObject caster,
        GameObject target)
    {
        Battlefield battlefield = GetBattlefield();

        if (battlefield == null ||
            !battlefield.TryFindRank(caster, out BattleRank casterRank) ||
            !battlefield.TryFindRank(target, out BattleRank targetRank))
        {
            return false;
        }

        // Укрытие проверяется только между противниками.
        if (casterRank.Side == targetRank.Side)
            return false;

        // Ранг 1 предполагается передним.
        for (int rankIndex = 1; rankIndex < targetRank.Index; rankIndex++)
        {
            BattleRank intermediateRank =
                new BattleRank(targetRank.Side, rankIndex);

            if (battlefield.IsOccupied(intermediateRank))
                return true;
        }

        return false;
    }
}