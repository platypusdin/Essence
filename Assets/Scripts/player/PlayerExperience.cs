using System;
using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    public int TotalExperience { get; private set; }

    public event Action<int> ExperienceChanged;

    public void AddExperience(int amount)
    {
        if (amount <= 0)
            return;

        TotalExperience += amount;
        ExperienceChanged?.Invoke(TotalExperience);
    }
}