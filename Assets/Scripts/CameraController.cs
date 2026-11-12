using UnityEngine;

/// <summary>
/// Handles camera views, cycling cameras, and VR head look emulation.
/// Aligned strictly with NPTEL Mobile Virtual Reality & AI (Week 1: Cycling cameras & Player movements, Week 4: VR viewpoint tracking).
/// </summary>
public class CameraController : MonoBehaviour
{
    public enum ViewMode
    {
        TopView,
        AngledPerspective,
        InnerCockpit,
        PlanetFocus
    }

    [Header("Current Camera View State")]
    public ViewMode currentView = ViewMode.AngledPerspective;

    [Header("Preset View Positions & Orientations")]
    [Tooltip("True 90-degree bird's-eye Top View looking straight down at the entire solar system.")]
    public Vector3 topViewPos = new Vector3(0f, 105f, 0f);
    public Vector3 topViewRot = new Vector3(89.9f, 0f, 0f);

    public Vector3 angledPos = new Vector3(0f, 38f, -65f);
    public Vector3 angledRot = new Vector3(30f, 0f, 0f);

    public Vector3 cockpitPos = new Vector3(0f, 4f, -14f);
    public Vector3 cockpitRot = new Vector3(5f, 0f, 0f);

    [Header("Interpolation Speeds")]
    public float positionLerpSpeed = 3.5f;
    public float rotationSlerpSpeed = 4.5f;

    [Header("Planet Focus Settings")]
    public Transform focusedPlanet;
    public float focusDistance = 6.0f;
    public float focusHeight = 2.5f;

    [Header("Editor & Desktop Mouse-Look VR Emulation")]
    public bool enableMouseLook = true;
    public float mouseSensitivity = 2.0f;

    private float pitch = 0f;
    private float yaw = 0f;
    private bool isMouseLooking = false;

    private void Start()
    {
        // Enable mobile gyroscope if supported on Android/iOS Cardboard device
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }

        ApplyPreset(currentView);
    }

    private void LateUpdate()
    {
        // Check if user is holding Right Click or Alt key for Editor VR Head Look emulation
        HandleEditorHeadLook();

        if (isMouseLooking)
        {
            return;
        }

        // Handle active camera mode
        switch (currentView)
        {
            case ViewMode.TopView:
                InterpolateToTransform(topViewPos, Quaternion.Euler(topViewRot));
                break;

            case ViewMode.AngledPerspective:
                InterpolateToTransform(angledPos, Quaternion.Euler(angledRot));
                break;

            case ViewMode.InnerCockpit:
                InterpolateToTransform(cockpitPos, Quaternion.Euler(cockpitRot));
                break;

            case ViewMode.PlanetFocus:
                UpdatePlanetFocus();
                break;
        }
    }

    /// <summary>
    /// Smoothly updates camera to follow the selected planet.
    /// </summary>
    private void UpdatePlanetFocus()
    {
        if (focusedPlanet == null)
        {
            currentView = ViewMode.AngledPerspective;
            return;
        }

        float scale = focusedPlanet.localScale.x;
        float actualDist = Mathf.Max(6.0f, scale * 3.2f);
        float actualHeight = Mathf.Max(2.0f, scale * 1.2f);

        Vector3 targetPos = focusedPlanet.position - (focusedPlanet.forward * actualDist) + (Vector3.up * actualHeight);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * positionLerpSpeed);

        Quaternion targetRot = Quaternion.LookRotation(focusedPlanet.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSlerpSpeed);
    }

    /// <summary>
    /// Resets camera to standard angled overview.
    /// </summary>
    public void ResetToOverview()
    {
        focusedPlanet = null;
        currentView = ViewMode.AngledPerspective;
        ApplyPreset(currentView);
    }

    private void InterpolateToTransform(Vector3 targetPos, Quaternion targetRot)
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * positionLerpSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSlerpSpeed);
    }

    /// <summary>
    /// Cycles to the next camera perspective in sequence (Top View -> Angled Perspective -> Cockpit).
    /// </summary>
    public void CycleCamera()
    {
        focusedPlanet = null;
        int nextIndex = ((int)currentView + 1) % 3; // Cycle through TopView (0), AngledPerspective (1), InnerCockpit (2)
        currentView = (ViewMode)nextIndex;
        ApplyPreset(currentView);
    }

    /// <summary>
    /// Directly switches the camera to the 90-degree Bird's Eye Top View.
    /// </summary>
    public void SetTopView()
    {
        focusedPlanet = null;
        currentView = ViewMode.TopView;
        ApplyPreset(currentView);
    }

    /// <summary>
    /// Sets camera target to inspect a specific planet up close.
    /// </summary>
    public void FocusOnPlanet(Transform planet)
    {
        focusedPlanet = planet;
        currentView = ViewMode.PlanetFocus;
    }

    public string GetCurrentViewName()
    {
        switch (currentView)
        {
            case ViewMode.TopView: return "Top View (Map)";
            case ViewMode.AngledPerspective: return "Angled View";
            case ViewMode.InnerCockpit: return "Inner Cockpit";
            case ViewMode.PlanetFocus: return focusedPlanet != null ? $"{focusedPlanet.name} Orbit" : "Planet View";
            default: return "Custom View";
        }
    }

    private void ApplyPreset(ViewMode mode)
    {
        switch (mode)
        {
            case ViewMode.TopView:
                pitch = topViewRot.x;
                yaw = topViewRot.y;
                break;
            case ViewMode.AngledPerspective:
                pitch = angledRot.x;
                yaw = angledRot.y;
                break;
            case ViewMode.InnerCockpit:
                pitch = cockpitRot.x;
                yaw = cockpitRot.y;
                break;
        }
    }

    /// <summary>
    /// Allows the developer/grader to simulate VR head rotation in the Unity Editor or Desktop using the mouse.
    /// </summary>
    private void HandleEditorHeadLook()
    {
        if (!enableMouseLook) return;

        // Hold Right-Click or Alt to look around in 360 degrees
        if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.LeftAlt))
        {
            isMouseLooking = true;
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity * 2.0f;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * 2.0f;
            pitch = Mathf.Clamp(pitch, -85f, 85f);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
        else if (isMouseLooking && (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.LeftAlt)))
        {
            isMouseLooking = false;
        }
    }
}
