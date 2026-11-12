using UnityEngine;

/// <summary>
/// Holds scientific and descriptive data for a celestial body in the Virtual Solar System.
/// Aligned with the NPTEL Mobile Virtual Reality & AI curriculum (Week 1: C# Variables, Data structures, and Inspector fields).
/// </summary>
public class PlanetData : MonoBehaviour
{
    [Header("Basic Identification")]
    public string planetName = "Unknown";
    public string planetType = "Terrestrial Planet"; // e.g. Star, Terrestrial, Gas Giant, Ice Giant
    
    [Header("Physical & Orbital Characteristics")]
    public string distanceFromSun = "149.6 Million km (1.0 AU)";
    public string orbitalPeriod = "365.25 Days";
    public string rotationPeriod = "24 Hours";
    public string diameter = "12,742 km";
    public int moonsCount = 1;
    public string averageTemperature = "15 °C";
    
    [Header("Educational Information")]
    [TextArea(3, 6)]
    public string funFact = "Earth is the only known planet in the universe that supports life and possesses liquid water oceans covering over 70% of its surface.";

    /// <summary>
    /// Returns a multi-line formatted string suitable for displaying inside a VR World-Space UI text panel.
    /// </summary>
    public string GetFormattedDetails()
    {
        return $"<b>Type:</b> {planetType}\n" +
               $"<b>Distance from Sun:</b> {distanceFromSun}\n" +
               $"<b>Orbital Period:</b> {orbitalPeriod}\n" +
               $"<b>Rotation Period:</b> {rotationPeriod}\n" +
               $"<b>Diameter:</b> {diameter}\n" +
               $"<b>Known Moons:</b> {moonsCount}\n" +
               $"<b>Avg Temperature:</b> {averageTemperature}\n\n" +
               $"<b>Fact:</b> <i>{funFact}</i>";
    }
}
