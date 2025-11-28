using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsGUI : MonoBehaviour {

    private RobotAgent robotA;
    private RobotAgent robotB;

    private GameObject robotBoardA;
    private GameObject robotBoardB;

    private float timer = 0f;

    private void Start() {
        robotBoardA = transform.Find("RobotBoardA").gameObject;
        robotBoardB = transform.Find("RobotBoardB").gameObject;
    }

    internal void SetRobots(RobotAgent robotA, RobotAgent robotB) {
        this.robotA = robotA;
        this.robotB = robotB;
    }

    private void Update() {

        // call only every second
        timer = timer + Time.deltaTime;
        if (timer < 1f) {
            return;
        }
        timer = 0f;

        if (robotA != null) {
            UpdateRobotBoard(robotBoardA, robotA);
        }

        if (robotB != null) {
            UpdateRobotBoard(robotBoardB, robotB);
        }
    }

    private void UpdateRobotBoard(GameObject robotBoard, RobotAgent robot) {
        robotBoard.transform.Find("RobotBoardIn/RobotName").GetComponent<TextMeshProUGUI>().SetText(robot.gameObject.name);

        StatsBarGUI[] statBarArray = robotBoard.GetComponentsInChildren<StatsBarGUI>();
        WheelComponent[] wheelComponents = robot.GetComponentsInChildren<WheelComponent>()
            .OrderBy(e => e.transform.parent.gameObject.name).ToArray();

        foreach (StatsBarGUI statBar in statBarArray) {
            switch (statBar.Name) {
                case "StatItemBumper":
                    statBar.UpdateStatBar(robot.GetComponentInChildren<BumperComponent>());
                    break;
                case "StatItemBody":
                    statBar.UpdateStatBar(robot.GetComponentInChildren<BodyComponent>());
                    break;
                case "StatItemLWheels":
                    statBar.UpdateStatBar(wheelComponents[0]);
                    break;
                case "StatItemRWheels":
                    statBar.UpdateStatBar(wheelComponents[1]);
                    break;
                case "StatItemAverage":
                    statBar.UpdateStatBar(robot.weightedAverageHealth, 0.2f, 1f);
                    break;
                default:
                    Debug.LogWarning("Unknown stat name: " + statBar.Name);
                    break;
            }
        }

        // Debug.Log("Updating " + statBarArray.Count() + " bars for " + robot.name);
    }
}
