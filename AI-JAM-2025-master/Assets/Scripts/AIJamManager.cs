using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Manažuje logiku najvyššej úrovne pre AI Jam. Stará sa o spawnovanie robotov vo viacerých arénach
/// pre účely trénovania alebo turnaja, riadi, začína a končí zápasy.
/// </summary>
public class AIJamManager : MonoBehaviour
{
    // Prefaby robotov pre tréning alebo turnaj. Môžu to byť rovnaké alebo rôzne roboty.
    [SerializeField] RobotAgent robotPrefabA;
    [SerializeField] RobotAgent robotPrefabB;


    // Počet arén pre tréning. Je potrebné otestovať výkon pri rôznych počtoch.
    private enum numArenas {
        One = 1,
        Three = 3,
        Five = 5
    }
    [SerializeField] numArenas numOfArenas = numArenas.Three;


    // Pole všetkých trénovacích prostredí - určené počtom arén zvoleným v inšpektore.
    private GameObject[] environments;


    private void Start() {

        environments = GameObject.FindGameObjectsWithTag("Environment");

        if (robotPrefabA == null || robotPrefabB == null) {
            Debug.LogError("AIJamManager: One or both RobotAgent references are not set.");
            return;
        }

        foreach (var env in environments.OrderBy(e => e.name).Take((int)numOfArenas)) {
            var robotA = Instantiate(robotPrefabA, env.transform);
            var robotB = Instantiate(robotPrefabB, env.transform);
            robotA.gameObject.name = robotPrefabA.gameObject.name;
            robotB.gameObject.name = robotPrefabB.gameObject.name;

            robotA.OnRobotRespawn += (s, e) => MoveRobot(s as RobotAgent, 0);
            robotB.OnRobotRespawn += (s, e) => MoveRobot(s as RobotAgent, 1); 

            var statsGUI = env.GetComponentInChildren<StatisticsGUI>();
            statsGUI.SetRobots(robotA, robotB);

            var scoreGUI = env.GetComponentInChildren<ScoreGUI>();
            scoreGUI.SetRobots(robotA, robotB);

        }
    }

    private void MoveRobot(RobotAgent robot, int robotIdx) {
        // TODO: premiestnit respawn do RobotAgent skriptu a zavolat z OnEpisodeBegin

        float angle = 0f;
        if (robotIdx == 0) {
            angle = Random.Range(-25f, -155f);
        } else if (robotIdx == 1) {
            angle = Random.Range(25f, 155f);
        }

        robot.transform.localRotation = Quaternion.identity;
        robot.transform.localPosition = new Vector3(0, 0.022f, 0);
        robot.transform.localPosition += Quaternion.Euler(0, angle, 0) * Vector3.forward * Random.Range(0.8f, 1.7f);
        robot.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
    }
}
