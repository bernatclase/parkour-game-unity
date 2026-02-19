using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

/// <summary>
/// Genera un nivel de plataformas 3D proceduralmente.
/// Crea el jugador, la cámara, el GameManager, la UI y las plataformas con checkpoints y monedas.
/// </summary>
public class GameSetup : MonoBehaviour
{
    [Header("Configuración del nivel")]
    public int numberOfPlatforms = 30;
    public float platformSpacing = 5.5f;

    [Header("Estética")]
    public Color skyBottomColor = new Color(0.78f, 0.9f, 1.0f);
    public Color platformColor  = new Color(0.1f, 0.6f, 1.0f);
    public Color ninjaColor     = new Color(0.12f, 0.12f, 0.15f);

    void Start()
    {
        try
        {
            CleanScene();
            SetupEnvironment();
            CreateLevel();
            Debug.Log("<color=cyan>[GameSetup]</color> ¡Nivel generado exitosamente!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameSetup] Error Crítico: {e.Message}\n{e.StackTrace}");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LIMPIEZA
    // ─────────────────────────────────────────────────────────────────────────
    void CleanScene()
    {
        string[] toRemove = {
            "Global Light 2D", "Plane", "Capsule", "Directional Light",
            "Main Camera", "Canvas", "EventSystem", "GameManager", "Fill Light"
        };
        foreach (string n in toRemove)
        {
            GameObject obj = GameObject.Find(n);
            if (obj != null) DestroyImmediate(obj);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ENTORNO (luces / niebla)
    // ─────────────────────────────────────────────────────────────────────────
    void SetupEnvironment()
    {
        // Cámara Principal
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.backgroundColor = skyBottomColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        camObj.AddComponent<AudioListener>();

        // Luz principal
        GameObject sunObj = new GameObject("Directional Light");
        Light sun = sunObj.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.6f;
        sun.color = new Color(1f, 0.96f, 0.88f);
        sun.shadows = LightShadows.Soft;
        sunObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Luz de relleno
        GameObject fillObj = new GameObject("Fill Light");
        Light fill = fillObj.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.5f;
        fill.color = new Color(0.85f, 0.9f, 1f);
        fill.shadows = LightShadows.None;
        fillObj.transform.rotation = Quaternion.Euler(-30f, 150f, 0f);

        RenderSettings.fog = true;
        RenderSettings.fogColor = skyBottomColor;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.015f;
        RenderSettings.ambientLight = new Color(0.3f, 0.35f, 0.45f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // NIVEL
    // ─────────────────────────────────────────────────────────────────────────
    void CreateLevel()
    {
        // 1. GameManager  ← PRIMERO siempre
        new GameObject("GameManager").AddComponent<GameManager>();

        // 2. UI  ← SEGUNDO (necesita GameManager en Start)
        CreateUI();

        // 3. Jugador
        GameObject player = CreatePlayer();

        // 4. Cámara Follow
        CameraFollow cf = Camera.main.gameObject.AddComponent<CameraFollow>();
        cf.target = player.transform;
        cf.offset = new Vector3(0f, 6f, -10f);

        // 5. Plataformas
        CreatePlatform(Vector3.zero, new Vector3(10f, 1f, 10f), platformColor); // inicio

        Vector3 lastPos = Vector3.zero;
        int coinCount = 0;
        for (int i = 0; i < numberOfPlatforms; i++)
        {
            float x = Random.Range(-3f, 3f);
            float y = Random.Range(-0.3f, 1.2f);
            float z = platformSpacing + Random.Range(0f, 1f);
            Vector3 pos = lastPos + new Vector3(x, y, z);

            CreatePlatform(pos, new Vector3(Random.Range(3f, 5f), 0.8f, Random.Range(3f, 5f)), platformColor);

            if (Random.value > 0.3f)
            {
                CreateCoin(pos + Vector3.up * 1.5f);
                coinCount++;
            }
            if (i > 0 && i % 7 == 0)  CreateCheckpoint(pos + Vector3.up * 0.6f);

            lastPos = pos;
        }

        // Ajustar coinsToWin al número real de monedas generadas (mínimo 1)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coinsToWin = Mathf.Max(1, coinCount);
            Debug.Log($"<color=cyan>[GameSetup]</color> Monedas generadas: {coinCount}, coinsToWin = {GameManager.Instance.coinsToWin}");
            if (UIManager.Instance != null) UIManager.Instance.UpdateUI();
        }

        // Meta — plataforma verde con trigger de victoria
        GameObject goalPlatform = CreatePlatform(lastPos + new Vector3(0, 1, 6), new Vector3(8, 1, 8), Color.green);
        goalPlatform.name = "Goal";

        // Trigger invisible encima de la plataforma de meta
        GameObject goalTrigger = new GameObject("GoalTrigger");
        goalTrigger.transform.position = goalPlatform.transform.position + Vector3.up * 1.5f;
        BoxCollider goalCol = goalTrigger.AddComponent<BoxCollider>();
        goalCol.isTrigger = true;
        goalCol.size = new Vector3(7f, 3f, 7f);
        goalTrigger.AddComponent<GoalTrigger>();

        // Estrella decorativa sobre la meta
        GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        star.name = "GoalStar";
        star.transform.position = goalPlatform.transform.position + Vector3.up * 4f;
        star.transform.localScale = Vector3.one * 1.5f;
        SetColor(star, Color.yellow);
        Collider starCol = star.GetComponent<Collider>();
        if (starCol != null) DestroyImmediate(starCol);
        star.AddComponent<GoalStarAnimation>();

        // Kill Zone — usamos un Cube muy plano porque el Plane usa MeshCollider
        // que NO soporta isTrigger. El Cube usa BoxCollider, que sí lo soporta.
        GameObject kz = GameObject.CreatePrimitive(PrimitiveType.Cube);
        kz.name = "KillZone";
        kz.transform.position   = new Vector3(lastPos.x * 0.5f, -22f, lastPos.z * 0.5f);
        kz.transform.localScale = new Vector3(2000f, 1f, 2000f); // enorme y plano
        kz.GetComponent<Collider>().isTrigger      = true;       // BoxCollider: OK con isTrigger
        kz.GetComponent<Renderer>().enabled        = false;      // invisible
        kz.AddComponent<KillZoneTrigger>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // JUGADOR
    // ─────────────────────────────────────────────────────────────────────────
    GameObject CreatePlayer()
    {
        GameObject player = null;

#if UNITY_EDITOR
        // Cargar directamente el modelo (no la animación @Walking que también contiene "Ch24_nonPBR")
        GameObject ninjaModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Ch24_nonPBR.fbx");
        if (ninjaModel != null)
        {
            player = Instantiate(ninjaModel, new Vector3(0f, 1.5f, 0f), Quaternion.identity);
            player.name = "Player";
            FixModelMaterials(player);
            SetupPlayerAnimator(player);
        }
#endif

        if (player == null)
            player = CreateProceduralNinja();

        player.tag = "Player";

        // Collider
        CapsuleCollider cap = player.GetComponent<CapsuleCollider>();
        if (cap == null) cap = player.AddComponent<CapsuleCollider>();
        cap.height = 2.0f;
        cap.radius = 0.3f;
        cap.center = new Vector3(0, 1.0f, 0);

        // Rigidbody
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb == null) rb = player.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.mass = 1.0f;
        rb.useGravity = false;

        // PlayerController  ← añadir AL FINAL una vez el GO está configurado
        player.AddComponent<PlayerController>();

        return player;
    }

    void FixModelMaterials(GameObject model)
    {
        Renderer[] rs = model.GetComponentsInChildren<Renderer>();
        foreach (var r in rs)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            Material mat = new Material(sh);
            mat.color = ninjaColor;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", ninjaColor);
            mat.SetFloat("_Smoothness", 0.4f);
            r.material = mat;
        }
    }

    void SetupPlayerAnimator(GameObject player)
    {
        Animator anim = player.GetComponent<Animator>();
        if (anim == null) anim = player.AddComponent<Animator>();

#if UNITY_EDITOR
        string controllerPath = "Assets/Animation/CharacterAnimator.controller";

        // Cargar el controller que el AnimatorControllerBuilder crea/actualiza al compilar
        RuntimeAnimatorController existing =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);

        if (existing == null)
        {
            // Fallback: buscar cualquier AnimatorController en el proyecto
            string[] guids = AssetDatabase.FindAssets("t:AnimatorController");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                existing = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
                if (existing != null) break;
            }
        }

        if (existing != null)
        {
            anim.runtimeAnimatorController = existing;
            Debug.Log("<color=green>[GameSetup]</color> AnimatorController asignado: " + existing.name);
        }
        else
        {
            Debug.LogWarning("[GameSetup] No se encontró ningún AnimatorController. Usa Tools > Rebuild Character Animator.");
        }
#else
        // En runtime (Build), intentar cargar desde Resources
        var controller = Resources.Load<RuntimeAnimatorController>("Animation/CharacterAnimator");
        if (controller != null)
        {
            anim.runtimeAnimatorController = controller;
            Debug.Log("<color=green>[GameSetup]</color> AnimatorController asignado desde Resources: " + controller.name);
        }
        else
        {
            Debug.LogWarning("[GameSetup] No se encontró CharacterAnimator en Resources/Animation/. Asegúrate de copiar el controller a una carpeta Resources.");
        }
#endif
    }



    GameObject CreateProceduralNinja()
    {
        GameObject n    = new GameObject("Player");
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(n.transform);
        body.transform.localPosition = new Vector3(0, 1, 0);
        // Quitar el collider del hijo (el collider real está en el padre)
        Collider childCol = body.GetComponent<Collider>();
        if (childCol != null) DestroyImmediate(childCol);
        SetColor(body, ninjaColor);
        n.transform.position = new Vector3(0f, 1.5f, 0f);
        return n;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS DE NIVEL
    // ─────────────────────────────────────────────────────────────────────────
    GameObject CreatePlatform(Vector3 pos, Vector3 scale, Color color)
    {
        GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
        p.transform.position = pos;
        p.transform.localScale = scale;
        SetColor(p, color);
        return p;
    }

    void CreateCoin(Vector3 pos)
    {
        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        c.name = "Coin";
        c.transform.position = pos;
        c.transform.localScale = new Vector3(0.6f, 0.05f, 0.6f);
        c.transform.rotation = Quaternion.Euler(90, 0, 0);
        SetColor(c, Color.yellow);
        c.GetComponent<Collider>().isTrigger = true;
        c.AddComponent<CoinTag>();
        c.AddComponent<Coin>();
    }

    void CreateCheckpoint(Vector3 pos)
    {
        GameObject cp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cp.name = "Checkpoint";
        cp.transform.position = pos;
        cp.transform.localScale = new Vector3(1.5f, 0.12f, 1.5f);
        SetColor(cp, new Color(0.4f, 0.4f, 0.5f));
        cp.GetComponent<Collider>().isTrigger = true;
        cp.AddComponent<Checkpoint>();
    }

    void SetColor(GameObject obj, Color c)
    {
        Renderer r = obj.GetComponent<Renderer>();
        if (!r) return;
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        Material m = new Material(sh);
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.3f);
        r.material = m;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UI
    // ─────────────────────────────────────────────────────────────────────────
    void CreateUI()
    {
        // Canvas
        GameObject cObj = new GameObject("Canvas");
        Canvas canvas = cObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = cObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        cObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // UIManager en un GO separado para que Awake/Start tengan el orden correcto
        GameObject uiManagerObj = new GameObject("UIManager");
        UIManager ui = uiManagerObj.AddComponent<UIManager>();

        // Panel HUD
        GameObject panel = new GameObject("HUD");
        panel.transform.SetParent(cObj.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Fuente
        Font fnt = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fnt == null) fnt = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // --- VIDAS ---
        ui.livesText = CreateText("Lives", panel.transform, fnt,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(20, -20), new Vector2(250, 50),
            24, Color.red, TextAnchor.UpperLeft);

        // --- MONEDAS ---
        ui.coinsText = CreateText("Coins", panel.transform, fnt,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-20, -20), new Vector2(250, 50),
            24, Color.yellow, TextAnchor.UpperRight);

        // --- SHIELD ---
        ui.shieldText = CreateText("Shield", panel.transform, fnt,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -20), new Vector2(300, 50),
            22, new Color(0.5f, 0.8f, 1f), TextAnchor.UpperCenter);

        // --- MENSAJE (Game Over / Win) ---
        GameObject msgObj = new GameObject("Message");
        msgObj.transform.SetParent(panel.transform, false);
        Text tMsg = msgObj.AddComponent<Text>();
        if (fnt != null) tMsg.font = fnt;
        tMsg.fontSize = 54;
        tMsg.fontStyle = FontStyle.Bold;
        tMsg.alignment = TextAnchor.MiddleCenter;
        tMsg.color = Color.white;
        RectTransform rMsg = msgObj.GetComponent<RectTransform>();
        rMsg.anchorMin = new Vector2(0.5f, 0.5f);
        rMsg.anchorMax = new Vector2(0.5f, 0.5f);
        rMsg.pivot     = new Vector2(0.5f, 0.5f);
        rMsg.sizeDelta = new Vector2(700, 120);
        rMsg.anchoredPosition = Vector2.zero;
        ui.messageText = tMsg;
        msgObj.SetActive(false);

        // --- BOTÓN REINICIAR ---
        GameObject btnObj = new GameObject("RestartButton");
        btnObj.transform.SetParent(panel.transform, false);
        UnityEngine.UI.Image bImg = btnObj.AddComponent<UnityEngine.UI.Image>();
        bImg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        UnityEngine.UI.Button bBtn = btnObj.AddComponent<UnityEngine.UI.Button>();
        bBtn.targetGraphic = bImg;
        RectTransform rBtn = btnObj.GetComponent<RectTransform>();
        rBtn.anchorMin = new Vector2(0.5f, 0.5f);
        rBtn.anchorMax = new Vector2(0.5f, 0.5f);
        rBtn.pivot     = new Vector2(0.5f, 0.5f);
        rBtn.sizeDelta = new Vector2(200, 60);
        rBtn.anchoredPosition = new Vector2(0, -80);

        GameObject btnTxt = new GameObject("Text");
        btnTxt.transform.SetParent(btnObj.transform, false);
        Text bt = btnTxt.AddComponent<Text>();
        if (fnt != null) bt.font = fnt;
        bt.text = "REINICIAR";
        bt.alignment = TextAnchor.MiddleCenter;
        bt.color = Color.white;
        bt.fontSize = 26;
        bt.fontStyle = FontStyle.Bold;
        RectTransform rBt = btnTxt.GetComponent<RectTransform>();
        rBt.anchorMin = Vector2.zero;
        rBt.anchorMax = Vector2.one;
        rBt.offsetMin = Vector2.zero;
        rBt.offsetMax = Vector2.zero;

        ui.restartButton = btnObj;
        btnObj.SetActive(false);

        // Event System
        GameObject esObj = GameObject.Find("EventSystem");
        if (esObj == null) esObj = new GameObject("EventSystem");

        if (esObj.GetComponent<UnityEngine.EventSystems.EventSystem>() == null)
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();

#if UNITY_INPUT_SYSTEM || ENABLE_INPUT_SYSTEM
        var oldModule = esObj.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        if (oldModule != null) DestroyImmediate(oldModule);
        if (esObj.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        if (esObj.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>() == null)
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
    }

    /// <summary>Crea un Text UI con anclas y posición dadas.</summary>
    Text CreateText(string goName, Transform parent, Font fnt,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta,
        int fontSize, Color color, TextAnchor alignment)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        if (fnt != null) t.font = fnt;
        t.fontSize   = fontSize;
        t.color      = color;
        t.alignment  = alignment;
        t.fontStyle  = FontStyle.Bold;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin       = anchorMin;
        rt.anchorMax       = anchorMax;
        rt.pivot           = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta       = sizeDelta;
        return t;
    }
}
