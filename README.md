# Virtual Solar System (Mobile VR & AI)

**NPTEL Swayam Course Project:** Mobile Virtual Reality and Artificial Intelligence (`noc26_cs186`)  
**Instructor:** Prof. Varun Dutt, IIT Mandi  
**Primary Engine:** Unity 3D & C# Scripting  

---

## 🌌 Project Overview
The **Virtual Solar System** is an interactive Mobile Virtual Reality (VR) application designed strictly within the syllabus and tools taught in **NPTEL NOC26_CS186: Mobile Virtual Reality and Artificial Intelligence**.

Users explore the Sun and the eight revolving planets of our solar system (Mercury, Venus, Earth with Moon, Mars, Jupiter, Saturn with rings, Uranus, and Neptune). In accordance with Google Cardboard / Mobile VR design principles, user interaction is driven by **Gaze Raycasting** and a **Center Reticle Pointer**: simply gazing at any celestial body reveals a detailed, interactive 3D World-Space Information Card featuring real astronomical data and scientific facts.

---

## 📚 Direct Mapping to NPTEL Course Syllabus

| Course Module | Concepts Taught in NOC26_CS186 | Implementation in this Project | Corresponding Script / Asset |
| :--- | :--- | :--- | :--- |
| **Week 1: Foundations & Unity Setup** | Unity Editor interface, GameObjects, 3D Primitives (Sphere, Cylinder), Transforms | Sun (glowing sphere), 8 planets with scaled radii, Saturn's rings | `Assets/Scenes/SolarSystemVR.unity`, `SolarSystemSceneBuilder.cs` |
| **Week 1: Materials & Lighting** | Creating Materials, Colors, Emission, Point & Directional Lights | Sun with Emissive yellow material + central `PointLight` casting real-time light and shadows onto revolving planets | `SunGlow.cs`, Standard / URP Materials |
| **Week 1: C# Programming** | `MonoBehaviour`, variables, methods, `Start()`, `Update()`, `transform.Rotate`, `transform.RotateAround` | Daily axial rotation (`transform.Rotate`) and orbital revolution (`transform.RotateAround`) | `PlanetController.cs`, `PlanetData.cs` |
| **Week 1: User Interaction** | Inputs (`Input.GetKeyDown`, `Input.GetMouseButtonDown`), Player movements, **Cycling Cameras** | 3 preset camera viewpoints (Top Overview, Angled Perspective, Inner Cockpit) + smooth target tracking | `CameraController.cs` |
| **Week 1: Menus & UI Canvas** | Canvas, RectTransform, UI Text, Buttons, Panels | Planet Info Card (Name, Type, Distance, Periods, Moons, Facts), Pause/Resume toggle, Speed multiplier | `SolarSystemUIManager.cs` |
| **Week 1: Particle Systems** | Particle System setup, emission, shape | Deep space 3D starfield backdrop with 800 twinkling particles | Built via `SolarSystemSceneBuilder.cs` |
| **Week 4: Mobile VR & Cardboard** | Google VR / Cardboard SDK, **Gaze-based interaction**, **Reticle pointer**, **Raycasting** | Camera raycast (`Physics.Raycast`), center reticle with dwell progress ring (1.2s dwell) & screen tap trigger | `VRGazeRaycaster.cs`, `VRHeadTracker.cs` |
| **Week 4: Mobile VR Head Tracking** | Gyroscope head tracking & Editor look emulation | Mobile device gyroscope (`Input.gyro.attitude`) with right-click mouse-look emulation in Editor | `VRHeadTracker.cs`, `CameraController.cs` |

---

## 🪐 Planetary Data Included

Every celestial body contains pre-configured scientific data in its `PlanetData` component:
- **Sun:** Yellow Dwarf Star (G2V), 1,392,700 km diameter, 5,500 °C surface, nuclear fusion facts.
- **Mercury:** Terrestrial Planet, 57.9M km from Sun, 88-day orbit, extreme temperature swings.
- **Venus:** Terrestrial Planet, runaway greenhouse effect, 464 °C surface temperature.
- **Earth:** Terrestrial Planet with Moon, liquid water oceans, 365.25-day orbit.
- **Mars:** Terrestrial Planet, iron oxide rust surface, Olympus Mons volcano.
- **Jupiter:** Gas Giant, 778.5M km from Sun, 95 moons, Great Red Spot.
- **Saturn:** Gas Giant with realistic icy rings, 146 moons.
- **Uranus:** Ice Giant, 98-degree extreme axial tilt, pale cyan atmosphere.
- **Neptune:** Ice Giant, supersonic 2,100 km/h methane winds, deep azure appearance.

