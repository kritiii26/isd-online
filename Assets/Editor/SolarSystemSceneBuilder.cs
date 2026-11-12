#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Unity Editor utility to automatically construct the complete Virtual Solar System VR scene.
/// Generates Sun, 8 Planets with Rings, Materials, Orbit Lines, Lights, VR Gaze Camera, and World Space VR UI.
/// Provides automatic scene building on first open, and via the Unity menu: "Solar System VR -> Build Complete Scene".
/// 
/// Strictly aligned with NPTEL Mobile Virtual Reality & AI (Prof. Varun Dutt, IIT Mandi).
/// </summary>
[InitializeOnLoad]
public static class SolarSystemSceneBuilder
{
    static SolarSystemSceneBuilder()
    {
        EditorApplication.delayCall += CheckAndAutoBuildScene;
    }

    private static void CheckAndAutoBuildScene()
    {
        string scenePath = "Assets/Scenes/SolarSystemVR.unity";
        if (!File.Exists(scenePath))
        {
            Debug.Log("<color=cyan><b>[Solar System VR]</b> First-time setup detected. Generating complete VR Solar System scene...</color>");
            BuildScene(false);
        }
    }

    [MenuItem("Solar System VR/Build Complete Scene", false, 1)]
    public static void MenuBuildScene()
    {
        BuildScene(true);
    }

    public static void BuildScene(bool showDialog = true)
    {
        // 1. Create a new empty scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2. Configure Environment Lighting (Deep space ambience)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientSkyColor = new Color(0.04f, 0.04f, 0.07f);

        // 3. Create Starfield Background (Particle System)
        CreateStarfield();

        // 4. Create the Sun
        GameObject sunObj = CreateSun();

        // 5. Create the 8 Planets + Moon + Saturn Rings
        CreateAllPlanets(sunObj.transform);

        // 6. Create UI Canvas (World Space / Overlay for VR)
        GameObject canvasObj = CreateVRUI(out RectTransform reticleRect, out Image reticleImg, out GameObject infoPanel, out Text titleTxt, out Text detailsTxt, out Text pauseTxt, out Text speedTxt, out Text viewTxt);

        // 7. Create VR Camera Rig
        CreateCameraRig(sunObj.transform, reticleRect, reticleImg, infoPanel, titleTxt, detailsTxt, pauseTxt, speedTxt, viewTxt);

        // 8. Ensure destination directory exists and save scene
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        string scenePath = "Assets/Scenes/SolarSystemVR.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green><b>[Solar System VR]</b> Scene successfully built and saved to {scenePath}!</color>");
        
