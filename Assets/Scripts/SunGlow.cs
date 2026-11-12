using UnityEngine;

/// <summary>
/// Controls the Sun's axial rotation and subtle solar flare/pulsing illumination.
/// Demonstrates Lights, Materials, and Update loops from Week 1.
/// </summary>
public class SunGlow : MonoBehaviour
{
    [Tooltip("Axial rotation speed of the Sun.")]
    public float rotationSpeed = 4.0f;

    [Tooltip("Point light located at the core of the Sun.")]
    public Light sunLight;

    [Tooltip("Base light intensity.")]
    public float baseIntensity = 2.5f;

    [Tooltip("Subtle pulse magnitude.")]
    public float pulseMagnitude = 0.2f;

    [Tooltip("Pulse speed.")]
    public float pulseFrequency = 1.5f;

    private void Update()
    {
        // 1. Slow, majestic rotation on the solar axis
        transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime), Space.World);

        // 2. Subtle solar luminous intensity breathing effect
        if (sunLight != null)
        {
            sunLight.intensity = baseIntensity + Mathf.Sin(Time.time * pulseFrequency) * pulseMagnitude;
        }
    }
}
