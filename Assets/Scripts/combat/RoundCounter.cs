using UnityEngine;
using TMPro;

public class RoundCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text roundText;

    public void SetRound(int round)
    {
        roundText.text = round.ToString();
    }
}
