using UnityEngine;

/// <summary>
/// Renders a lightweight, smooth circular orbital trajectory around the Sun using Unity's built-in LineRenderer.
/// Taught in Week 1 (Components, LineRenderers, and Math for Circles: Sin/Cos).
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class OrbitTrailRenderer : MonoBehaviour
{
    [Tooltip("The central object around which this orbit is drawn (typically the Sun).")]
    public Transform centerPoint;

    [Tooltip("Distance from the center point to the orbit line.")]
    public float orbitRadius = 10f;

    [Tooltip("Number of segments used to draw a smooth circle.")]
    public int segments = 80;

    [Tooltip("Width of the orbit line.")]
    public float lineWidth = 0.04f;

    [Tooltip("Color of the orbit line.")]
    public Color orbitColor = new Color(0.4f, 0.6f, 0.9f, 0.25f);

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        ConfigureLineRenderer();
        DrawOrbit();
    }

    private void ConfigureLineRenderer()
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.loop = true;

        // Use standard transparent unlit shader or vertex color
        Shader unlitShader = Shader.Find("Sprites/Default");
        if (unlitShader == null) unlitShader = Shader.Find("UI/Default");
        
        Material lineMat = new Material(unlitShader);
        lineMat.color = orbitColor;
        lineRenderer.material = lineMat;
        lineRenderer.startColor = orbitColor;
        lineRenderer.endColor = orbitColor;
    }

    /// <summary>
    /// Calculates the circle vertices around the center point.
    /// </summary>
    public void DrawOrbit()
    {
        Vector3 center = (centerPoint != null) ? centerPoint.position : Vector3.zero;
        float angleStep = 360f / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = center.x + Mathf.Cos(angle) * orbitRadius;
            float z = center.z + Mathf.Sin(angle) * orbitRadius;
            lineRenderer.SetPosition(i, new Vector3(x, center.y, z));
        }
    }
}
