using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Room Data")]
public class RoomData : ScriptableObject
{
    [Header("Room")]
    [SerializeField] private string roomName;
    [SerializeField] private Sprite background;

    [Header("Encounter")]
    [SerializeField] private EncounterPool encounterPool;

    public string RoomName => roomName;
    public Sprite Background => background;
    public EncounterPool EncounterPool => encounterPool;
}