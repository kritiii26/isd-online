using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all 2D and World-Space 3D VR user interface elements:
/// 1. Planet Information Card (displays name, type, distances, and facts)
/// 2. Orbital simulation controls (Play/Pause, speed adjustment)
/// 3. View switching & cycling
/// 4. Billboard orientation ensuring text panels face the VR camera
/// 
/// Aligned with NPTEL Mobile Virtual Reality & AI (Week 1: Menus, Prefabs, and UI Canvas).
/// </summary>
public class SolarSystemUIManager : MonoBehaviour
{
    [Header("Information Card Elements")]
    [Tooltip("Panel container for the planet's info card.")]
    public GameObject infoPanel;

    [Tooltip("Text component for planet title.")]
    public Text planetTitleText;

    [Tooltip("Text component for detailed scientific data.")]
    public Text planetDetailsText;

    [Header("Simulation Controls")]
    public Text pauseButtonText;
    public Text speedButtonText;
    public Text currentViewText;

    [Header("Camera & VR References")]
    public Camera vrCamera;
    public CameraController cameraController;
    public SolarSystemAudio audioFeedback;

    // Internal state
    private Transform targetPlanetTransform;
    private readonly float[] speedOptions = new float[] { 1.0f, 2.0f, 5.0f, 0.5f };
    private int currentSpeedIndex = 0;

    private void Start()
    {
        if (vrCamera == null)
        {
            vrCamera = Camera.main;
        }

        if (cameraController == null)
        {
            cameraController = Object.FindFirstObjectByType<CameraController>();
        }

        if (audioFeedback == null)
        {
            audioFeedback = Object.FindFirstObjectByType<SolarSystemAudio>();
        }

        UpdateSimulationUI();
    }

    private void LateUpdate()
    {
        // Keep the World Space UI billboarded towards the camera
        if (infoPanel != null && infoPanel.activeSelf && vrCamera != null)
        {
            // Billboard effect: UI panel faces VR camera orientation
            infoPanel.transform.rotation = vrCamera.transform.rotation;
        }

        // Support keyboard hotkeys for convenience during development/grading
        HandleKeyboardShortcuts();
    }

    /// <summary>
    /// Displays the detailed information card for a celestial body.
    /// </summary>
    public void DisplayPlanetInfo(PlanetData data, Transform planetTransform)
    {
        if (data == null) return;

        targetPlanetTransform = planetTransform;

        if (planetTitleText != null)
        {
            planetTitleText.text = data.planetName.ToUpper();
        }

        if (planetDetailsText != null)
        {
            planetDetailsText.text = data.GetFormattedDetails();
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }

        if (cameraController != null && planetTransform != null)
        {
            cameraController.FocusOnPlanet(planetTransform);
        }
    }

    /// <summary>
    /// Closes the currently active info card.
    /// </summary>
    public void CloseInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (audioFeedback != null)
        {
            audioFeedback.PlayClickTone();
        }
    }

    /// <summary>
    /// Toggles orbital revolution motion on and off.
    /// </summary>
    public void TogglePause()
    {
        PlanetController.isMotionPaused = !PlanetController.isMotionPaused;
        UpdateSimulationUI();

        if (audioFeedback != null)
        {
            audioFeedback.PlayClickTone();
        }
    }

    /// <summary>
    /// Cycles through simulation speed multipliers: 1x -> 2x -> 5x -> 0.5x.
    /// </summary>
    public void CycleSpeed()
    {
        currentSpeedIndex = (currentSpeedIndex + 1) % speedOptions.Length;
        PlanetController.globalSpeedMultiplier = speedOptions[currentSpeedIndex];
        UpdateSimulationUI();

        if (audioFeedback != null)
        {
            audioFeedback.PlayClickTone();
        }
    }

    /// <summary>
    /// Cycles camera viewpoints (Overview, Angled, Inner Orbit, Planet Focus).
    /// </summary>
    public void CycleCameraView()
    {
        if (cameraController != null)
        {
            cameraController.CycleCamera();
            if (currentViewText != null)
            {
                currentViewText.text = $"View: {cameraController.GetCurrentViewName()}";
            }
        }

        if (audioFeedback != null)
        {
            audioFeedback.PlayClickTone();
        }
    }

    /// <summary>
    /// Directly switches the camera to the 90-degree Bird's Eye Top View.
    /// </summary>
    public void SetTopView()
    {
        if (cameraController != null)
        {
            cameraController.SetTopView();
            if (currentViewText != null)
            {
                currentViewText.text = $"View: {cameraController.GetCurrentViewName()}";
            }
        }

        if (audioFeedback != null)
        {
            audioFeedback.PlayClickTone();
        }
    }

    /// <summary>
    /// Updates the text on UI control buttons to reflect current simulation status.
    /// </summary>
    private void UpdateSimulationUI()
    {
        if (pauseButtonText != null)
        {
            pauseButtonText.text = PlanetController.isMotionPaused ? "Resume Orbits" : "Pause Orbits";
        }

        if (speedButtonText != null)
        {
            speedButtonText.text = $"Speed: {speedOptions[currentSpeedIndex]}x";
        }

        if (currentViewText != null && cameraController != null)
        {
            currentViewText.text = $"View: {cameraController.GetCurrentViewName()}";
        }
    }

    /// <summary>
    /// Convenience keyboard shortcuts for testing in editor / desktop.
    /// </summary>
    private void HandleKeyboardShortcuts()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            CycleCameraView();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            SetTopView();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            CycleSpeed();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInfoPanel();
        }
    }
}
