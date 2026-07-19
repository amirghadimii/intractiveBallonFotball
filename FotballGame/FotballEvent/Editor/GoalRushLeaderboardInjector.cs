using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using UPersian.Components;
using GoalRush;

[InitializeOnLoad]
public static class GoalRushLeaderboardInjector
{
    private static readonly Color[] TeamColors = new Color[]
    {
        new Color(0.957f, 0.263f, 0.212f),
        new Color(0.204f, 0.596f, 0.859f),
        new Color(0.298f, 0.686f, 0.314f),
        new Color(0.957f, 0.867f, 0.212f),
        new Color(0.506f, 0.259f, 0.608f),
        new Color(0.957f, 0.612f, 0.071f),
    };

    private static readonly string[] TeamLabels = { "قرمز", "آبی", "سبز", "زرد", "بنفش", "نارنجی" };

    [MenuItem("GoalRush/Inject Leaderboard + Team Selection")]
    static void Inject()
    {
        var mainCanvas = GameObject.Find("MainCanvas_GoalRush");
        if (mainCanvas == null) { Debug.LogError("MainCanvas_GoalRush not found! Run GoalRush/Setup Complete Scene first."); return; }

        var ui = Object.FindFirstObjectByType<UIManager>();
        if (ui == null) { Debug.LogError("UIManager not found!"); return; }

        var menuContainer = FindChildRecursive(mainCanvas.transform, "MenuContainer");
        if (menuContainer == null) { Debug.LogError("MenuContainer not found!"); return; }

        var gameOverContainer = FindChildRecursive(mainCanvas.transform, "GameOverContainer");

        // Remove any previously injected elements to avoid duplicates
        RemoveOldInjectedElements(mainCanvas.transform);

        AddTeamIcons(menuContainer.transform);
        AddNameInput(menuContainer.transform);
        AddMenuButtons(menuContainer.transform);
        CreateLeaderboardContainer(mainCanvas.transform);
        if (gameOverContainer != null) AddBackToMenuButton(gameOverContainer.transform);

        WireAllNewReferences(ui, mainCanvas.transform);

        AssetDatabase.SaveAssets();
        Debug.Log("<color=green>Leaderboard + Team Selection injected successfully!</color>");
    }

    static void RemoveOldInjectedElements(Transform root)
    {
        string[] names = {
            "TeamIconsRow", "NameInput", "LeaderboardButton", "ResetButton",
            "LeaderboardContainer", "BackToMenuButton"
        };
        foreach (string n in names)
        {
            var go = FindChildRecursive(root, n);
            if (go != null) Object.DestroyImmediate(go);
        }
    }

    #region Team Icons

    static void AddTeamIcons(Transform menuParent)
    {
        var title = FindChildRecursive(menuParent, "TitleText");
        float titleBottom = title != null ? title.GetComponent<RectTransform>().anchoredPosition.y - 50f : 150f;

        CreateUIText("TeamLabel", menuParent, "انتخاب تیم", 22,
            new Vector2(0, titleBottom), new Vector2(300, 30));

        float rowY = titleBottom - 60f;
        GameObject row = new GameObject("TeamIconsRow", typeof(RectTransform));
        row.transform.SetParent(menuParent, false);
        RectTransform rr = row.GetComponent<RectTransform>();
        rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.5f);
        rr.anchoredPosition = new Vector2(0, rowY);
        rr.sizeDelta = new Vector2(720, 90);