        if (showDialog)
        {
            EditorUtility.DisplayDialog("Solar System VR", "Scene successfully built!\n\nOpen Assets/Scenes/SolarSystemVR.unity and press Play to explore.", "OK");
        }
    }

    private static Font GetDefaultFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (f == null) f = Font.CreateDynamicFontFromOSFont("Arial", 14);
        if (f == null) f = Font.CreateDynamicFontFromOSFont("Helvetica", 14);
        return f;
    }

    private static void CreateStarfield()
    {
        GameObject starsObj = new GameObject("Starfield");
        ParticleSystem ps = starsObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 100f;
        main.startSpeed = 0f;
        main.startSize = 0.45f;
        main.maxParticles = 800;

        var emission = ps.emission;
        emission.rateOverTime = 800f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 120f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(0.7f, 0.85f, 1f), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0.0f), new GradientAlphaKey(0.8f, 1.0f) }
        );
        colorOverLifetime.color = grad;

        var renderer = starsObj.GetComponent<ParticleSystemRenderer>();
        Shader defaultShader = Shader.Find("Particles/Standard Unlit");
        if (defaultShader == null) defaultShader = Shader.Find("Sprites/Default");
        renderer.material = new Material(defaultShader);
    }

    private static GameObject CreateSun()
    {
        GameObject sun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sun.name = "Sun";
        sun.transform.position = Vector3.zero;
        sun.transform.localScale = new Vector3(4.5f, 4.5f, 4.5f);

        // Emissive Sun Material
        Shader standardShader = Shader.Find("Standard");
        if (standardShader == null) standardShader = Shader.Find("Universal Render Pipeline/Lit");
        Material sunMat = new Material(standardShader);
        Color sunColor = new Color(1.0f, 0.85f, 0.2f);
        sunMat.color = sunColor;
        sunMat.EnableKeyword("_EMISSION");
        sunMat.SetColor("_EmissionColor", sunColor * 1.8f);
        sun.GetComponent<Renderer>().sharedMaterial = sunMat;

        // Core Point Light (illuminates planets)
        GameObject lightObj = new GameObject("SunPointLight");
        lightObj.transform.SetParent(sun.transform, false);
        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = new Color(1.0f, 0.96f, 0.90f);
        pointLight.range = 160f;
        pointLight.intensity = 2.4f;

        // Remove Sun's collider so forward gaze raycast is not permanently blocked by the central star
        Object.DestroyImmediate(sun.GetComponent<Collider>());

        // Scripts
        SunGlow glow = sun.AddComponent<SunGlow>();
        glow.sunLight = pointLight;
        glow.rotationSpeed = 3.0f;

        PlanetData data = sun.AddComponent<PlanetData>();
        data.planetName = "Sun";
        data.planetType = "Yellow Dwarf Star (G2V)";
        data.distanceFromSun = "0.0 km (Center of Solar System)";
        data.orbitalPeriod = "N/A (Galactic Orbit: ~230 Million Years)";
        data.rotationPeriod = "25 to 35 Earth Days";
        data.diameter = "1,392,700 km (~109x Earth)";
        data.moonsCount = 8;
        data.averageTemperature = "5,500 °C (Core: 15,000,000 °C)";
        data.funFact = "The Sun contains 99.86% of all the mass in the entire Solar System and produces energy through nuclear fusion of Hydrogen into Helium.";

        PlanetController controller = sun.AddComponent<PlanetController>();
        controller.orbitCenter = null;
        controller.orbitSpeed = 0f;
        controller.rotationSpeed = 3f;

        return sun;
    }

    private static void CreateAllPlanets(Transform sunTransform)
    {
        // Mercury
        CreatePlanet(sunTransform, "Mercury", "Terrestrial Planet", 7.5f, 0.55f, 26f, 10f, new Color(0.68f, 0.65f, 0.62f),
            "57.9 Million km (0.39 AU)", "88 Earth Days", "58.6 Earth Days", "4,879 km", 0, "167 °C (-180 to 430 °C)",
            "Mercury is the smallest planet and experiences the most extreme temperature swings in the Solar System.");

        // Venus
        CreatePlanet(sunTransform, "Venus", "Terrestrial Planet", 11.0f, 0.88f, 19f, -7f, new Color(0.92f, 0.78f, 0.48f),
            "108.2 Million km (0.72 AU)", "225 Earth Days", "243 Earth Days (Retrograde)", "12,104 km", 0, "464 °C",
            "Venus has a runaway greenhouse atmosphere of Carbon Dioxide and sulfuric acid clouds, making it the hottest planet.");

        // Earth
        GameObject earth = CreatePlanet(sunTransform, "Earth", "Terrestrial Planet", 15.0f, 0.95f, 14f, 25f, new Color(0.22f, 0.55f, 0.88f),
            "149.6 Million km (1.00 AU)", "365.25 Earth Days", "23 Hours 56 Mins", "12,742 km", 1, "15 °C",
            "Earth is the only known planet in the universe with abundant liquid water and thriving biological life.");

        // Moon
        CreateMoon(earth.transform, "Moon", 1.8f, 0.25f, 60f, new Color(0.75f, 0.75f, 0.75f));

        // Mars
        CreatePlanet(sunTransform, "Mars", "Terrestrial Planet", 19.5f, 0.70f, 11f, 24f, new Color(0.88f, 0.38f, 0.22f),
            "227.9 Million km (1.52 AU)", "687 Earth Days", "24 Hours 37 Mins", "6,779 km", 2, "-63 °C",
            "Mars is known as the Red Planet due to iron oxide (rust) on its surface and hosts Olympus Mons, the largest volcano in the Solar System.");

        // Jupiter
        CreatePlanet(sunTransform, "Jupiter", "Gas Giant", 26.0f, 2.2f, 7.5f, 45f, new Color(0.85f, 0.72f, 0.55f),
            "778.5 Million km (5.20 AU)", "11.86 Earth Years", "9 Hours 55 Mins", "139,820 km", 95, "-110 °C",
            "Jupiter is the largest planet in our solar system, more than twice as massive as all other planets combined.");

        // Saturn
        GameObject saturn = CreatePlanet(sunTransform, "Saturn", "Gas Giant", 33.0f, 1.8f, 5.5f, 40f, new Color(0.92f, 0.84f, 0.60f),
            "1.43 Billion km (9.58 AU)", "29.45 Earth Years", "10 Hours 33 Mins", "116,460 km", 146, "-140 °C",
            "Saturn is famed for its spectacular ring system composed of billions of chunks of water ice and rock.");

        // Saturn Rings
        CreateSaturnRings(saturn.transform);

        // Uranus
        CreatePlanet(sunTransform, "Uranus", "Ice Giant", 39.5f, 1.35f, 4.0f, -22f, new Color(0.55f, 0.85f, 0.88f),
            "2.87 Billion km (19.2 AU)", "84 Earth Years", "17 Hours 14 Mins", "50,724 km", 28, "-195 °C",
            "Uranus rotates on its side at an extreme 98-degree tilt, likely due to a massive collision early in its history.");

        // Neptune
        CreatePlanet(sunTransform, "Neptune", "Ice Giant", 45.5f, 1.30f, 3.0f, 24f, new Color(0.24f, 0.44f, 0.95f),
            "4.50 Billion km (30.1 AU)", "164.8 Earth Years", "16 Hours 6 Mins", "49,244 km", 16, "-200 °C",
            "Neptune is the most distant major planet and has the fastest supersonic winds recorded, reaching 2,100 km/h.");
    }

    private static GameObject CreatePlanet(Transform sun, string name, string type, float dist, float scale, float orbitSpd, float rotSpd, Color color,
        string distStr, string orbitStr, string rotStr, string diamStr, int moons, string tempStr, string fact)
    {
        GameObject planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        planet.name = name;
        planet.transform.position = new Vector3(dist, 0f, 0f);
        planet.transform.localScale = Vector3.one * scale;

        // Material
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = new Material(shader);
        mat.color = color;
        mat.SetFloat("_Glossiness", 0.35f);
        planet.GetComponent<Renderer>().sharedMaterial = mat;

        // PlanetData
        PlanetData data = planet.AddComponent<PlanetData>();
        data.planetName = name;
        data.planetType = type;
        data.distanceFromSun = distStr;
        data.orbitalPeriod = orbitStr;
        data.rotationPeriod = rotStr;
        data.diameter = diamStr;
        data.moonsCount = moons;
        data.averageTemperature = tempStr;
        data.funFact = fact;

        // PlanetController
        PlanetController controller = planet.AddComponent<PlanetController>();
        controller.orbitCenter = sun;
        controller.orbitSpeed = orbitSpd;
        controller.rotationSpeed = rotSpd;

        // Orbit Line Renderer
        GameObject orbitLineObj = new GameObject($"{name}_OrbitLine");
        OrbitTrailRenderer orbitTrail = orbitLineObj.AddComponent<OrbitTrailRenderer>();
        orbitTrail.centerPoint = sun;
        orbitTrail.orbitRadius = dist;
        orbitTrail.lineWidth = 0.05f;
        orbitTrail.orbitColor = new Color(color.r, color.g, color.b, 0.22f);

        return planet;
    }

    private static void CreateMoon(Transform earth, string name, float dist, float scale, float orbitSpd, Color color)
    {
        GameObject moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        moon.name = name;
        moon.transform.SetParent(earth, false);
        moon.transform.localPosition = new Vector3(dist, 0f, 0f);
        moon.transform.localScale = Vector3.one * scale;

        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = new Material(shader);
        mat.color = color;
        moon.GetComponent<Renderer>().sharedMaterial = mat;

        PlanetController controller = moon.AddComponent<PlanetController>();
        controller.orbitCenter = earth;
        controller.orbitSpeed = orbitSpd;
        controller.rotationSpeed = 15f;
    }

    private static void CreateSaturnRings(Transform saturn)
    {
        GameObject ringObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ringObj.name = "SaturnRings";
        ringObj.transform.SetParent(saturn, false);
        ringObj.transform.localPosition = Vector3.zero;
        ringObj.transform.localRotation = Quaternion.Euler(26f, 0f, 0f);
        ringObj.transform.localScale = new Vector3(3.2f, 0.005f, 3.2f);

        // Remove cylinder collider so raycast passes through cleanly to the planet
        Object.DestroyImmediate(ringObj.GetComponent<Collider>());

        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = new Material(shader);
        mat.color = new Color(0.85f, 0.78f, 0.62f, 0.8f);
        ringObj.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static GameObject CreateVRUI(out RectTransform reticleRect, out Image reticleImg, out GameObject infoPanel, out Text titleTxt, out Text detailsTxt, out Text pauseTxt, out Text speedTxt, out Text viewTxt)
    {
        Font defaultFont = GetDefaultFont();

        // Create Canvas
        GameObject canvasObj = new GameObject("VR_HUD_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Center Reticle (subtle 8x8 dot pointer)
        GameObject reticleObj = new GameObject("VR_Reticle");
        reticleObj.transform.SetParent(canvasObj.transform, false);
        reticleRect = reticleObj.AddComponent<RectTransform>();
        reticleRect.sizeDelta = new Vector2(8, 8);
        reticleImg = reticleObj.AddComponent<Image>();
        reticleImg.color = new Color(1f, 1f, 1f, 0.90f);

        // Info Panel
        infoPanel = new GameObject("PlanetInfoPanel");
        infoPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform infoRect = infoPanel.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.02f, 0.45f);
        infoRect.anchorMax = new Vector2(0.38f, 0.95f);
        infoRect.offsetMin = Vector2.zero;
        infoRect.offsetMax = Vector2.zero;

        Image panelBg = infoPanel.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.08f, 0.14f, 0.90f);

        // Planet Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(infoPanel.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.82f);
        titleRect.anchorMax = new Vector2(0.95f, 0.98f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        titleTxt = titleObj.AddComponent<Text>();
        titleTxt.font = defaultFont;
        titleTxt.text = "PLANET NAME";
        titleTxt.fontSize = 22;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.color = new Color(1.0f, 0.85f, 0.25f);
        titleTxt.alignment = TextAnchor.MiddleLeft;

        // Details Text
        GameObject detailsObj = new GameObject("DetailsText");
        detailsObj.transform.SetParent(infoPanel.transform, false);
        RectTransform detailsRect = detailsObj.AddComponent<RectTransform>();
        detailsRect.anchorMin = new Vector2(0.05f, 0.12f);
        detailsRect.anchorMax = new Vector2(0.95f, 0.80f);
        detailsRect.offsetMin = Vector2.zero;
        detailsRect.offsetMax = Vector2.zero;
        detailsTxt = detailsObj.AddComponent<Text>();
        detailsTxt.font = defaultFont;
        detailsTxt.text = "Select any planet using your VR Gaze reticle to explore.";
        detailsTxt.fontSize = 14;
        detailsTxt.color = Color.white;
        detailsTxt.alignment = TextAnchor.UpperLeft;

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(infoPanel.transform, false);
        RectTransform closeRect = closeBtnObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.70f, 0.02f);
        closeRect.anchorMax = new Vector2(0.95f, 0.10f);
        closeRect.offsetMin = Vector2.zero;
        closeRect.offsetMax = Vector2.zero;
        Image closeImg = closeBtnObj.AddComponent<Image>();
        closeImg.color = new Color(0.7f, 0.2f, 0.2f);
        closeBtnObj.AddComponent<Button>();

        GameObject closeTxtObj = new GameObject("CloseText");
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTxtRect = closeTxtObj.AddComponent<RectTransform>();
        closeTxtRect.anchorMin = Vector2.zero;
        closeTxtRect.anchorMax = Vector2.one;
        closeTxtRect.offsetMin = Vector2.zero;
        closeTxtRect.offsetMax = Vector2.zero;
        Text closeTxt = closeTxtObj.AddComponent<Text>();
        closeTxt.font = defaultFont;
        closeTxt.text = "Close";
        closeTxt.fontSize = 13;
        closeTxt.color = Color.white;
        closeTxt.alignment = TextAnchor.MiddleCenter;

        // Bottom Controls Bar
        GameObject controlBar = new GameObject("ControlBar");
        controlBar.transform.SetParent(canvasObj.transform, false);
        RectTransform barRect = controlBar.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.08f, 0.02f);
        barRect.anchorMax = new Vector2(0.92f, 0.08f);
        barRect.offsetMin = Vector2.zero;
        barRect.offsetMax = Vector2.zero;
        Image barBg = controlBar.AddComponent<Image>();
        barBg.color = new Color(0.04f, 0.06f, 0.10f, 0.85f);

        // 1. Pause Button
        GameObject pauseBtnObj = CreateButton(controlBar.transform, "PauseBtn", new Vector2(0.02f, 0.1f), new Vector2(0.24f, 0.9f), "Pause Orbits", defaultFont, out pauseTxt);

        // 2. Speed Button
        GameObject speedBtnObj = CreateButton(controlBar.transform, "SpeedBtn", new Vector2(0.26f, 0.1f), new Vector2(0.48f, 0.9f), "Speed: 1x", defaultFont, out speedTxt);

        // 3. Dedicated Top View Button
        Text topViewTxt;
        GameObject topViewBtnObj = CreateButton(controlBar.transform, "TopViewBtn", new Vector2(0.50f, 0.1f), new Vector2(0.72f, 0.9f), "Top View [T]", defaultFont, out topViewTxt);

        // 4. Cycle View Button
        GameObject viewBtnObj = CreateButton(controlBar.transform, "ViewBtn", new Vector2(0.74f, 0.1f), new Vector2(0.98f, 0.9f), "Cycle View [C]", defaultFont, out viewTxt);

        infoPanel.SetActive(false); // Initially hidden until a planet is selected

        return canvasObj;
    }

    private static GameObject CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string defaultLabel, Font font, out Text btnTxt)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.25f, 0.40f);
        btnObj.AddComponent<Button>();

        GameObject txtObj = new GameObject("Label");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        btnTxt = txtObj.AddComponent<Text>();
        btnTxt.font = font;
        btnTxt.text = defaultLabel;
        btnTxt.fontSize = 13;
        btnTxt.fontStyle = FontStyle.Bold;
        btnTxt.color = Color.white;
        btnTxt.alignment = TextAnchor.MiddleCenter;

        return btnObj;
    }

    private static void CreateCameraRig(Transform sun, RectTransform reticleRect, Image reticleImg, GameObject infoPanel, Text titleTxt, Text detailsTxt, Text pauseTxt, Text speedTxt, Text viewTxt)
    {
        GameObject camObj = new GameObject("VR_Main_Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.015f, 0.015f, 0.035f);
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 500f;
        camObj.AddComponent<AudioListener>();

        // Audio Feedback
        SolarSystemAudio audio = camObj.AddComponent<SolarSystemAudio>();

        // Camera Controller
        CameraController camController = camObj.AddComponent<CameraController>();
        camController.currentView = CameraController.ViewMode.AngledPerspective;

        // VR Head Tracker
        camObj.AddComponent<VRHeadTracker>();

        // VR Gaze Raycaster
        VRGazeRaycaster gaze = camObj.AddComponent<VRGazeRaycaster>();
        gaze.vrCamera = cam;
        gaze.reticleTransform = reticleRect;
        gaze.reticleProgressImage = reticleImg;
        gaze.audioFeedback = audio;

        // UI Manager
        GameObject uiManagerObj = new GameObject("UIManager");
        SolarSystemUIManager uiManager = uiManagerObj.AddComponent<SolarSystemUIManager>();
        uiManager.infoPanel = infoPanel;
        uiManager.planetTitleText = titleTxt;
        uiManager.planetDetailsText = detailsTxt;
        uiManager.pauseButtonText = pauseTxt;
        uiManager.speedButtonText = speedTxt;
        uiManager.currentViewText = viewTxt;
        uiManager.vrCamera = cam;
        uiManager.cameraController = camController;
        uiManager.audioFeedback = audio;

        gaze.uiManager = uiManager;

        // Wire button events
        Button closeBtn = infoPanel.GetComponentInChildren<Button>();
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(uiManager.CloseInfoPanel);
        }

        Transform controlBar = pauseTxt.transform.parent.parent;
        Button[] buttons = controlBar.GetComponentsInChildren<Button>();
        if (buttons.Length >= 4)
        {
            buttons[0].onClick.AddListener(uiManager.TogglePause);
            buttons[1].onClick.AddListener(uiManager.CycleSpeed);
            buttons[2].onClick.AddListener(uiManager.SetTopView);
            buttons[3].onClick.AddListener(uiManager.CycleCameraView);
        }
    }
}
#endif
