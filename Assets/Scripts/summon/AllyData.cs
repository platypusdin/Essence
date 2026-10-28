using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Ally Data")]
public class AllyData : ScriptableObject
{
    [Header("Display")]
    public string allyName = "Vermit";
    public Sprite sprite;
    public Sprite icon;

    [Header("Stats")]
    [Min(1)] public int maxHp = 20;
    [Min(1)] public int speed = 1;
    [Min(0)] public int damage = 10;
}