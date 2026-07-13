using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public static class GoalRushSetupWizard
{
    [MenuItem("GoalRush/Setup Complete Scene")]
    static void SetupScene()
    {
        CleanupOldObjects();
        SetupMainCamera();
        SetupGlobalVolume();
        CreateCanvasAndUI();
        CreateManagers();
        CreateFloatingCanvas();
        CreateTargetPrefabs();
        WireAllReferences();

        Debug.Log("<color=green>Goal-Rush Precision scene setup complete!</color>");
    }

    [MenuItem("GoalRush/Wire All References")]
    static void WireOnly()
    {
        WireAllReferences();
    }

    [MenuItem("GoalRush/Clear Old & Rebuild")]
    static void ClearAndRebuild()
    {
        CleanupOldObjects();
        // Delete old prefabs
        string prefabDir = "Assets/FotballGame/FotballEvent/Prefabs";
        string[] prefabs = { "GoldTarget.prefab", "PenaltyTarget.prefab", "FloatingText.prefab" };
        foreach (var p in prefabs)
        {
            string path = prefabDir + "/" + p;
            if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
        }
        SetupScene();
    }

    #region Cleanup

    static void CleanupOldObjects()
    {
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        var toDelete = new List<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj != null && (
                obj.name.StartsWith("GoalRush_") ||
                obj.name == "MainCanvas_GoalRush" ||
                obj.name == "FloatingCanvas_GoalRush"))
            {
                toDelete.Add(obj);
            }
        }
        foreach (var obj in toDelete)
        {
            if (obj != null) Object.DestroyImmediate(obj);
        }

        // Remove old GameManager by iterating all GameObjects
        foreach (var obj in allObjects)
        {
            if (obj == null) continue;
            var comps = obj.GetComponents<MonoBehaviour>();
            foreach (var comp in comps)
            {
                if (comp != null && comp.GetType().Name == "GameManager" &&
                    comp.GetType().Namespace == null)
                {
                    Debug.Log($"Removing old GameManager component from '{obj.name}'");
                    Object.DestroyImmediate(comp);
                }
            }
        }
    }

    #endregion

    #region Camera

    static void SetupMainCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.102f, 0.204f, 0.090f);
        cam.orthographic = true;
        cam.orthographicSize = 540f;

        if (cam.transform.position == Vector3.zero)
            cam.transform.position = new Vector3(0, 0, -10);
    }

    static void SetupGlobalVolume()
    {
        Volume volume = Object.FindFirstObjectByType<Volume>();
        if (volume == null)
        {
            GameObject volObj = new GameObject("Global Volume");
            volume = volObj.AddComponent<Volume>();
        }

        volume.isGlobal = true;
        volume.sharedProfile = CreateBloomProfile();
    }

    static VolumeProfile CreateBloomProfile()
    {
        string dir = "Assets/FotballGame/FotballEvent/Profiles";
        string path = dir + "/GoalRush_BloomProfile.asset";
        Directory.CreateDirectory(dir);

        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
        }

        if (!profile.TryGet<Bloom>(out var bloom))
        {
            bloom = profile.Add<Bloom>(true);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.5f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 1.5f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.7f;
            bloom.tint.overrideState = true;
            bloom.tint.value = Color.white;
        }

        if (!profile.TryGet<Tonemapping>(out var tone))
        {
            tone = profile.Add<Tonemapping>(true);
            tone.mode.overrideState = true;
            tone.mode.value = TonemappingMode.Neutral;
        }

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        return profile;
    }

    #endregion

    #region Main Canvas

    static GameObject _mainCanvasGO;

    static void CreateCanvasAndUI()
    {
        _mainCanvasGO = new GameObject("MainCanvas_GoalRush",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _mainCanvasGO.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = _mainCanvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = _mainCanvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // EventSystem
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        Transform root = _mainCanvasGO.transform;

        CreateBackground(root);
        CreateGoalFrame(root);
        CreateRedFlash(root);
        CreateGreenFlash(root);
        CreateClusterParent(root);
        CreateLoadingOverlay(root);
        CreateMenuContainer(root);
        CreateHUDContainer(root);
        CreateCountdownContainer(root);
        CreateGameOverContainer(root);
        CreatePauseOverlay(root);
        CreateNotificationContainer(root);
        CreateLevelUpContainer(root);
    }

    static void CreateBackground(Transform parent)
    {
        GameObject bg = CreateImageFull("Background", parent, new Color(0.102f, 0.204f, 0.090f));

        GameObject grass = CreateImageFull("GrassStripes", parent, new Color(0, 0, 0, 0.03f));

        Texture2D stripeTex = new Texture2D(4, 4);
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                stripeTex.SetPixel(x, y, y % 2 == 0 ? Color.white : Color.clear);
        stripeTex.Apply();
        stripeTex.wrapMode = TextureWrapMode.Repeat;

        string texPath = "Assets/FotballGame/FotballEvent/Textures/StripePattern.asset";
        Directory.CreateDirectory("Assets/FotballGame/FotballEvent/Textures");
        AssetDatabase.CreateAsset(stripeTex, texPath);

        Image gImg = grass.GetComponent<Image>();
        gImg.sprite = Sprite.Create(stripeTex, new Rect(0, 0, 4, 4), Vector2.zero, 1);
        gImg.type = Image.Type.Tiled;
    }

    static GameObject CreateImageFull(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return go;
    }

    static void CreateGoalFrame(Transform parent)
    {
        GameObject goalContainer = new GameObject("GoalFrame", typeof(RectTransform));
        goalContainer.transform.SetParent(parent, false);
        RectTransform gRoot = goalContainer.GetComponent<RectTransform>();
        gRoot.anchorMin = new Vector2(0.15f, 0.2f);
        gRoot.anchorMax = new Vector2(0.85f, 0.8f);
        gRoot.sizeDelta = Vector2.zero;
        gRoot.anchoredPosition = Vector2.zero;

        CreateGoalBar("TopBar", gRoot, new Vector2(0, 1), new Vector2(1, 1), true);
        CreateGoalBar("LeftBar", gRoot, new Vector2(0, 0), new Vector2(0, 1), false);
        CreateGoalBar("RightBar", gRoot, new Vector2(1, 0), new Vector2(1, 1), false);
    }

    static void CreateGoalBar(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, bool isTop)
    {
        float thickness = 15f;
        GameObject bar = new GameObject(name, typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(parent, false);
        RectTransform r = bar.GetComponent<RectTransform>();
        r.anchorMin = anchorMin;
        r.anchorMax = anchorMax;

        if (isTop)
        {
            r.sizeDelta = new Vector2(0, thickness);
            r.anchoredPosition = new Vector2(0, -thickness * 0.5f);
        }
        else
        {
            r.sizeDelta = new Vector2(thickness, 0);
            float sign = name == "LeftBar" ? 1 : -1;
            r.anchoredPosition = new Vector2(sign * thickness * 0.5f, 0);
        }

        bar.GetComponent<Image>().color = Color.white;

        Shadow shadow = bar.AddComponent<Shadow>();
        shadow.effectColor = new Color(1, 1, 1, 0.3f);
        shadow.effectDistance = Vector2.zero;
    }

    static void CreateRedFlash(Transform parent)
    {
        GameObject flash = CreateImageFull("RedFlashOverlay", parent, new Color(1, 0, 0, 0));
        flash.GetComponent<Image>().raycastTarget = false;
    }

    static void CreateGreenFlash(Transform parent)
    {
        GameObject flash = CreateImageFull("GreenFlashOverlay", parent, new Color(0, 1, 0, 0));
        flash.GetComponent<Image>().raycastTarget = false;
    }

    static void CreateClusterParent(Transform parent)
    {
        GameObject cluster = new GameObject("ClusterParent", typeof(RectTransform));
        cluster.transform.SetParent(parent, false);
        RectTransform r = cluster.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(200, 200);
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = Vector2.zero;
        cluster.SetActive(false);
    }

    static void CreateLoadingOverlay(Transform parent)
    {
        GameObject container = new GameObject("LoadingContainer", typeof(RectTransform));
        container.transform.SetParent(parent, false);
        RectTransform cr = container.GetComponent<RectTransform>();
        cr.anchorMin = Vector2.zero;
        cr.anchorMax = Vector2.one;
        cr.sizeDelta = Vector2.zero;

        GameObject bg = CreateImageFull("LoadingBg", container.transform, new Color(0, 0, 0, 0.85f));
        bg.GetComponent<Image>().raycastTarget = false;

        GameObject text = CreateUIText("LoadingText", container.transform,
            "در حال بارگذاری...", 48, Vector2.zero, new Vector2(400, 80));
        text.GetComponent<TextMeshProUGUI>().color = new Color(1, 0.922f, 0.231f);

        container.SetActive(false);
    }

    static void CreateMenuContainer(Transform parent)
    {
        GameObject menu = new GameObject("MenuContainer", typeof(RectTransform));
        menu.transform.SetParent(parent, false);
        RectTransform r = menu.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;

        CreateImageFull("OverlayBg", menu.transform, new Color(0, 0, 0, 0.7f));

        CreateUIText("TitleText", menu.transform, "PRECISION STRIKER", 72,
            new Vector2(0, 100), new Vector2(800, 100));

        GameObject btn = CreateButton("StartButton", menu.transform,
            "شروع بازی", 36, new Vector2(0, -40), new Vector2(300, 70),
            new Color(0.298f, 0.686f, 0.314f));

        btn.AddComponent<Shadow>();

        CreateUIText("HighScoreText", menu.transform, "رکورد: 0", 22,
            new Vector2(0, -110), new Vector2(300, 40));
    }

    static void CreateHUDContainer(Transform parent)
    {
        GameObject hud = new GameObject("HUDContainer", typeof(RectTransform));
        hud.transform.SetParent(parent, false);
        RectTransform hr = hud.GetComponent<RectTransform>();
        hr.anchorMin = Vector2.zero;
        hr.anchorMax = Vector2.one;
        hr.sizeDelta = Vector2.zero;

        GameObject scorePanel = CreateGlassPanel("ScorePanel", hud.transform,
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(40, -40), new Vector2(220, 70));
        CreateUIText("ScoreText", scorePanel.transform, "امتیاز: 0", 28,
            Vector2.zero, new Vector2(200, 50));

        GameObject hitsText = CreateUIText("HitsText", hud.transform, "تعداد گل: 0", 22,
            new Vector2(40, -100), new Vector2(200, 40));
        RectTransform hitsRt = hitsText.GetComponent<RectTransform>();
        hitsRt.anchorMin = hitsRt.anchorMax = new Vector2(0, 1);
        hitsRt.pivot = new Vector2(0, 1);

        GameObject timerPanel = CreateGlassPanel("TimerPanel", hud.transform,
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-40, -40), new Vector2(160, 70));
        CreateUIText("TimerText", timerPanel.transform, "120 ثانیه", 28,
            Vector2.zero, new Vector2(140, 50));

        GameObject comboText = CreateUIText("ComboText", hud.transform, "", 22,
            new Vector2(-40, -100), new Vector2(200, 40));
        RectTransform cr = comboText.GetComponent<RectTransform>();
        cr.anchorMin = cr.anchorMax = new Vector2(1, 1);
        cr.pivot = new Vector2(1, 1);

        GameObject diffPanel = CreateGlassPanel("DifficultyPanel", hud.transform,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -40), new Vector2(160, 50));
        CreateUIText("DifficultyText", diffPanel.transform, "سطح 0", 18,
            Vector2.zero, new Vector2(140, 36));

        GameObject pauseBtn = CreateButton("PauseButton", hud.transform,
            "II", 24, new Vector2(0, 0), new Vector2(50, 50),
            new Color(1, 1, 1, 0.3f));
        RectTransform pbr = pauseBtn.GetComponent<RectTransform>();
        pbr.anchorMin = new Vector2(1, 1);
        pbr.anchorMax = new Vector2(1, 1);
        pbr.anchoredPosition = new Vector2(-60, -60);
        pbr.sizeDelta = new Vector2(50, 50);

        hud.SetActive(false);
    }

    static GameObject CreateGlassPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform r = panel.GetComponent<RectTransform>();
        r.anchorMin = anchorMin;
        r.anchorMax = anchorMax;
        r.anchoredPosition = pos;
        r.sizeDelta = size;

        Image img = panel.GetComponent<Image>();
        img.color = new Color(1, 1, 1, 0.12f);
        img.raycastTarget = false;

        Shadow shadow = panel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.3f);
        shadow.effectDistance = new Vector2(0, 4);
        return panel;
    }

    static void CreateCountdownContainer(Transform parent)
    {
        GameObject container = new GameObject("CountdownContainer", typeof(RectTransform));
        container.transform.SetParent(parent, false);
        RectTransform r = container.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;

        CreateUIText("CountdownText", container.transform, "3", 180,
            Vector2.zero, new Vector2(400, 200));

        container.SetActive(false);
    }

    static void CreateGameOverContainer(Transform parent)
    {
        GameObject container = new GameObject("GameOverContainer", typeof(RectTransform));
        container.transform.SetParent(parent, false);
        RectTransform r = container.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;

        CreateImageFull("OverlayBg", container.transform, new Color(0, 0, 0, 0.7f));

        GameObject title = CreateUIText("GameOverTitle", container.transform,
            "پایان بازی", 72, new Vector2(0, 140), new Vector2(600, 100));
        title.GetComponent<TextMeshProUGUI>().color = new Color(0.957f, 0.263f, 0.212f);

        CreateUIText("ScoreLabel", container.transform,
            "امتیاز نهایی", 28, new Vector2(0, 60), new Vector2(400, 50));

        CreateUIText("FinalScoreText", container.transform,
            "0", 80, new Vector2(0, -10), new Vector2(400, 80));

        CreateUIText("HighScoreText", container.transform,
            "رکورد: 0", 22, new Vector2(0, -70), new Vector2(300, 40));

        GameObject newHighScore = CreateUIText("NewHighScoreText", container.transform,
            "رکورد جدید!", 28, new Vector2(0, -105), new Vector2(400, 40));
        newHighScore.GetComponent<TextMeshProUGUI>().color = new Color(1, 0.84f, 0);
        newHighScore.SetActive(false);

        CreateUIText("AccuracyText", container.transform,
            "دقت: 0%", 20, new Vector2(0, -145), new Vector2(300, 30));

        CreateUIText("GoldHitsText", container.transform,
            "ضربات: 0 / 0", 20, new Vector2(0, -170), new Vector2(300, 30));

        CreateUIText("ComboText", container.transform,
            "بهترین ترکیب: 0", 20, new Vector2(0, -195), new Vector2(300, 30));

        CreateButton("RestartButton", container.transform,
            "دوباره", 32, new Vector2(0, -130), new Vector2(280, 65),
            new Color(0.298f, 0.686f, 0.314f));

        container.SetActive(false);
    }

    static void CreatePauseOverlay(Transform parent)
    {
        GameObject overlay = new GameObject("PauseOverlay", typeof(RectTransform));
        overlay.transform.SetParent(parent, false);
        RectTransform r = overlay.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;

        CreateImageFull("PauseBg", overlay.transform, new Color(0, 0, 0, 0.7f));
        CreateUIText("PauseText", overlay.transform, "مکث", 72,
            Vector2.zero, new Vector2(400, 100)).GetComponent<TextMeshProUGUI>().color = Color.white;
        overlay.SetActive(false);
    }

    static void CreateNotificationContainer(Transform parent)
    {
        GameObject container = new GameObject("NotificationContainer", typeof(RectTransform));
        container.transform.SetParent(parent, false);
        RectTransform r = container.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = new Vector2(0, 200);
        r.sizeDelta = new Vector2(400, 80);

        CreateUIText("NotificationText", container.transform, "", 36,
            Vector2.zero, new Vector2(400, 80));
        container.SetActive(false);
    }

    static void CreateLevelUpContainer(Transform parent)
    {
        GameObject container = new GameObject("LevelUpContainer", typeof(RectTransform));
        container.transform.SetParent(parent, false);
        RectTransform r = container.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = new Vector2(0, -100);
        r.sizeDelta = new Vector2(400, 80);

        GameObject txt = CreateUIText("LevelUpText", container.transform, "سطح ۱!", 48,
            Vector2.zero, new Vector2(400, 80));
        txt.GetComponent<TextMeshProUGUI>().color = new Color(1, 0.84f, 0);
        container.SetActive(false);
    }

    static GameObject CreateUIText(string name, Transform parent, string text,
        float fontSize, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = size;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.isRightToLeftText = true;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        return go;
    }

    static GameObject CreateButton(string name, Transform parent, string label,
        float fontSize, Vector2 pos, Vector2 size, Color color)
    {
        GameObject btn = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btn.transform.SetParent(parent, false);
        RectTransform br = btn.GetComponent<RectTransform>();
        br.anchorMin = br.anchorMax = new Vector2(0.5f, 0.5f);
        br.anchoredPosition = pos;
        br.sizeDelta = size;

        Image img = btn.GetComponent<Image>();
        img.color = color;
        img.type = Image.Type.Sliced;

        Button btnComp = btn.GetComponent<Button>();
        btnComp.transition = Selectable.Transition.ColorTint;
        ColorBlock cb = btnComp.colors;
        cb.highlightedColor = color * 1.2f;
        cb.pressedColor = color * 0.8f;
        btnComp.colors = cb;

        GameObject txt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txt.transform.SetParent(btn.transform, false);
        RectTransform tr = txt.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.isRightToLeftText = true;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        return btn;
    }

    #endregion

    #region Floating Canvas

    static void CreateFloatingCanvas()
    {
        GameObject canvasGO = new GameObject("FloatingCanvas_GoalRush",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.layer = LayerMask.NameToLayer("UI");
        canvasGO.transform.SetAsLastSibling();

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    #endregion

    #region Managers

    static void CreateManagers()
    {
        var gmObj = new GameObject("GoalRush_GameManager");
        gmObj.AddComponent<GoalRush.GameManager>();

        var audioObj = new GameObject("GoalRush_AudioManager");
        audioObj.AddComponent<GoalRush.AudioManager>();
        var sfxSrc = audioObj.AddComponent<AudioSource>();
        sfxSrc.playOnAwake = false;
        var musicSrc = audioObj.AddComponent<AudioSource>();
        musicSrc.playOnAwake = false;
        musicSrc.loop = true;

        var uiObj = new GameObject("GoalRush_UIManager");
        uiObj.AddComponent<GoalRush.UIManager>();

        var spawnerObj = new GameObject("GoalRush_TargetSpawner");
        spawnerObj.AddComponent<GoalRush.TargetSpawner>();
    }

    #endregion

    #region Prefabs

    static void CreateTargetPrefabs()
    {
        string dir = "Assets/FotballGame/FotballEvent/Prefabs";
        Directory.CreateDirectory(dir);

        // Always delete old prefabs to ensure fresh creation
        string[] prefabFiles = { "/GoldTarget.prefab", "/PenaltyTarget.prefab", "/FloatingText.prefab",
            "/GoldHitParticles.prefab", "/PenaltyHitParticles.prefab", "/RingEffect.prefab" };
        foreach (var f in prefabFiles)
        {
            string p = dir + f;
            if (File.Exists(p)) AssetDatabase.DeleteAsset(p);
        }
        AssetDatabase.Refresh();

        CreateGoldPrefab(dir);
        CreatePenaltyPrefab(dir);
        CreateFloatingTextPrefab(dir);
        CreateGoldHitParticles(dir);
        CreatePenaltyHitParticles(dir);
        CreateRingEffectPrefab(dir);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Verify prefabs were created
        string[] checkFiles = { "/GoldTarget.prefab", "/PenaltyTarget.prefab", "/FloatingText.prefab",
            "/GoldHitParticles.prefab", "/PenaltyHitParticles.prefab", "/RingEffect.prefab" };
        foreach (var f in checkFiles)
        {
            string p = dir + f;
            if (File.Exists(p))
                Debug.Log($"<color=green>✓ Prefab created: {p}</color>");
            else
                Debug.LogError($"✗ Prefab NOT created: {p}");
        }
    }

    static void CreateGoldPrefab(string dir)
    {
        string path = dir + "/GoldTarget.prefab";

        GameObject go = new GameObject("GoldTarget",
            typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        go.AddComponent<GoalRush.TargetInteraction>();

        RectTransform r = go.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(110, 110);

        Image img = go.GetComponent<Image>();
        img.color = new Color(1, 0.922f, 0.231f);
        img.raycastTarget = true;

        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(1, 0.922f, 0.231f, 0.8f);
        shadow.effectDistance = Vector2.zero;

        var outline = go.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(2, -2);

        GameObject label = CreateChildText(go.transform, "ScoreLabel",
            "+25", 22, new Color(0.2f, 0.2f, 0.2f), new Vector2(60, 40));

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    static void CreatePenaltyPrefab(string dir)
    {
        string path = dir + "/PenaltyTarget.prefab";

        GameObject go = new GameObject("PenaltyTarget",
            typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        go.AddComponent<GoalRush.TargetInteraction>();

        RectTransform r = go.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(75, 75);

        Image img = go.GetComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = true;

        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0.957f, 0.263f, 0.212f);
        outline.effectDistance = new Vector2(3, -3);

        GameObject label = CreateChildText(go.transform, "ValueLabel",
            "-15", 16, new Color(0.957f, 0.263f, 0.212f), new Vector2(40, 30));

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    static void CreateFloatingTextPrefab(string dir)
    {
        string path = dir + "/FloatingText.prefab";

        GameObject go = new GameObject("FloatingText",
            typeof(RectTransform), typeof(TextMeshProUGUI), typeof(CanvasGroup));
        RectTransform r = go.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(200, 50);

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = "+25";
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.isRightToLeftText = true;
        tmp.color = Color.green;
        tmp.fontStyle = FontStyles.Bold;

        go.GetComponent<CanvasGroup>().alpha = 1f;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    static void CreateGoldHitParticles(string dir)
    {
        string path = dir + "/GoldHitParticles.prefab";
        GameObject go = new GameObject("GoldHitParticles", typeof(RectTransform), typeof(ParticleSystem));
        var ps = go.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(1, 0.922f, 0.231f);
        main.startSize = 5f;
        main.startLifetime = 0.5f;
        main.maxParticles = 20;
        main.duration = 0.3f;
        main.loop = false;

        var emit = ps.emission;
        emit.rateOverTime = 0;
        emit.SetBurst(0, new ParticleSystem.Burst(0f, 15));

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 30f;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    static void CreatePenaltyHitParticles(string dir)
    {
        string path = dir + "/PenaltyHitParticles.prefab";
        GameObject go = new GameObject("PenaltyHitParticles", typeof(RectTransform), typeof(ParticleSystem));
        var ps = go.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(0.957f, 0.263f, 0.212f);
        main.startSize = 4f;
        main.startLifetime = 0.4f;
        main.maxParticles = 15;
        main.duration = 0.2f;
        main.loop = false;

        var emit = ps.emission;
        emit.rateOverTime = 0;
        emit.SetBurst(0, new ParticleSystem.Burst(0f, 10));

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 20f;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    static void CreateRingEffectPrefab(string dir)
    {
        string path = dir + "/RingEffect.prefab";

        GameObject go = new GameObject("RingEffect", typeof(RectTransform), typeof(Image));
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(60, 60);

        Image img = go.GetComponent<Image>();
        img.color = new Color(1, 1, 1, 0.3f);
        img.raycastTarget = false;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    static GameObject CreateChildText(Transform parent, string name,
        string text, float fontSize, Color color, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = size;
        r.anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.isRightToLeftText = true;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
        return go;
    }

    #endregion

    #region Wiring

    static void WireAllReferences()
    {
        AssetDatabase.Refresh();

        var gm = Object.FindFirstObjectByType<GoalRush.GameManager>();
        var ui = Object.FindFirstObjectByType<GoalRush.UIManager>();
        var spawner = Object.FindFirstObjectByType<GoalRush.TargetSpawner>();
        var audio = Object.FindFirstObjectByType<GoalRush.AudioManager>();

        if (gm == null) Debug.LogError("GameManager not found in scene!");
        if (ui == null) Debug.LogError("UIManager not found in scene!");
        if (spawner == null) Debug.LogError("TargetSpawner not found in scene!");

        if (gm == null || ui == null || spawner == null) return;

        var mainCanvas = GameObject.Find("MainCanvas_GoalRush");
        var floatingCanvas = GameObject.Find("FloatingCanvas_GoalRush");

        if (mainCanvas == null) { Debug.LogError("MainCanvas_GoalRush not found!"); return; }
        if (floatingCanvas == null) Debug.LogWarning("FloatingCanvas_GoalRush not found");

        Debug.Log($"Found managers. Wiring references...");

        // Wire UIManager
        var so_ui = new SerializedObject(ui);

        LogWire(so_ui, "_hudContainer", FindChildRecursive(mainCanvas.transform, "HUDContainer"));
        LogWire(so_ui, "_menuContainer", FindChildRecursive(mainCanvas.transform, "MenuContainer"));
        LogWire(so_ui, "_countdownContainer", FindChildRecursive(mainCanvas.transform, "CountdownContainer"));
        LogWire(so_ui, "_gameOverContainer", FindChildRecursive(mainCanvas.transform, "GameOverContainer"));

        var hud = FindChildRecursive(mainCanvas.transform, "HUDContainer");
        if (hud != null)
        {
            LogWire(so_ui, "_scoreText", FindChildRecursive(hud.transform, "ScoreText")?.GetComponent<TextMeshProUGUI>());
            LogWire(so_ui, "_hudHitsText", FindChildRecursive(hud.transform, "HitsText")?.GetComponent<TextMeshProUGUI>());
            LogWire(so_ui, "_timerText", FindChildRecursive(hud.transform, "TimerText")?.GetComponent<TextMeshProUGUI>());
            LogWire(so_ui, "_comboText", FindChildRecursive(hud.transform, "ComboText")?.GetComponent<TextMeshProUGUI>());
            LogWire(so_ui, "_difficultyText", FindChildRecursive(hud.transform, "DifficultyText")?.GetComponent<TextMeshProUGUI>());

            var pauseBtn = FindChildRecursive(hud.transform, "PauseButton")?.GetComponent<Button>();
            if (pauseBtn != null) LogWire(so_ui, "_pauseButton", pauseBtn);
        }
        else Debug.LogWarning("HUDContainer not found!");

        var countdown = FindChildRecursive(mainCanvas.transform, "CountdownContainer");
        if (countdown != null)
            LogWire(so_ui, "_countdownText", FindChildRecursive(countdown.transform, "CountdownText")?.GetComponent<TextMeshProUGUI>());

        var gameOver = FindChildRecursive(mainCanvas.transform, "GameOverContainer");
        if (gameOver != null)
        {
            LogWire(so_ui, "_finalScoreText", FindChildRecursive(gameOver.transform, "FinalScoreText")?.GetComponent<TextMeshProUGUI>());
            LogWire(so_ui, "_highScoreText", FindChildRecursive(gameOver.transform, "HighScoreText")?.GetComponent<TextMeshProUGUI>());
            LogWire(so_ui, "_newHighScoreText", FindChildRecursive(gameOver.transform, "NewHighScoreText")?.GetComponent<TextMeshProUGUI>());
            LogWire(so_ui, "_accuracyText", FindChildRecursive(gameOver.transform, "AccuracyText")?.GetComponent<TextMeshProUGUI>());
            LogWire(so_ui, "_gameOverComboText", FindChildRecursive(gameOver.transform, "ComboText")?.GetComponent<TextMeshProUGUI>());
            LogWire(so_ui, "_gameOverGoldHitsText", FindChildRecursive(gameOver.transform, "GoldHitsText")?.GetComponent<TextMeshProUGUI>());
        }

        var menu = FindChildRecursive(mainCanvas.transform, "MenuContainer");
        if (menu != null)
        {
            LogWire(so_ui, "_startButton", FindChildRecursive(menu.transform, "StartButton")?.GetComponent<Button>());
            LogWire(so_ui, "_menuHighScoreText", FindChildRecursive(menu.transform, "HighScoreText")?.GetComponent<TextMeshProUGUI>());
        }
        if (gameOver != null)
            LogWire(so_ui, "_restartButton", FindChildRecursive(gameOver.transform, "RestartButton")?.GetComponent<Button>());

        LogWire(so_ui, "_redFlashImage", FindChildRecursive(mainCanvas.transform, "RedFlashOverlay")?.GetComponent<Image>());
        LogWire(so_ui, "_greenFlashImage", FindChildRecursive(mainCanvas.transform, "GreenFlashOverlay")?.GetComponent<Image>());
        LogWire(so_ui, "_pauseOverlay", FindChildRecursive(mainCanvas.transform, "PauseOverlay"));
        LogWire(so_ui, "_notificationContainer", FindChildRecursive(mainCanvas.transform, "NotificationContainer"));
        LogWire(so_ui, "_notificationText", FindChildRecursive(mainCanvas.transform, "NotificationText")?.GetComponent<TextMeshProUGUI>());
        LogWire(so_ui, "_levelUpContainer", FindChildRecursive(mainCanvas.transform, "LevelUpContainer"));
        LogWire(so_ui, "_levelUpText", FindChildRecursive(mainCanvas.transform, "LevelUpText")?.GetComponent<TextMeshProUGUI>());
        LogWire(so_ui, "_mainCamera", Camera.main);
        LogWire(so_ui, "_floatingCanvas", floatingCanvas?.GetComponent<Canvas>());

        string ftpPath = "Assets/FotballGame/FotballEvent/Prefabs/FloatingText.prefab";
        var ftpPrefab = AssetDatabase.LoadAssetAtPath<TextMeshProUGUI>(ftpPath);
        if (ftpPrefab != null)
            LogWire(so_ui, "_floatingTextPrefab", ftpPrefab);
        else
            Debug.LogWarning("FloatingText prefab not found at " + ftpPath);

        string celebrationParticlesPath = "Assets/FotballGame/FotballEvent/Prefabs/GoldHitParticles.prefab";
        var celebrationPsGo = AssetDatabase.LoadAssetAtPath<GameObject>(celebrationParticlesPath);
        if (celebrationPsGo != null)
        {
            var celebrationPs = celebrationPsGo.GetComponent<ParticleSystem>();
            if (celebrationPs != null)
                LogWire(so_ui, "_celebrationParticles", celebrationPs);
        }
        else
            Debug.LogWarning("GoldHitParticles prefab not found for celebration at " + celebrationParticlesPath);

        so_ui.ApplyModifiedProperties();
        Debug.Log("<color=green>UIManager wired.</color>");

        // Wire TargetSpawner
        var so_spawner = new SerializedObject(spawner);
        string goldPath = "Assets/FotballGame/FotballEvent/Prefabs/GoldTarget.prefab";
        string penaltyPath = "Assets/FotballGame/FotballEvent/Prefabs/PenaltyTarget.prefab";

        var goldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(goldPath);
        var penaltyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(penaltyPath);

        if (goldPrefab == null) Debug.LogError("GoldTarget prefab not found at " + goldPath);
        if (penaltyPrefab == null) Debug.LogError("PenaltyTarget prefab not found at " + penaltyPath);

        LogWire(so_spawner, "_goldTargetPrefab", goldPrefab);
        LogWire(so_spawner, "_penaltyTargetPrefab", penaltyPrefab);
        LogWire(so_spawner, "_clusterParent", FindChildRecursive(mainCanvas.transform, "ClusterParent")?.GetComponent<RectTransform>());
        LogWire(so_spawner, "_goalArea", FindChildRecursive(mainCanvas.transform, "GoalFrame")?.GetComponent<RectTransform>());

        string ringPath = "Assets/FotballGame/FotballEvent/Prefabs/RingEffect.prefab";
        var ringPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ringPath);
        if (ringPrefab != null) LogWire(so_spawner, "_ringEffectPrefab", ringPrefab);

        so_spawner.ApplyModifiedProperties();
        Debug.Log("<color=green>TargetSpawner wired.</color>");

        // Wire AudioManager
        if (audio != null)
        {
            var so_audio = new SerializedObject(audio);
            var sources = audio.GetComponents<AudioSource>();
            if (sources.Length > 0) LogWire(so_audio, "_sfxSource", sources[0]);
            if (sources.Length > 1) LogWire(so_audio, "_musicSource", sources[1]);
            so_audio.ApplyModifiedProperties();
            Debug.Log("<color=green>AudioManager wired.</color>");
        }

        // Wire TargetInteraction on prefabs
        string goldHitParticlesPath = "Assets/FotballGame/FotballEvent/Prefabs/GoldHitParticles.prefab";
        string penaltyHitParticlesPath = "Assets/FotballGame/FotballEvent/Prefabs/PenaltyHitParticles.prefab";
        var goldHitPs = AssetDatabase.LoadAssetAtPath<ParticleSystem>(goldHitParticlesPath);
        var penaltyHitPs = AssetDatabase.LoadAssetAtPath<ParticleSystem>(penaltyHitParticlesPath);

        var goldPrefabLoaded = AssetDatabase.LoadAssetAtPath<GameObject>(goldPath);
        if (goldPrefabLoaded != null)
        {
            var so_gold = new SerializedObject(goldPrefabLoaded);
            var goldInteract = goldPrefabLoaded.GetComponent<GoalRush.TargetInteraction>();
            if (goldInteract != null)
            {
                var so_gi = new SerializedObject(goldInteract);
                if (goldHitPs != null) LogWire(so_gi, "_successParticles", goldHitPs);
                so_gi.ApplyModifiedProperties();
            }
        }

        var penaltyPrefabLoaded = AssetDatabase.LoadAssetAtPath<GameObject>(penaltyPath);
        if (penaltyPrefabLoaded != null)
        {
            var penaltyInteract = penaltyPrefabLoaded.GetComponent<GoalRush.TargetInteraction>();
            if (penaltyInteract != null)
            {
                var so_pi = new SerializedObject(penaltyInteract);
                if (penaltyHitPs != null) LogWire(so_pi, "_failParticles", penaltyHitPs);
                so_pi.ApplyModifiedProperties();
            }
        }

        Debug.Log("<color=cyan>=== All references wired successfully! ===</color>");

        Debug.Log("<color=yellow>Note: GameManager has only config values (no scene references needed).</color>");
        Debug.Log("<color=yellow>Note: Assign audio clips and BGM to AudioManager manually in Inspector.</color>");
    }

    static void LogWire(SerializedObject so, string field, Object value)
    {
        var prop = so.FindProperty(field);
        if (prop == null)
        {
            Debug.LogWarning($"Field '{field}' not found on {so.targetObject.name}");
            return;
        }
        prop.objectReferenceValue = value;
        if (value != null)
            Debug.Log($"  ✓ {so.targetObject.name}.{field} → {value.name}");
        else
            Debug.Log($"  ✗ {so.targetObject.name}.{field} → NULL");
    }

    static GameObject FindChildRecursive(Transform parent, string name)
    {
        if (parent == null) return null;
        foreach (Transform child in parent)
        {
            if (child.name == name) return child.gameObject;
            var found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }

    #endregion
}
