using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Implements Google Cardboard & Mobile VR Gaze-Based Interaction:
/// 1. Casts a forward ray from the VR Camera's viewpoint (Raycasting).
/// 2. Controls the VR Reticle pointer (dot/ring) at the center of the screen.
/// 3. Supports both 'Gaze Dwell' (holding stare for 1.2 seconds) and 'Cardboard Trigger' (screen tap / button click).
/// 
/// Strictly aligned with NPTEL Mobile Virtual Reality & AI (Week 4: Mobile VR & Cardboard Interaction mechanics).
/// </summary>
public class VRGazeRaycaster : MonoBehaviour
{
    [Header("Raycast & Camera Settings")]
    [Tooltip("The VR camera performing the raycast. If null, Camera.main is used.")]
    public Camera vrCamera;

    [Tooltip("Maximum distance to detect celestial bodies.")]
    public float maxGazeDistance = 150.0f;

    [Tooltip("Layers checked for interactable planets.")]
    public LayerMask interactableLayers = ~0;

    [Header("Gaze Dwell Settings")]
    [Tooltip("Duration in seconds the user must look at a planet to automatically select it.")]
    public float gazeDwellDuration = 1.2f;

    [Header("Reticle UI Elements")]
    [Tooltip("RectTransform of the center reticle pointer.")]
    public RectTransform reticleTransform;

    [Tooltip("Optional image displaying radial dwell progress.")]
    public Image reticleProgressImage;

    [Header("Audio & Manager References")]
    public SolarSystemUIManager uiManager;
    public SolarSystemAudio audioFeedback;

    // Internal tracking state
    private PlanetController currentHoveredPlanet;
    private float currentDwellTimer = 0.0f;
    private Vector3 defaultReticleScale = Vector3.one;
    private Vector3 hoveredReticleScale = new Vector3(1.6f, 1.6f, 1.0f);

    private void Start()
    {
        if (vrCamera == null)
        {
            vrCamera = Camera.main;
        }

        if (reticleTransform != null)
        {
            defaultReticleScale = reticleTransform.localScale;
        }

        if (uiManager == null)
        {
            uiManager = Object.FindFirstObjectByType<SolarSystemUIManager>();
        }

        if (audioFeedback == null)
        {
            audioFeedback = Object.FindFirstObjectByType<SolarSystemAudio>();
        }
    }

    private void Update()
    {
        PerformGazeRaycast();
        CheckInputTrigger();
    }

    /// <summary>
    /// Performs raycasting from the VR camera forward along its line of sight.
    /// </summary>
    private void PerformGazeRaycast()
    {
        if (vrCamera == null) return;

        Ray ray = new Ray(vrCamera.transform.position, vrCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxGazeDistance, interactableLayers))
        {
            PlanetController planet = hit.collider.GetComponentInParent<PlanetController>();

            if (planet != null)
            {
                // Hovering over a new planet or continuing hover
                if (currentHoveredPlanet != planet)
                {
                    ResetCurrentHover();
                    currentHoveredPlanet = planet;
                    currentHoveredPlanet.SetGazeHover(true);
                    
                    if (audioFeedback != null)
                    {
                        audioFeedback.PlayHoverTone();
                    }
                }

                // Accumulate dwell timer
                currentDwellTimer += Time.deltaTime;
                UpdateReticleUI(currentDwellTimer / gazeDwellDuration, true);

                // Dwell activation reached
                if (currentDwellTimer >= gazeDwellDuration)
                {
                    SelectPlanet(currentHoveredPlanet);
                    currentDwellTimer = 0.0f; // Reset to avoid repeated instant triggers
                }

                return;
            }
        }

        // If no planet was hit by the raycast
        if (currentHoveredPlanet != null)
        {
            ResetCurrentHover();
        }
    }

    /// <summary>
    /// Checks for Cardboard physical trigger, screen tap, or mouse click.
    /// </summary>
    private void CheckInputTrigger()
    {
        bool triggered = Input.GetMouseButtonDown(0) || 
                         Input.GetButtonDown("Fire1") || 
                         (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        if (triggered && currentHoveredPlanet != null)
        {
            SelectPlanet(currentHoveredPlanet);
        }
    }

    /// <summary>
    /// Confirms selection of a planet, opening its info panel and triggering camera focus.
    /// </summary>
    private void SelectPlanet(PlanetController planet)
    {
        if (planet == null) return;

        PlanetData data = planet.GetPlanetData();
        if (uiManager != null && data != null)
        {
            uiManager.DisplayPlanetInfo(data, planet.transform);
        }

        if (audioFeedback != null)
        {
            audioFeedback.PlaySelectTone();
        }

        // Animate reticle briefly
        if (reticleTransform != null)
        {
            reticleTransform.localScale = hoveredReticleScale * 1.3f;
        }
    }

    /// <summary>
    /// Resets the current hover state and reticle UI.
    /// </summary>
    private void ResetCurrentHover()
    {
        if (currentHoveredPlanet != null)
        {
            currentHoveredPlanet.SetGazeHover(false);
            currentHoveredPlanet = null;
        }

        currentDwellTimer = 0.0f;
        UpdateReticleUI(0f, false);
    }

    /// <summary>
    /// Updates reticle visual indicators (scale and radial fill).
    /// </summary>
    private void UpdateReticleUI(float progress, bool isHovered)
    {
        if (reticleTransform != null)
        {
            Vector3 targetScale = isHovered ? hoveredReticleScale : defaultReticleScale;
            reticleTransform.localScale = Vector3.Lerp(reticleTransform.localScale, targetScale, Time.deltaTime * 12.0f);
        }

        if (reticleProgressImage != null)
        {
            reticleProgressImage.fillAmount = Mathf.Clamp01(progress);
        }
    }
}
