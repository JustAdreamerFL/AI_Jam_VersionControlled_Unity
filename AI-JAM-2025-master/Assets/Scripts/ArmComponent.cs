using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Tento skript obsahuje všetku logiku spojenú so správaním ramena.
/// Stará sa o výpočet odmien, a hodnôt pre senzory.
/// </summary>
public class ArmComponent : MonoBehaviour, IHealth
{

    public float ArmRaisingReward { get; private set; }
    public float ArmDropReward { 
        get {
            var temp = armDropReward;
            armDropReward = 0f;
            return temp; 
        }
    }
    /// <summary>
    /// Uhol ramena v rozsahu <-1f, 1f>
    /// </summary>
    public float ArmAngle = 0f;

    [SerializeField] private float armDropAngle = 0f;
    private float armDropReward = 0f;
    private float prevArmAngle = 0f;

    private SensorComponent sensorAngle = null;
    private SensorComponent sensorDamage = null;

    private CollisionZoneBehaviour[] zoneComponents = null;

    float IHealth.CurrentHealth => sensorDamage?.SensorValue ?? 1f;
    float IHealth.influenceOnTotalHealth => 0.1f;

    private void Start() {
        var sensors = GetComponentsInChildren<SensorComponent>();

        foreach (var sensor in sensors) {
            if (sensor.GetSensorType == SensorComponent.SensorType.Angle) {
                sensorAngle = sensor;
                sensorAngle.SetRangeValues(-85f, 85f);
            }
            if (sensor.GetSensorType == SensorComponent.SensorType.Damage) {
                sensorDamage = sensor;
                sensorDamage.SetSensorValue(1f);

                zoneComponents = GetComponentsInChildren<CollisionZoneBehaviour>();
                foreach (var zone in zoneComponents) {
                    zone.SetZoneType("arm");
                }

                //Debug.Log($"Arm found {zoneComponents.Length} collision zones.");

                foreach (var zone in zoneComponents) {
                    zone.OnCollision += ArmCollisionDetection;
                }
            }
        }
    }

    private void ArmCollisionDetection(object sender, CollisionZoneBehaviour.CollisionEventArgs e) {
        // TODO: Vyratat reward pri kolizii ramena do niecoho / robota
        //float armDropAngleDifference = armDropAngle - ArmAngle;
        //if (armDropAngleDifference > 5f) {
        armDropReward = Mathf.Clamp(armDropAngle / 90f, 0f, 1f);
        //}
        //Debug.Log($"Arm collision detected! Arm drop angle: {armDropAngle}");
    }

    private void FixedUpdate() {
        float minCurrentHealth = zoneComponents?.Min(e=> e.CurrentHealth) ?? 1f;
        ArmAngle = Mathf.Round(90f-transform.localEulerAngles.x);

        // Is arm raising?
        if (prevArmAngle < ArmAngle) {
            armDropAngle = ArmAngle;
            ArmRaisingReward = 1f;
        }
        else
            ArmRaisingReward = 0f;


        prevArmAngle = ArmAngle;

        sensorAngle.SetSensorValue(ArmAngle);
        sensorDamage.SetSensorValue(minCurrentHealth); // Placeholder for damage value
    }
}
