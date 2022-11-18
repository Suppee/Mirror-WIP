using UnityEngine;
using TMPro;

public class PlayerScore : MonoBehaviour
{
    public TextMeshProUGUI playerScore;

    public void DisplayPlayerScore(int newPlayerScore)
    {
        playerScore.text = "Score: " + newPlayerScore.ToString();
    }
}
