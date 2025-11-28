using UnityEngine;

public class ProximitySensorRay : MonoBehaviour
{
    public enum RaySensorType {
        Long,
        Wide
    }

    [SerializeField] private RaySensorType raySensorType = RaySensorType.Long;

    private SensorComponent sensorProximity = null;
    private SensorComponent sensorRayinfo = null;

    private float rayAngleLong = 45f / 6f;
    private float rayDistanceLong = 2.5f;
    private float rayAngleWide = 140f / 6f;
    private float rayDistanceWide = 0.7f;

    private void Start() {
        var sensors = GetComponentsInChildren<SensorComponent>();
        foreach (var sensor in sensors) {
            if (sensor.GetSensorType == SensorComponent.SensorType.Proximity) {
                sensorProximity = sensor;
            }
            if (sensor.GetSensorType == SensorComponent.SensorType.Rayinfo) {
                sensorRayinfo = sensor;
            }
        }
    }

    private void FixedUpdate() {

        var rayDistance = rayDistanceWide;
        var rayAngle = rayAngleWide;
        var rayDirection = transform.TransformDirection(Vector3.forward);

        if (raySensorType == RaySensorType.Long) {
            rayDistance = rayDistanceLong;
            rayAngle = rayAngleLong;
        }

        var sensorValue = rayDistance;
        int objectHitType = 0;
        for (int i = -3; i <= 3; i++) {
            var raycastDirection = Quaternion.AngleAxis(rayAngle * i, Vector3.up) * rayDirection;
            if (Physics.Raycast(transform.position, raycastDirection, out RaycastHit hit, rayDistance)) {

                // for now we wont ignore the arm hit, maybe in the future
                //if (hit.transform.parent == this.transform.parent) {
                //    //continue;
                //}

                //Debug.DrawRay(transform.position, raycastDirection * hit.distance, Color.red);
                if (sensorValue > hit.distance) {
                    sensorValue = hit.distance;
                    switch (hit.transform.gameObject.tag) {
                        case "Arena":
                            objectHitType = 1;
                            break;
                        case "Robot":
                            objectHitType = 2;
                            break;
                        case "Bonus":
                            objectHitType = 3;
                            break;
                    }
                }

                //return;
            }
            else {
                //Debug.DrawRay(transform.position, raycastDirection * rayDistance, Color.gray);
            }
        }
        //Debug.Log("Closest hit distance: " + sensorValue / rayDistance);
        //Debug.Log("Closest object hit type: " + objectHitType);
        sensorProximity.SetSensorValue(sensorValue / rayDistance);
        sensorRayinfo.SetSensorValue(objectHitType);
    }

    private void OnDrawGizmos() {
        var rayDistance = rayDistanceWide;
        var rayAngle = rayAngleWide;
        var rayDirection = transform.TransformDirection(Vector3.forward);

        if (raySensorType == RaySensorType.Long) {
            rayDistance = rayDistanceLong;
            rayAngle = rayAngleLong;
        }

        for (int i = -3; i <= 3; i++) {
            var raycastDirection = Quaternion.AngleAxis(rayAngle * i, Vector3.up) * rayDirection;
            if (Physics.Raycast(transform.position, raycastDirection, out RaycastHit hit, rayDistance)) {
                Debug.DrawRay(transform.position, raycastDirection * hit.distance, Color.red);
            }
            else {
                Debug.DrawRay(transform.position, raycastDirection * rayDistance, Color.gray);
            }
        }
    }
}