        for (int i = 0; i < 6; i++)
            CreateTeamIcon(row.transform, $"TeamIcon_{i}", new Vector2(-300 + i * 120, 0), TeamLabels[i], TeamColors[i]);
    }

    static GameObject CreateTeamIcon(Transform parent, string name, Vector2 pos, string label, Color color)
    {
        GameObject btn = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btn.transform.SetParent(parent, false);
        RectTransform r = btn.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = new Vector2(70, 70);

        Image img = btn.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = true;

        Outline outline = btn.AddComponent<Outline>();
        outline.effectColor = new Color(1, 1, 1, 0.2f);
        outline.effectDistance = Vector2.zero;

        GameObject highlight = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
        highlight.transform.SetParent(btn.transform, false);
        RectTransform hr = highlight.GetComponent<RectTransform>();
        hr.anchorMin = hr.anchorMax = new Vector2(0.5f, 0.5f);
        hr.anchoredPosition = Vector2.zero;
        hr.sizeDelta = new Vector2(86, 86);
        Image himg = highlight.GetComponent<Image>();
        himg.color = new Color(1, 1, 1, 0.25f);
        himg.raycastTarget = false;
        Outline hout = highlight.AddComponent<Outline>();
        hout.effectColor = Color.white;
        hout.effectDistance = new Vector2(2, -2);
        highlight.SetActive(false);

        GameObject txt = new GameObject("Label", typeof(RectTransform), typeof(RtlText));
        txt.transform.SetParent(btn.transform, false);
        RectTransform tr = txt.GetComponent<RectTransform>();
        tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0.5f);
        tr.anchoredPosition = new Vector2(0, -42);
        tr.sizeDelta = new Vector2(80, 20);
        RtlText rtl = txt.GetComponent<RtlText>();
        rtl.text = label;
        rtl.fontSize = 12;
        rtl.alignment = TextAnchor.MiddleCenter;
        rtl.color = new Color(0.8f, 0.8f, 0.8f);
        rtl.fontStyle = FontStyle.Bold;

        return btn;
    }

    #endregion

    #region Name Input

    static void AddNameInput(Transform menuParent)
    {
        var teamRow = FindChildRecursive(menuParent, "TeamIconsRow");
        float inputY = teamRow != null
            ? teamRow.GetComponent<RectTransform>().anchoredPosition.y - 80f
            : -10f;

        CreateUIText("NameLabel", menuParent, "نام بازیکن", 22,
            new Vector2(0, inputY), new Vector2(300, 30));

        float fieldY = inputY - 45f;
        GameObject inputObj = new GameObject("NameInput", typeof(RectTransform), typeof(Image), typeof(InputField));
        inputObj.transform.SetParent(menuParent, false);
        RectTransform ir = inputObj.GetComponent<RectTransform>();
        ir.anchorMin = ir.anchorMax = new Vector2(0.5f, 0.5f);
        ir.anchoredPosition = new Vector2(0, fieldY);
        ir.sizeDelta = new Vector2(320, 44);

        Image bg = inputObj.GetComponent<Image>();
        bg.color = new Color(1, 1, 1, 0.15f);

        GameObject inputText = new GameObject("Text", typeof(RectTransform), typeof(RtlText));
        inputText.transform.SetParent(inputObj.transform, false);
        RectTransform itr = inputText.GetComponent<RectTransform>();
        itr.anchorMin = Vector2.zero;
        itr.anchorMax = Vector2.one;
        itr.anchoredPosition = Vector2.zero;
        itr.sizeDelta = new Vector2(-16, -8);
        RtlText itxt = inputText.GetComponent<RtlText>();
        itxt.fontSize = 20;
        itxt.alignment = TextAnchor.MiddleLeft;
        itxt.color = Color.white;
        itxt.fontStyle = FontStyle.Bold;

        GameObject placeholder = new GameObject("Placeholder", typeof(RectTransform), typeof(RtlText));
        placeholder.transform.SetParent(inputObj.transform, false);
        RectTransform pr = placeholder.GetComponent<RectTransform>();
        pr.anchorMin = Vector2.zero;
        pr.anchorMax = Vector2.one;
        pr.anchoredPosition = Vector2.zero;
        pr.sizeDelta = new Vector2(-16, -8);
        RtlText ptxt = placeholder.GetComponent<RtlText>();
        ptxt.text = "نام خود را وارد کنید...";
        ptxt.fontSize = 18;
        ptxt.alignment = TextAnchor.MiddleLeft;
        ptxt.color = new Color(1, 1, 1, 0.35f);
        ptxt.fontStyle = FontStyle.Normal;

        InputField inputField = inputObj.GetComponent<InputField>();
        inputField.textComponent = itxt;
        inputField.placeholder = ptxt;
        inputField.lineType = InputField.LineType.SingleLine;
        inputField.characterLimit = 20;
    }

    #endregion

    #region Menu Buttons

    static void AddMenuButtons(Transform menuParent)
    {
        var nameInput = FindChildRecursive(menuParent, "NameInput");
        float btnY = nameInput != null
            ? nameInput.GetComponent<RectTransform>().anchoredPosition.y - 65f
            : -100f;

        CreateButton("LeaderboardButton", menuParent,
            "لیدر بورد", 22, new Vector2(-100, btnY), new Vector2(160, 42),
            new Color(0.3f, 0.3f, 0.5f));

        CreateButton("ResetButton", menuParent,
            "حذف همه", 22, new Vector2(100, btnY), new Vector2(160, 42),
            new Color(0.6f, 0.2f, 0.2f));
    }

    #endregion

    #region Leaderboard Panel

    static void CreateLeaderboardContainer(Transform parent)
    {
        GameObject container = new GameObject("LeaderboardContainer", typeof(RectTransform));
        container.transform.SetParent(parent, false);
        RectTransform r = container.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;

        CreateImageFull("LeaderboardBg", container.transform, new Color(0, 0, 0, 0.85f));

        CreateUIText("LeaderboardTitle", container.transform, "لیدر بورد", 48,
            new Vector2(0, 200), new Vector2(500, 70));

        GameObject entryPrefab = CreateLeaderboardEntry(container.transform);
        entryPrefab.SetActive(false);

        GameObject scrollObj = new GameObject("LeaderboardList", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(container.transform, false);
        RectTransform sr = scrollObj.GetComponent<RectTransform>();
        sr.anchorMin = sr.anchorMax = new Vector2(0.5f, 0.5f);
        sr.anchoredPosition = new Vector2(0, 20);
        sr.sizeDelta = new Vector2(580, 350);
        Image scrollBg = scrollObj.GetComponent<Image>();
        scrollBg.color = new Color(1, 1, 1, 0.04f);

        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(scrollObj.transform, false);
        RectTransform cr = content.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0, 1);
        cr.anchorMax = new Vector2(1, 1);
        cr.pivot = new Vector2(0.5f, 1);
        cr.anchoredPosition = Vector2.zero;
        cr.sizeDelta = new Vector2(0, 400);

        ScrollRect scrollRect = scrollObj.GetComponent<ScrollRect>();
        scrollRect.content = cr;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.viewport = sr;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        CreateButton("LeaderboardBackButton", container.transform,
            "برگشت", 24, new Vector2(0, -200), new Vector2(200, 48),
            new Color(0.3f, 0.3f, 0.5f));

        CreateButton("LeaderboardResetButton", container.transform,
            "پاک کردن لیست", 18, new Vector2(0, -260), new Vector2(260, 38),
            new Color(0.6f, 0.2f, 0.2f));

        container.SetActive(false);
    }

    static GameObject CreateLeaderboardEntry(Transform parent)
    {
        GameObject entry = new GameObject("LeaderboardEntry", typeof(RectTransform));
        entry.transform.SetParent(parent, false);
        RectTransform er = entry.GetComponent<RectTransform>();
        er.anchorMin = new Vector2(0.5f, 1);
        er.anchorMax = new Vector2(0.5f, 1);
        er.pivot = new Vector2(0.5f, 1);
        er.sizeDelta = new Vector2(540, 38);

        CreateChildText(entry.transform, "RankText", "#1", 18, new Color(1, 0.84f, 0), new Vector2(60, 28));

        GameObject teamIcon = new GameObject("TeamIconImage", typeof(RectTransform), typeof(Image));
        teamIcon.transform.SetParent(entry.transform, false);
        RectTransform tir = teamIcon.GetComponent<RectTransform>();
        tir.anchorMin = tir.anchorMax = new Vector2(0.5f, 0.5f);
        tir.anchoredPosition = new Vector2(-130, 0);
        tir.sizeDelta = new Vector2(26, 26);
        teamIcon.GetComponent<Image>().raycastTarget = false;
        Outline tOutline = teamIcon.AddComponent<Outline>();
        tOutline.effectColor = new Color(1, 1, 1, 0.2f);
        tOutline.effectDistance = Vector2.zero;

        CreateChildText(entry.transform, "NameText", "---", 16, Color.white, new Vector2(200, 28));
        RectTransform nameRt = entry.transform.Find("NameText").GetComponent<RectTransform>();
        nameRt.anchoredPosition = new Vector2(40, 0);

        CreateChildText(entry.transform, "ScoreText", "0", 18, new Color(0.298f, 0.686f, 0.314f), new Vector2(80, 28));
        RectTransform scoreRt = entry.transform.Find("ScoreText").GetComponent<RectTransform>();
        scoreRt.anchoredPosition = new Vector2(210, 0);
        scoreRt.GetComponent<RtlText>().alignment = TextAnchor.MiddleRight;

        return entry;
    }

    #endregion

    #region Game Over Back Button

    static void AddBackToMenuButton(Transform gameOverParent)
    {
        CreateButton("BackToMenuButton", gameOverParent,
            "منو", 22, new Vector2(0, -210), new Vector2(160, 46),
            new Color(0.4f, 0.4f, 0.4f));
    }

    #endregion

    #region Wiring

    static void WireAllNewReferences(UIManager ui, Transform mainCanvas)
    {
        var so = new SerializedObject(ui);
        var menu = FindChildRecursive(mainCanvas, "MenuContainer");
        var gameOver = FindChildRecursive(mainCanvas, "GameOverContainer");
        Transform menuT = menu != null ? menu.transform : null;
        Transform goT = gameOver != null ? gameOver.transform : null;

        // Team icons
        var teamRow = menuT != null ? FindChildRecursive(menuT, "TeamIconsRow") : null;
        if (teamRow != null)
        {
            var imgs = new List<Image>();
            var hls = new List<GameObject>();
            var btns = new List<Button>();
            Transform rowT = teamRow.transform;
            for (int i = 0; i < 6; i++)
            {
                var icon = FindChildRecursive(rowT, $"TeamIcon_{i}");
                if (icon == null) continue;
                imgs.Add(icon.GetComponent<Image>());
                hls.Add(FindChildRecursive(icon.transform, "Highlight"));
                btns.Add(icon.GetComponent<Button>());
            }
            SetArray(so, "_teamIconImages", imgs.ToArray());
            SetArray(so, "_teamIconHighlights", hls.ToArray());
            SetArray(so, "_teamIconButtons", btns.ToArray());
        }

        // Name input
        if (menuT != null)
        {
            var nameInput = FindChildRecursive(menuT, "NameInput");
            if (nameInput != null)
            {
                SetRef(so, "_nameInput", nameInput.GetComponent<InputField>());
                SetRef(so, "_namePlaceholder", FindChildRecursive(nameInput.transform, "Placeholder")?.GetComponent<RtlText>());
            }

            SetRef(so, "_leaderboardButton", FindChildRecursive(menuT, "LeaderboardButton")?.GetComponent<Button>());
            SetRef(so, "_resetButton", FindChildRecursive(menuT, "ResetButton")?.GetComponent<Button>());
        }

        // Leaderboard panel
        var lb = FindChildRecursive(mainCanvas, "LeaderboardContainer");
        if (lb != null)
        {
            SetRef(so, "_leaderboardContainer", lb);
            var listObj = FindChildRecursive(lb.transform, "LeaderboardList");
            if (listObj != null)
                SetRef(so, "_leaderboardListParent", FindChildRecursive(listObj.transform, "Content")?.GetComponent<RectTransform>());
            SetRef(so, "_leaderboardEntryPrefab", FindChildRecursive(lb.transform, "LeaderboardEntry"));
            SetRef(so, "_leaderboardBackButton", FindChildRecursive(lb.transform, "LeaderboardBackButton")?.GetComponent<Button>());
            SetRef(so, "_leaderboardResetButton", FindChildRecursive(lb.transform, "LeaderboardResetButton")?.GetComponent<Button>());
        }

        // Back button on game over
        if (gameOver != null)
            SetRef(so, "_backToMenuButton", FindChildRecursive(gameOver.transform, "BackToMenuButton")?.GetComponent<Button>());

        so.ApplyModifiedProperties();
        Debug.Log("<color=green>All new references wired.</color>");
    }

    #endregion

    #region Helpers

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

    static GameObject CreateUIText(string name, Transform parent, string text,
        float fontSize, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(RtlText));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = size;

        RtlText rtl = go.GetComponent<RtlText>();
        rtl.text = text;
        rtl.fontSize = (int)fontSize;
        rtl.alignment = TextAnchor.MiddleCenter;
        rtl.color = Color.white;
        rtl.fontStyle = FontStyle.Bold;
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

        GameObject txt = new GameObject("Text", typeof(RectTransform), typeof(RtlText));
        txt.transform.SetParent(btn.transform, false);
        RectTransform tr = txt.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.sizeDelta = Vector2.zero;

        RtlText rtl = txt.GetComponent<RtlText>();
        rtl.text = label;
        rtl.fontSize = (int)fontSize;
        rtl.alignment = TextAnchor.MiddleCenter;
        rtl.color = Color.white;
        rtl.fontStyle = FontStyle.Bold;
        return btn;
    }

    static GameObject CreateChildText(Transform parent, string name, string text, float fontSize, Color color, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(RtlText));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = size;
        r.anchoredPosition = Vector2.zero;

        RtlText rtl = go.GetComponent<RtlText>();
        rtl.text = text;
        rtl.fontSize = (int)fontSize;
        rtl.alignment = TextAnchor.MiddleCenter;
        rtl.color = color;
        rtl.fontStyle = FontStyle.Bold;
        return go;
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

    static void SetRef(SerializedObject so, string field, Object value)
    {
        var prop = so.FindProperty(field);
        if (prop == null) { Debug.LogWarning($"Field '{field}' not found"); return; }
        prop.objectReferenceValue = value;
    }

    static void SetArray(SerializedObject so, string field, Object[] values)
    {
        var prop = so.FindProperty(field);
        if (prop == null) { Debug.LogWarning($"Field '{field}' not found"); return; }
        prop.ClearArray();
        for (int i = 0; i < values.Length; i++)
        {
            prop.InsertArrayElementAtIndex(i);
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    #endregion
}
