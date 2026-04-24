using UnityEngine;
using UnityEngine.UI;

/// Builds the whole Interactive Solar System scene at runtime.
/// Drop this on an empty GameObject in SampleScene and press Play.
public class SolarSystemBootstrap : MonoBehaviour
{
    void Awake()
    {
        BuildLights();
        GameObject sun = BuildSun();
        GameObject planet = BuildPlanet(sun.transform);
        BuildMoon(planet.transform);
        Camera cam = BuildCamera();
        BuildUI(cam);
    }

    // ---------- Lighting ----------
    void BuildLights()
    {
        // Friendly bright ambient sky
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.5f);

        GameObject dirGO = new GameObject("DirectionalLight");
        Light dir = dirGO.AddComponent<Light>();
        dir.type = LightType.Directional;
        dir.color = new Color(1f, 0.97f, 0.9f);
        dir.intensity = 0.9f;
        dirGO.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
    }

    // ---------- Sun ----------
    GameObject BuildSun()
    {
        GameObject sun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sun.name = "Sun";
        sun.transform.position = Vector3.zero;
        sun.transform.localScale = Vector3.one * 3f;

        sun.GetComponent<Renderer>().material = LoadMat("SunMaterial",
            new Color(1f, 0.75f, 0.1f), emissive: true,
            emissionColor: new Color(1f, 0.6f, 0.1f) * 1.2f);

        // Sun point light
        GameObject lightGO = new GameObject("SunLight");
        lightGO.transform.SetParent(sun.transform, false);
        Light pt = lightGO.AddComponent<Light>();
        pt.type = LightType.Point;
        pt.color = new Color(1f, 0.85f, 0.5f);
        pt.intensity = 2.2f;
        pt.range = 40f;

        // Self rotation only
        var spin = sun.AddComponent<OrbitRotation>();
        spin.orbitCenter = null;
        spin.selfRotationSpeed = 12f;

        AddClickable(sun, "Sun",
            "The Sun is a giant ball of hot gas. It gives us light and keeps us warm!",
            new Color(1f, 0.9f, 0.4f));
        return sun;
    }

    // ---------- Planet ----------
    GameObject BuildPlanet(Transform sun)
    {
        // Parent pivot at sun for stable orbit
        GameObject planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        planet.name = "Planet";
        planet.transform.position = new Vector3(8f, 0f, 0f);
        planet.transform.localScale = Vector3.one * 1.4f;

        planet.GetComponent<Renderer>().material = LoadMat("EarthMaterial",
            new Color(0.25f, 0.55f, 0.95f));

        var orbit = planet.AddComponent<OrbitRotation>();
        orbit.orbitCenter = sun;
        orbit.orbitSpeed = 18f;
        orbit.selfRotationSpeed = 40f;

        AddClickable(planet, "Blue Planet",
            "This planet is blue because it is covered in water, just like Earth!",
            new Color(0.5f, 0.8f, 1f));
        return planet;
    }

    // ---------- Moon ----------
    void BuildMoon(Transform planet)
    {
        GameObject moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        moon.name = "Moon";
        moon.transform.position = planet.position + new Vector3(2.2f, 0.4f, 0f);
        moon.transform.localScale = Vector3.one * 0.5f;

        moon.GetComponent<Renderer>().material = LoadMat("MoonMaterial",
            new Color(0.85f, 0.85f, 0.88f));

        var orbit = moon.AddComponent<OrbitRotation>();
        orbit.orbitCenter = planet;
        orbit.orbitSpeed = 70f;
        orbit.selfRotationSpeed = 30f;

        AddClickable(moon, "Moon",
            "The Moon goes around the planet. At night, it shines in the sky!",
            new Color(1f, 1f, 0.7f));
    }

    // ---------- Camera ----------
    Camera BuildCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
        }
        cam.transform.position = new Vector3(0f, 8f, -18f);
        cam.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
        cam.backgroundColor = new Color(0.03f, 0.03f, 0.1f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        if (cam.GetComponent<CameraFocusController>() == null)
            cam.gameObject.AddComponent<CameraFocusController>();
        return cam;
    }

    // ---------- UI ----------
    void BuildUI(Camera cam)
    {
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Title banner
        CreateText(canvasGO.transform, "TitleText", "Solar System Explorer",
            new Vector2(0, -60), new Vector2(900, 90), 52, TextAnchor.MiddleCenter,
            new Color(1f, 0.95f, 0.6f), TextAnchor.UpperCenter);

        // Instruction
        CreateText(canvasGO.transform, "HintText",
            "Click the Sun, Planet, or Moon!   (Press ESC or Back to return)",
            new Vector2(0, 60), new Vector2(1200, 50), 28, TextAnchor.MiddleCenter,
            Color.white, TextAnchor.LowerCenter);

        // Info panel
        GameObject panel = new GameObject("InfoPanel");
        panel.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = new Vector2(0, -300);
        panelRT.sizeDelta = new Vector2(1100, 260);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.1f, 0.3f, 0.85f);

        Text title = CreateText(panel.transform, "Title", "",
            new Vector2(0, -45), new Vector2(1000, 70), 48, TextAnchor.MiddleCenter,
            new Color(1f, 0.85f, 0.3f), TextAnchor.UpperCenter);

        Text fact = CreateText(panel.transform, "Fact", "",
            new Vector2(0, 0), new Vector2(1000, 140), 32, TextAnchor.MiddleCenter,
            Color.white, TextAnchor.MiddleCenter);
        RectTransform factRT = fact.GetComponent<RectTransform>();
        factRT.anchorMin = new Vector2(0.5f, 0.5f);
        factRT.anchorMax = new Vector2(0.5f, 0.5f);
        factRT.anchoredPosition = new Vector2(0, -20);

        panel.SetActive(false);

        // Back button
        GameObject btnGO = new GameObject("BackButton");
        btnGO.transform.SetParent(canvasGO.transform, false);
        RectTransform btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0, 1);
        btnRT.anchorMax = new Vector2(0, 1);
        btnRT.pivot = new Vector2(0, 1);
        btnRT.anchoredPosition = new Vector2(30, -30);
        btnRT.sizeDelta = new Vector2(200, 80);
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(1f, 0.5f, 0.3f, 0.95f);
        Button btn = btnGO.AddComponent<Button>();

        Text btnText = CreateText(btnGO.transform, "Text", "< Back",
            Vector2.zero, new Vector2(200, 80), 36, TextAnchor.MiddleCenter, Color.white, TextAnchor.MiddleCenter);
        RectTransform btnTextRT = btnText.GetComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = Vector2.zero;
        btnTextRT.offsetMax = Vector2.zero;

        // Wire UI into camera + clickables
        CameraFocusController ctrl = cam.GetComponent<CameraFocusController>();
        ctrl.backButton = btn;

        AudioClip click = MakeClickClip();
        foreach (var c in Object.FindObjectsByType<ClickableCelestial>(FindObjectsSortMode.None))
        {
            c.infoPanel = panel;
            c.titleText = title;
            c.factText = fact;
            c.cameraController = ctrl;
            c.clickSound = click;
        }
    }

    // ---------- Helpers ----------
    Text CreateText(Transform parent, string name, string content,
        Vector2 anchoredPos, Vector2 size, int fontSize, TextAnchor align,
        Color color, TextAnchor anchorPreset)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();

        Vector2 min = new Vector2(0.5f, 1f), max = new Vector2(0.5f, 1f), piv = new Vector2(0.5f, 1f);
        if (anchorPreset == TextAnchor.LowerCenter) { min = new Vector2(0.5f, 0f); max = new Vector2(0.5f, 0f); piv = new Vector2(0.5f, 0f); }
        else if (anchorPreset == TextAnchor.MiddleCenter) { min = new Vector2(0.5f, 0.5f); max = new Vector2(0.5f, 0.5f); piv = new Vector2(0.5f, 0.5f); }

        rt.anchorMin = min; rt.anchorMax = max; rt.pivot = piv;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Text t = go.AddComponent<Text>();
        t.text = content;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = fontSize;
        t.alignment = align;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    // Loads a .mat from any Resources folder. Falls back to a generated material if missing.
    Material LoadMat(string resourceName, Color fallbackColor,
        bool emissive = false, Color? emissionColor = null)
    {
        Material m = Resources.Load<Material>(resourceName);
        if (m != null)
        {
            Material inst = new Material(m);
            if (emissive)
            {
                inst.EnableKeyword("_EMISSION");
                if (emissionColor.HasValue && inst.HasProperty("_EmissionColor"))
                    inst.SetColor("_EmissionColor", emissionColor.Value);
            }
            return inst;
        }
        return MakeMaterial(fallbackColor, emissive, emissionColor);
    }

    Material MakeMaterial(Color baseColor, bool emissive = false, Color? emissionColor = null)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material m = new Material(shader);
        if (m.HasProperty("_Color")) m.color = baseColor;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", baseColor);
        if (emissive && emissionColor.HasValue)
        {
            m.EnableKeyword("_EMISSION");
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", emissionColor.Value);
        }
        return m;
    }

    void AddClickable(GameObject go, string name, string fact, Color highlight)
    {
        var cc = go.AddComponent<ClickableCelestial>();
        cc.objectName = name;
        cc.funFact = fact;
        cc.highlightColor = highlight;
        cc.highlightIntensity = 1.8f;
    }

    // Generates a short "blip" click sound procedurally so no audio file is needed.
    AudioClip MakeClickClip()
    {
        int sampleRate = 44100;
        float duration = 0.12f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 25f);
            float tone = Mathf.Sin(2f * Mathf.PI * 660f * t) * 0.6f
                       + Mathf.Sin(2f * Mathf.PI * 990f * t) * 0.3f;
            data[i] = tone * envelope;
        }
        AudioClip clip = AudioClip.Create("ClickBlip", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
