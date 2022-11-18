using TMPro;
using Mirror;

public class GlobalScore : NetworkBehaviour
{
    public TextMeshProUGUI globalScore;

    [SyncVar(hook = nameof(DisplayGlobalScore))]
    public int globalScoreValue = 0;

    public void DisplayGlobalScore(int oldScore, int newScore)
    {
        globalScore.text = "Global: " + newScore;
    }

    // Update is called once per frame
    void Start()
    {
        globalScore.text = "Global: 0";
    }
}
