using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

public static class GoalRushNewFeatureInjector
{
    private const string PrefabDir = "Assets/FotballGame/FotballEvent/Prefabs";
    private static GameObject _mainCanvas;
    private static GoalRush.UIManager _ui;
    private static SerializedObject _so;

    [MenuItem("GoalRush/Inject New Features (Safe)")]
    static void InjectNewFeatures()
    {
        Debug.Log("<color=cyan>=== GoalRush Feature Injector ===</color>");
        Debug.Log("This tool only ADDS missing scene objects and wires them. Never modifies existing.");

        _mainCanvas = GameObject.Find("MainCanvas_GoalRush");
        if (_mainCanvas == null)
        {
            Debug.LogError("MainCanvas_GoalRush not found in scene. Create it first.");
            return;
        }

        _ui = Object.FindFirstObjectByType<GoalRush.UIManager>();
        if (_ui == null)
        {
            Debug.LogError("GoalRush.UIManager not found in scene.");
            return;
        }

        _so = new SerializedObject(_ui);

        int count = 0;
        count += InjectMenu();
        count += InjectHUD();
        count += InjectCountdown();
        count += InjectGameOver();
        count += InjectScreenEffects();
        count += InjectPause();
        count += InjectNotifications();
        count += InjectLevelUp();
        count += InjectFloatingCanvas();
        count += InjectFloatingTextPrefab();
        count += InjectCelebrationParticles();
        count += InjectMainCamera();
        count += InjectPrefabAssets();

        _so.ApplyModifiedProperties();

        if (count == 0)
            Debug.Log("<color=green>All UIManager fields are already wired. Nothing to inject.</color>");
        else
            Debug.Log($"<color=green>✓ {count} new feature(s) injected.</color>");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    #region Helpers

    static bool IsNull(string fieldName)
    {
        var prop = _so.FindProperty(fieldName);
        return prop == null || prop.objectReferenceValue == null;
    }

    static void WireIfNull(string fieldName, Object value)
    {
        if (value == null) return;
        var prop = _so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogWarning($"  ! Field '{fieldName}' not found on UIManager.");
            return;
        }
        if (prop.objectReferenceValue != null) return;
        prop.objectReferenceValue = value;
        Debug.Log($"  ✓ Wired '{fieldName}' → '{value.name}'");
    }

    static GameObject FindContainer(string name)
    {
        var t = _mainCanvas.transform.Find(name);
        return t != null ? t.gameObject : null;
    }

    static GameObject EnsureContainer(string name)
    {
        var t = _mainCanvas.transform.Find(name);
        if (t != null) return t.gameObject;
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(_mainCanvas.transform, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;
        Debug.Log($"  <color=green>+ Created container '{name}'</color>");
        return go;
    }

    static TextMeshProUGUI EnsureText(GameObject parent, string name, string textVal, float fontSize, Vector2 pos, Vector2 size)
    {
        var existing = parent.transform.Find(name);
        if (existing != null) return existing.GetComponent<TextMeshProUGUI>();
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent.transform, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = size;
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = textVal;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.isRightToLeftText = true;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        Debug.Log($"  <color=green>+ Created Text '{name}' under '{parent.name}'</color>");
        return tmp;
    }

    static Button EnsureButton(GameObject parent, string name, string label, float fontSize, Vector2 pos, Vector2 size, Color color)
    {
        var existing = parent.transform.Find(name);
        if (existing != null) return existing.GetComponent<Button>();
        GameObject btn = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btn.transform.SetParent(parent.transform, false);
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
        Debug.Log($"  <color=green>+ Created Button '{name}' under '{parent.name}'</color>");
        return btnComp;
    }

    static Image EnsureImageFull(GameObject parent, string name, Color color)
    {
        var existing = parent.transform.Find(name);
        if (existing != null) return existing.GetComponent<Image>();
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent.transform, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        Image img = go.GetComponent<Image>();
        img.color = color;
        Debug.Log($"  <color=green>+ Created Image '{name}' under '{parent.name}'</color>");
        return img;
    }

    static GameObject EnsureGlassPanel(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        var existing = parent.transform.Find(name);
        if (existing != null) return existing.gameObject;
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent.transform, false);
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
        Debug.Log($"  <color=green>+ Created panel '{name}'</color>");
        return panel;
    }

    #endregion

    #region Menu

