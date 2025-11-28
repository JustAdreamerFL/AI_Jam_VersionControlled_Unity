using Unity.VisualScripting;
using UnityEngine;

public static class ExtensionMethod {

    /// <summary>
    /// Sets a sensor's value, optionally normalizing it to the range [-1, 1].
    /// </summary>
    /// <param name="sensor">The sensor to which we want to send our value.</param>
    /// <param name="value">The raw, unprocessed sensor value.</param>
    /// <returns>The final value assigned to the sensor (normalized if enabled).</returns>
    public static float SetSensorValue(this SensorComponent sensor, float value) {
        if (sensor != null) {
            if (sensor.normalized == true) {
                value = Mathf.Clamp(value, sensor.minSensorValue, sensor.maxSensorValue);
                value = Mathf.InverseLerp(sensor.minSensorValue, sensor.maxSensorValue, value) * 2f - 1f;
            }
            sensor.SensorValue = value;
        }
        return value;
    }

    public static void SetRangeValues(this SensorComponent sensor, float minValue, float maxValue) {
        if (sensor != null) {
            sensor.normalized = true;
            sensor.minSensorValue = minValue;
            sensor.maxSensorValue = maxValue;
        }
    }
}