---

## 🎮 How to Open & Run the Project in Unity

### 1. Requirements
- **Unity Hub** & **Unity Editor** (Compatible with Unity 2021.3 LTS, 2022.3 LTS, or Unity 6 `6000.x`).
- Standard Unity Modules: Physics, Audio, UI (uGUI).

### 2. Opening in Unity
1. Open **Unity Hub**.
2. Click **Add** -> **Add project from disk**.
3. Select this folder: `isd-online-project`.
4. Open the project with your installed Unity Editor.

### 3. Automatic Scene Setup
- **Automatic:** The project includes an editor script (`SolarSystemSceneBuilder.cs`) configured with `[InitializeOnLoad]`. On first opening, it automatically constructs and saves the complete scene to `Assets/Scenes/SolarSystemVR.unity`.
- **Manual Menu Item:** If you ever wish to re-generate the entire scene from scratch, simply click:  
  **`Solar System VR` -> `Build Complete Scene`** from the Unity top menu bar.

### 4. Running the Simulation
1. In the Project window, double-click `Assets/Scenes/SolarSystemVR.unity`.
2. Press the **Play (▶)** button at the top of the Unity Editor.

---

## 🕹️ Controls & Interaction Guide

| Action | VR / Cardboard Mode | Desktop / Unity Editor Mode |
| :--- | :--- | :--- |
| **Inspect a Planet** | Gaze at any planet for **1.2 seconds** (or press Cardboard trigger) | Look at planet with reticle, or Left-Click on planet |
| **Look Around (VR Head Look)** | Move your head (mobile gyroscope) | Hold **Right Mouse Button** or **Left Alt** and move mouse |
| **Direct Top View (Orbital Map)** | Gaze at / Click "Top View [T]" on HUD | Press **`T`** key |
| **Pause / Resume Revolution** | Look at / Click "Pause Orbits" on HUD | Press **`Space`** key |
| **Cycle Camera Views** | Look at / Click "Cycle View [C]" on HUD | Press **`C`** key (Cycles Top View -> Angled -> Cockpit) |
| **Change Simulation Speed** | Look at / Click "Speed" on HUD | Press **`S`** key (Cycles 1x -> 2x -> 5x -> 0.5x) |
| **Close Info Panel** | Gaze at "Close" button on panel | Press **`Escape`** key |

---

## 🌐 Instant In-Browser Preview (`index.html`)

For instant verification or quick grading without launching Unity, a zero-dependency interactive WebGL 3D preview is provided in `index.html`:
- Double-click **`index.html`** or open it in Google Chrome / Safari / Firefox.
- Features the exact same celestial bodies, rotation speeds, revolution speeds, VR gaze reticle, procedural audio chimes, and interactive info cards.

---

## 📁 Project File Structure

```text
isd-online-project/
├── Assets/
│   ├── Editor/
│   │   └── SolarSystemSceneBuilder.cs   # Automatic one-click scene generator
│   ├── Scenes/
│   │   └── SolarSystemVR.unity          # Main Unity VR scene
│   └── Scripts/
│       ├── PlanetData.cs                # Celestial data model (astronomy statistics & facts)
│       ├── PlanetController.cs          # Axial rotation (Rotate) & orbital revolution (RotateAround)
│       ├── OrbitTrailRenderer.cs        # Circular orbit tracks via LineRenderer
│       ├── VRGazeRaycaster.cs           # Google Cardboard raycasting & gaze dwell detection
│       ├── VRHeadTracker.cs             # Mobile gyroscope head tracking & mouse-look emulation
│       ├── SolarSystemUIManager.cs      # World-Space VR UI, Info Card, and HUD menus
│       ├── CameraController.cs          # Camera cycling (Overview, Angled, Cockpit, Focus)
│       └── SolarSystemAudio.cs          # Zero-dependency procedural audio tone synthesizer
├── Packages/
│   └── manifest.json                    # Minimal package dependencies
├── ProjectSettings/
│   └── ProjectVersion.txt               # Unity version configuration
├── index.html                           # Standalone Web 3D interactive preview
└── README.md                            # Complete course documentation
```
