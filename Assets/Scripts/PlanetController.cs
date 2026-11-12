using UnityEngine;

/// <summary>
/// Controls the planetary physics and kinematics:
/// 1. Axial rotation (spinning on its own axis via transform.Rotate)
/// 2. Orbital revolution (revolving around the Sun via transform.RotateAround)
/// 3. Visual gaze hover/selection feedback for Mobile VR
/// 
/// Aligned strictly with NPTEL Mobile Virtual Reality & AI (Week 1: Transforms, Vectors, and Update loop).
/// </summary>
public class PlanetController : MonoBehaviour
{
    [Header("Orbital & Rotational Parameters")]
    [Tooltip("The central object around which this planet revolves (usually the Sun).")]
    public Transform orbitCenter;

    [Tooltip("Speed of revolution around the Sun (degrees per second).")]
    public float orbitSpeed = 15.0f;

    [Tooltip("Speed of rotation on its own axis (degrees per second).")]
    public float rotationSpeed = 30.0f;

    [Tooltip("Global multiplier to speed up or slow down orbital time.")]
    public static float globalSpeedMultiplier = 1.0f;

    [Tooltip("Global pause flag for orbital motion.")]
    public static bool isMotionPaused = false;

    [Header("VR Selection & Visual Feedback")]
    [SerializeField] private float hoverScaleMultiplier = 1.15f;
    [SerializeField] private float scaleLerpSpeed = 8.0f;

    private Vector3 originalScale;
    private Vector3 currentTargetScale;
    private PlanetData cachedPlanetData;
    private Renderer planetRenderer;
    private Color originalEmissionColor;
    private Material planetMaterial;

    private void Awake()
    {
        originalScale = transform.localScale;
        currentTargetScale = originalScale;
        cachedPlanetData = GetComponent<PlanetData>();
        
        planetRenderer = GetComponent<Renderer>();
        if (planetRenderer != null)
        {
            // Create an instance of the material so each planet's highlight is independent
            planetMaterial = planetRenderer.material;
            if (planetMaterial.HasProperty("_EmissionColor"))
            {
                originalEmissionColor = planetMaterial.GetColor("_EmissionColor");
            }
        }
    }

    private void Update()
    {
        // 1. Axial self-rotation (spinning on own axis)
        if (!isMotionPaused)
        {
            transform.Rotate(Vector3.up * (rotationSpeed * globalSpeedMultiplier * Time.deltaTime), Space.Self);

            // 2. Orbital revolution around the Sun
            if (orbitCenter != null)
            {
                transform.RotateAround(orbitCenter.position, Vector3.up, orbitSpeed * globalSpeedMultiplier * Time.deltaTime);
            }
        }

        // 3. Smooth scale interpolation when hovered in Mobile VR
        if (transform.localScale != currentTargetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, currentTargetScale, Time.deltaTime * scaleLerpSpeed);
        }
    }

    /// <summary>
    /// Triggered when the VR Gaze Reticle enters or leaves the planet collider.
    /// </summary>
    public void SetGazeHover(bool isHovered)
    {
        currentTargetScale = isHovered ? originalScale * hoverScaleMultiplier : originalScale;

        if (planetMaterial != null && planetMaterial.HasProperty("_EmissionColor"))
        {
            if (isHovered)
            {
                planetMaterial.EnableKeyword("_EMISSION");
                planetMaterial.SetColor("_EmissionColor", Color.white * 0.4f);
            }
            else
            {
                planetMaterial.SetColor("_EmissionColor", originalEmissionColor);
            }
        }
    }

    /// <summary>
    /// Returns the attached PlanetData component.
    /// </summary>
    public PlanetData GetPlanetData()
    {
        if (cachedPlanetData == null)
        {
            cachedPlanetData = GetComponent<PlanetData>();
        }
        return cachedPlanetData;
    }

    private void OnDestroy()
    {
        if (planetMaterial != null)
        {
            Destroy(planetMaterial);
        }
    }
}
