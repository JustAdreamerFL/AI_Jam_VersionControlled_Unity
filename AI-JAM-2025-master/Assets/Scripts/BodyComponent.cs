using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BodyComponent : MonoBehaviour, IHealth
{
    private SensorComponent sensorVelocityForwards = null;
    private SensorComponent sensorVelocitySideways = null;
    private SensorComponent sensorDamage = null;
    private SensorComponent sensorEnemyAzimuth = null;

    private Rigidbody rb;

    private RobotAgent thisRobot;

    private CollisionZoneBehaviour[] zoneComponents = null;

    float IHealth.CurrentHealth => sensorDamage?.SensorValue ?? 1f;
    float IHealth.influenceOnTotalHealth => 1.0f;

    private void Start() {
        rb = GetComponentInParent<Rigidbody>();

        thisRobot = GetComponentInParent<RobotAgent>();

        var sensors = GetComponentsInChildren<SensorComponent>();

        foreach (var sensor in sensors) {
            // TODO: znormalizovatť
            if (sensor.GetSensorType == SensorComponent.SensorType.VelocityForwards) {
                sensorVelocityForwards = sensor;
            }
            if (sensor.GetSensorType == SensorComponent.SensorType.VelocitySideways) {
                sensorVelocitySideways = sensor;
            }
            if (sensor.GetSensorType == SensorComponent.SensorType.Damage) {
                sensorDamage = sensor;
                sensorDamage.SetSensorValue(1f);

                zoneComponents = GetComponentsInChildren<CollisionZoneBehaviour>();
                foreach (var zone in zoneComponents) {
                    zone.SetZoneType("body");
                }
            }
            if (sensor.GetSensorType == SensorComponent.SensorType.AzimuthToTarget) {
                sensorEnemyAzimuth = sensor;
                sensorEnemyAzimuth.SetRangeValues(-180f, 180f);
            }
        }

    }

    private void Update() {
        SetVelocitySensorValues();
        SetAzimuthSensorValues();

        float minCurrentHealth = zoneComponents?.Min(e => e.CurrentHealth) ?? 1f;
        sensorDamage.SetSensorValue(minCurrentHealth); // Placeholder for damage value
        
    }

    private void SetVelocitySensorValues() {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        sensorVelocityForwards.SetSensorValue(localVelocity.z);
        sensorVelocitySideways.SetSensorValue(localVelocity.x);
    }

    private void SetAzimuthSensorValues() {
        
        if (thisRobot.enemyRobot != null) {
            var vectorToEnemy = thisRobot.enemyRobot.transform.position - thisRobot.transform.position;
            var angleToEnemy = Vector3.SignedAngle(thisRobot.transform.forward, vectorToEnemy, Vector3.up);

            //Debug.Log("Angle to Enemy: " + angleToEnemy);

            sensorEnemyAzimuth.SetSensorValue(angleToEnemy);
        }
 
    }
}
