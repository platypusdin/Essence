using System.Collections.Generic;
using UnityEngine;

public class CombatParticipant : MonoBehaviour
{
    [SerializeField] private int speed = 1;

    private readonly Dictionary<object, int> speedModifiers = new();

    public int BaseSpeed => speed;

    public int Speed
    {
        get
        {
            int result = speed;

            foreach (int modifier in speedModifiers.Values)
                result += modifier;

            return Mathf.Max(1, result);
        }
    }

    public int Initiative { get; private set; }

    public void SetSpeed(int value)
    {
        speed = Mathf.Max(1, value);
    }

    public void SetSpeedModifier(object source, int modifier)
    {
        if (source == null)
            return;

        if (modifier == 0)
            speedModifiers.Remove(source);
        else
            speedModifiers[source] = modifier;
    }

    public void RemoveSpeedModifier(object source)
    {
        if (source != null)
            speedModifiers.Remove(source);
    }

    public void AddInitiative()
    {
        Initiative += Speed;
    }

    public void ResetInitiative(int value)
    {
        Initiative = value;
    }

    public bool IsAlive
    {
        get
        {
            Enemy enemy = GetComponent<Enemy>();

            if (enemy != null)
                return !enemy.IsDead;

            SummonedAlly ally = GetComponent<SummonedAlly>();

            if (ally != null)
                return !ally.IsDead;

            PlayerStats player = GetComponent<PlayerStats>();

            return player == null || player.Hp > 0;
        }
    }
}