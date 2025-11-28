using TMPro;
using UnityEngine;

public class ScoreGUI : MonoBehaviour
{
    private RobotAgent robotA;
    private RobotAgent robotB;

    private int scoreA = 0;
    private int scoreB = 0;

    private GameObject scoreTextA;
    private GameObject scoreTextB;

    private AIJamManager aiJamManager;

    private void Start() {
        scoreTextA = transform.Find("ScoreIn/ScoreA/ScoreAtext").gameObject;
        scoreTextB = transform.Find("ScoreIn/ScoreB/ScoreBtext").gameObject;
    }

    internal void SetRobots(RobotAgent robotA, RobotAgent robotB) {
        this.robotA = robotA;
        this.robotB = robotB;

        robotA.OnRobotDie += (s, e) => {
            scoreB += 1;
            scoreTextB.GetComponent<TextMeshProUGUI>().SetText(scoreB.ToString());
        };
        robotB.OnRobotDie += (s, e) => {
            scoreA += 1;
            scoreTextA.GetComponent<TextMeshProUGUI>().SetText(scoreA.ToString());
        };
    }
}
