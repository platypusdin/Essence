using System;
using UnityEngine;

public class CharacterAttributes : MonoBehaviour
{
    [Header("Attributes (%)")]
    [SerializeField, Range(0f, 100f)]
    private float luck = 0f;

    [SerializeField, Range(0f, 100f)]
    private float agility = 0f;

    [SerializeField, Range(0f, 100f)]
    private float vitality = 0f;

    public float Luck => luck;
    public float Agility => agility;
    public float Vitality => vitality;

    public event Action AttributesChanged;

    public void SetLuck(float value)
    {
        luck = Mathf.Clamp(value, 0f, 100f);
        AttributesChanged?.Invoke();
    }

    public void SetAgility(float value)
    {
        agility = Mathf.Clamp(value, 0f, 100f);
        AttributesChanged?.Invoke();
    }

    public void SetVitality(float value)
    {
        vitality = Mathf.Clamp(value, 0f, 100f);
        AttributesChanged?.Invoke();
    }
}