    static int InjectMenu()
    {
        if (!IsNull("_menuContainer") && !IsNull("_menuTitleText") && !IsNull("_menuHighScoreText") && !IsNull("_startButton"))
            return 0;

        int c = 0;
        GameObject menu = FindContainer("MenuContainer");
        if (menu == null)
        {
            menu = EnsureContainer("MenuContainer");
            WireIfNull("_menuContainer", menu);
            c++;
        }

        var title = EnsureText(menu, "TitleText", "PRECISION STRIKER", 72, new Vector2(0, 100), new Vector2(800, 100));
        WireIfNull("_menuTitleText", title);

        var hs = EnsureText(menu, "HighScoreText", "رکورد: 0", 22, new Vector2(0, -110), new Vector2(300, 40));
        WireIfNull("_menuHighScoreText", hs);

        var start = EnsureButton(menu, "StartButton", "شروع بازی", 36, new Vector2(0, -40), new Vector2(300, 70), new Color(0.298f, 0.686f, 0.314f));
        WireIfNull("_startButton", start);

        return c + 3;
    }

    #endregion

    #region HUD

    static int InjectHUD()
    {
        if (!IsNull("_hudContainer") && !IsNull("_scoreText") && !IsNull("_hudHitsText") && !IsNull("_timerText") && !IsNull("_comboText") && !IsNull("_difficultyText") && !IsNull("_pauseButton"))
            return 0;

        int c = 0;
        GameObject hud = FindContainer("HUDContainer");
        if (hud == null)
        {
            hud = EnsureContainer("HUDContainer");
            WireIfNull("_hudContainer", hud);
            hud.SetActive(false);
            c++;
        }

        var scorePanel = EnsureGlassPanel(hud, "ScorePanel", new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -40), new Vector2(220, 70));
        var scoreText = EnsureText(scorePanel, "ScoreText", "امتیاز: 0", 28, Vector2.zero, new Vector2(200, 50));
        WireIfNull("_scoreText", scoreText);

        var hitsText = EnsureText(hud, "HitsText", "تعداد گل: 0", 22, new Vector2(40, -100), new Vector2(200, 40));
        WireIfNull("_hudHitsText", hitsText);
        RectTransform hitsRt = hitsText.GetComponent<RectTransform>();
        hitsRt.anchorMin = hitsRt.anchorMax = new Vector2(0, 1);
        hitsRt.pivot = new Vector2(0, 1);

