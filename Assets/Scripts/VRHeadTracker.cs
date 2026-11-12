using UnityEngine;

/// <summary>
/// Provides Google Cardboard / Mobile VR Head Tracking using the mobile device gyroscope,
/// with mouse-look emulation inside the Unity Editor.
/// Aligned strictly with NPTEL Mobile Virtual Reality & AI (Week 4: Cardboard VR SDK mechanics).
/// </summary>
public class VRHeadTracker : MonoBehaviour
{
    [Header("Tracking Settings")]
    public bool enableGyroscope = true;
    public bool enableEditorSimulation = true;

    [Header("Editor Controls")]
    public float lookSensitivity = 2.5f;

    private bool gyroSupported = false;
    private Quaternion gyroInitialRotation;
    private float pitch = 0f;
    private float yaw = 0f;

    private void Start()
    {
        gyroSupported = SystemInfo.supportsGyroscope && enableGyroscope;
        if (gyroSupported)
        {
            Input.gyro.enabled = true;
            Input.gyro.updateInterval = 0.0167f; // ~60 Hz
        }
    }

    private void Update()
    {
        if (gyroSupported && Application.isMobilePlatform)
        {
            // Transform gyroscope quaternion into Unity camera coordinates
            transform.localRotation = ConvertGyroRotation(Input.gyro.attitude);
        }
        else if (enableEditorSimulation)
        {
            // Editor / Desktop mouse look simulation
            if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.LeftAlt))
            {
                yaw += Input.GetAxis("Mouse X") * lookSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
                pitch = Mathf.Clamp(pitch, -85f, 85f);

                transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
            }
        }
    }

    /// <summary>
    /// Converts right-handed mobile gyroscope orientation to left-handed Unity coordinate system.
    /// Standard transformation used across mobile Cardboard SDK implementations.
    /// </summary>
    private static Quaternion ConvertGyroRotation(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}
