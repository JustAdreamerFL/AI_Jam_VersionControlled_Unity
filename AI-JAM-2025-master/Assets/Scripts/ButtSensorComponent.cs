using System.Linq;
using UnityEngine;

public class ButtSensorComponent : MonoBehaviour {

    private SensorComponent buttSensor = null;
    private ButtMarker buttMarkerObject;
    private RobotAgent thisRobot;

    private void Start() {
        thisRobot = GetComponentInParent<RobotAgent>();

        var sensors = GetComponentsInChildren<SensorComponent>();

        foreach (var sensor in sensors) {
            // TODO: znormalizovat
            if (sensor.GetSensorType == SensorComponent.SensorType.ButtSensor) {
                buttSensor = sensor;

                if (thisRobot.enemyRobot == null) return;
                buttMarkerObject = thisRobot.enemyRobot.GetComponentInChildren<ButtMarker>();

            }
        }
    }

    private void Update() {
        if (buttSensor != null && buttMarkerObject != null) {
            if (Physics.Raycast(transform.position,
                (buttMarkerObject.transform.position - transform.position).normalized,
                out RaycastHit hitInfo)) {

                if (hitInfo.collider.tag == "ButtTag") {
                    buttSensor.SensorValue = 1f;
                    return;
                }
                //Debug.Log("Butt Receiver Hit:" + hitInfo.collider.name);

            }
            buttSensor.SensorValue = 0f;
        }
    }

    private void OnDrawGizmos() {
        if (buttSensor?.SensorValue == 1f) {
            Gizmos.color = Color.green;
        }
        else {
            Gizmos.color = Color.blue;
        }

        if (buttMarkerObject != null) {
            Gizmos.DrawLine(transform.position, buttMarkerObject.transform.position);
        }
    }
}