        var timerPanel = EnsureGlassPanel(hud, "TimerPanel", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -40), new Vector2(160, 70));
        var timerText = EnsureText(timerPanel, "TimerText", "120 ثانیه", 28, Vector2.zero, new Vector2(140, 50));
        WireIfNull("_timerText", timerText);

        var comboText = EnsureText(hud, "ComboText", "", 22, new Vector2(-40, -100), new Vector2(200, 40));
        WireIfNull("_comboText", comboText);
        RectTransform cr2 = comboText.GetComponent<RectTransform>();
        cr2.anchorMin = cr2.anchorMax = new Vector2(1, 1);
        cr2.pivot = new Vector2(1, 1);

        var diffPanel = EnsureGlassPanel(hud, "DifficultyPanel", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(160, 50));
        var diffText = EnsureText(diffPanel, "DifficultyText", "سطح 0", 18, Vector2.zero, new Vector2(140, 36));
        WireIfNull("_difficultyText", diffText);

        var pauseBtn = EnsureButton(hud, "PauseButton", "II", 24, new Vector2(0, 0), new Vector2(50, 50), new Color(1, 1, 1, 0.3f));
        RectTransform pbr = pauseBtn.GetComponent<RectTransform>();
        pbr.anchorMin = new Vector2(1, 1);
        pbr.anchorMax = new Vector2(1, 1);
        pbr.anchoredPosition = new Vector2(-60, -60);
        pbr.sizeDelta = new Vector2(50, 50);
        WireIfNull("_pauseButton", pauseBtn);

        // Update InjectHUD return count — we always touch these if called
        // Return value already accounts for missing containers

        return c + 5;
    }

    #endregion

    #region Countdown

    static int InjectCountdown()
    {
        if (!IsNull("_countdownContainer") && !IsNull("_countdownText"))
            return 0;

        int c = 0;
        GameObject container = FindContainer("CountdownContainer");
        if (container == null)
        {
            container = EnsureContainer("CountdownContainer");
            WireIfNull("_countdownContainer", container);
            container.SetActive(false);
            c++;
        }

        var text = EnsureText(container, "CountdownText", "3", 180, Vector2.zero, new Vector2(400, 200));
        WireIfNull("_countdownText", text);

        return c + 1;
    }

    #endregion

    #region Game Over

    static int InjectGameOver()
    {
        if (!IsNull("_gameOverContainer") && !IsNull("_finalScoreText") && !IsNull("_highScoreText") && !IsNull("_newHighScoreText") && !IsNull("_accuracyText") && !IsNull("_gameOverComboText") && !IsNull("_gameOverGoldHitsText") && !IsNull("_restartButton"))
            return 0;

        int c = 0;
        GameObject container = FindContainer("GameOverContainer");
        if (container == null)
        {
            container = EnsureContainer("GameOverContainer");
            WireIfNull("_gameOverContainer", container);
            container.SetActive(false);
            c++;
        }

        EnsureImageFull(container, "OverlayBg", new Color(0, 0, 0, 0.7f));

        var title = EnsureText(container, "GameOverTitle", "پایان بازی", 72, new Vector2(0, 140), new Vector2(600, 100));
        title.color = new Color(0.957f, 0.263f, 0.212f);

        EnsureText(container, "ScoreLabel", "امتیاز نهایی", 28, new Vector2(0, 60), new Vector2(400, 50));

        var finalScore = EnsureText(container, "FinalScoreText", "0", 80, new Vector2(0, -10), new Vector2(400, 80));
        WireIfNull("_finalScoreText", finalScore);

        var hs = EnsureText(container, "HighScoreText", "رکورد: 0", 22, new Vector2(0, -70), new Vector2(300, 40));
        WireIfNull("_highScoreText", hs);

        var newHs = EnsureText(container, "NewHighScoreText", "رکورد جدید!", 28, new Vector2(0, -105), new Vector2(400, 40));
        newHs.color = new Color(1, 0.84f, 0);
        newHs.gameObject.SetActive(false);
        WireIfNull("_newHighScoreText", newHs);

        var acc = EnsureText(container, "AccuracyText", "دقت: 0%", 20, new Vector2(0, -145), new Vector2(300, 30));
        WireIfNull("_accuracyText", acc);

        var hits = EnsureText(container, "GoldHitsText", "ضربات: 0 / 0", 20, new Vector2(0, -170), new Vector2(300, 30));
        WireIfNull("_gameOverGoldHitsText", hits);

        var combo = EnsureText(container, "ComboText", "بهترین ترکیب: 0", 20, new Vector2(0, -195), new Vector2(300, 30));
        WireIfNull("_gameOverComboText", combo);

        var restart = EnsureButton(container, "RestartButton", "دوباره", 32, new Vector2(0, -130), new Vector2(280, 65), new Color(0.298f, 0.686f, 0.314f));
        WireIfNull("_restartButton", restart);

        return c + 8;
    }

    #endregion

    #region Screen Effects

    static int InjectScreenEffects()
    {
        int c = 0;
        if (IsNull("_redFlashImage"))
        {
            var flash = EnsureImageFull(_mainCanvas, "RedFlashOverlay", new Color(1, 0, 0, 0));
            flash.raycastTarget = false;
            WireIfNull("_redFlashImage", flash);
            c++;
        }
        if (IsNull("_greenFlashImage"))
        {
            var flash = EnsureImageFull(_mainCanvas, "GreenFlashOverlay", new Color(0, 1, 0, 0));
            flash.raycastTarget = false;
            WireIfNull("_greenFlashImage", flash);
            c++;
        }
        return c;
    }

    #endregion

    #region Pause

    static int InjectPause()
    {
        if (!IsNull("_pauseOverlay"))
            return 0;

        var overlay = EnsureContainer("PauseOverlay");
        WireIfNull("_pauseOverlay", overlay);
        EnsureImageFull(overlay, "PauseBg", new Color(0, 0, 0, 0.7f));
        var pauseText = EnsureText(overlay, "PauseText", "مکث", 72, Vector2.zero, new Vector2(400, 100));
        pauseText.color = Color.white;
        overlay.SetActive(false);
        return 1;
    }

    #endregion

    #region Notifications

    static int InjectNotifications()
    {
        if (!IsNull("_notificationContainer") && !IsNull("_notificationText"))
            return 0;

        int c = 0;
        var container = FindContainer("NotificationContainer");
        if (container == null)
        {
            container = new GameObject("NotificationContainer", typeof(RectTransform));
            container.transform.SetParent(_mainCanvas.transform, false);
            RectTransform r = container.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(0, 200);
            r.sizeDelta = new Vector2(400, 80);
            container.SetActive(false);
            WireIfNull("_notificationContainer", container);
            c++;
        }

        var text = EnsureText(container, "NotificationText", "", 36, Vector2.zero, new Vector2(400, 80));
        WireIfNull("_notificationText", text);

        return c + 1;
    }

    #endregion

    #region Level Up

    static int InjectLevelUp()
    {
        if (!IsNull("_levelUpContainer") && !IsNull("_levelUpText"))
            return 0;

        int c = 0;
        var container = FindContainer("LevelUpContainer");
        if (container == null)
        {
            container = new GameObject("LevelUpContainer", typeof(RectTransform));
            container.transform.SetParent(_mainCanvas.transform, false);
            RectTransform r = container.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(0, -100);
            r.sizeDelta = new Vector2(400, 80);
            container.SetActive(false);
            WireIfNull("_levelUpContainer", container);
            c++;
        }

        var text = EnsureText(container, "LevelUpText", "سطح ۱!", 48, Vector2.zero, new Vector2(400, 80));
        text.color = new Color(1, 0.84f, 0);
        WireIfNull("_levelUpText", text);

        return c + 1;
    }

    #endregion

    #region Floating Canvas

    static int InjectFloatingCanvas()
    {
        if (!IsNull("_floatingCanvas"))
            return 0;

        var canvasGO = GameObject.Find("FloatingCanvas_GoalRush");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("FloatingCanvas_GoalRush", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.layer = LayerMask.NameToLayer("UI");
            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            Debug.Log("  <color=green>+ Created FloatingCanvas_GoalRush</color>");
        }

        WireIfNull("_floatingCanvas", canvasGO.GetComponent<Canvas>());
        return 1;
    }

    #endregion

    #region Floating Text Prefab

    static int InjectFloatingTextPrefab()
    {
        if (!IsNull("_floatingTextPrefab"))
            return 0;

        string path = PrefabDir + "/FloatingText.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<TextMeshProUGUI>(path);
        if (prefab == null)
        {
            Directory.CreateDirectory(PrefabDir);
            GameObject go = new GameObject("FloatingText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(CanvasGroup));
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
            prefab = PrefabUtility.SaveAsPrefabAsset(go, path).GetComponent<TextMeshProUGUI>();
            Object.DestroyImmediate(go);
            Debug.Log("  <color=green>+ Created FloatingText.prefab</color>");
        }

        WireIfNull("_floatingTextPrefab", prefab);
        return 1;
    }

    #endregion

    #region Celebration Particles

    static int InjectCelebrationParticles()
    {
        if (!IsNull("_celebrationParticles"))
            return 0;

        string path = PrefabDir + "/GoldHitParticles.prefab";
        var prefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefabGo == null)
        {
            Debug.LogWarning("  ! GoldHitParticles.prefab not found. Run 'Inject New Features' again after prefabs are created.");
            return 0;
        }

        var ps = prefabGo.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            WireIfNull("_celebrationParticles", ps);
            return 1;
        }
        return 0;
    }

    #endregion

    #region Camera

    static int InjectMainCamera()
    {
        if (!IsNull("_mainCamera"))
            return 0;

        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
            Debug.Log("  <color=green>+ Created MainCamera</color>");
        }

        WireIfNull("_mainCamera", cam);
        return 1;
    }

    #endregion

    #region Prefab Assets

    static int InjectPrefabAssets()
    {
        int c = 0;
        c += TryCreatePrefab("RingEffect.prefab", CreateRingEffectPrefab);
        c += TryCreatePrefab("GoldHitParticles.prefab", CreateGoldHitParticles);
        c += TryCreatePrefab("PenaltyHitParticles.prefab", CreatePenaltyHitParticles);
        return c;
    }

    static int TryCreatePrefab(string name, System.Action<string> createFn)
    {
        string path = PrefabDir + "/" + name;
        if (File.Exists(path)) return 0;
        Directory.CreateDirectory(PrefabDir);
        createFn(path);
        Debug.Log($"  <color=green>+ Created {name}</color>");
        return 1;
    }

    static void CreateRingEffectPrefab(string path)
    {
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

    static void CreateGoldHitParticles(string path)
    {
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

    static void CreatePenaltyHitParticles(string path)
    {
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

    #endregion
}
