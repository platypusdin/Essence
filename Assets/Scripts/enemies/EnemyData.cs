using UnityEngine;


[CreateAssetMenu(menuName = "Combat/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Display")]
    public string enemyName;
    public Sprite sprite;
    public Sprite icon;
    public Sprite corpseSprite;

    [Header("Stats")]
    public int maxHp = 50;
    public int speed = 2;
    
    [Header("Enemy AI")]
    [Min(0)] public int tacticalApPerTurn = 2;

    [Tooltip(
        "Порядок задаёт приоритет активных способностей ИИ. " +
        "Первая подходящая способность выбирается первой. " +
        "Пассивные способности и реакции не участвуют в обычном выборе."
    )]
    public Ability[] abilities;
}
