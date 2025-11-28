using UnityEngine;

public class SensorComponent : MonoBehaviour
{
    public enum SensorType {
        Angle,
        Damage,
        VelocityForwards,
        VelocitySideways,
        AzimuthToTarget,
        Proximity,
        ButtSensor,
        Rayinfo
    }

    [SerializeField] private SensorType sensorType;

    public SensorType GetSensorType => sensorType;

    public float SensorValue { get; set; }
    public bool normalized = false;
    public float minSensorValue = -1f;
    public float maxSensorValue = 1f;
}
