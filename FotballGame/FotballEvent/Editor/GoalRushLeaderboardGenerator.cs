using RTLTMPro;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UPersian.Components;

namespace GoalRush.EditorTools
{
    public class GoalRushLeaderboardGenerator : EditorWindow
    {
        private TMP_FontAsset _persianFont;
        private Color _modalBg = new Color(0.043f, 0.118f, 0.078f, 1f);
        private Color _borderGold = new Color(0.980f, 0.800f, 0.082f, 1f);
        private Color _separator = new Color(0.094f, 0.200f, 0.137f, 1f);

        [MenuItem("GoalRush/Generate Leaderboard UI Modal")]
        public static void ShowWindow()
        {
            GetWindow<GoalRushLeaderboardGenerator>("Leaderboard Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("\U0001F3C6 Persian Football Leaderboard Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Creates a complete Persian RTL Leaderboard modal in the scene.", MessageType.Info);
            EditorGUILayout.Space();

            _persianFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Persian TMP Font:", _persianFont, typeof(TMP_FontAsset), false);
            EditorGUILayout.Space();
            _modalBg = EditorGUILayout.ColorField("Modal Background:", _modalBg);
            _borderGold = EditorGUILayout.ColorField("Border & Title Gold:", _borderGold);
            _separator = EditorGUILayout.ColorField("Separator Color:", _separator);

            EditorGUILayout.Space();
            if (GUILayout.Button("\u26A1 Generate Leaderboard UI", GUILayout.Height(40)))
                GenerateUI();
        }

        private void GenerateUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject cgo = new GameObject("Canvas_Leaderboard");
                canvas = cgo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = cgo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                cgo.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(cgo, "Create Canvas");
            }

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            }

            string[] existing = { "LeaderboardOverlay" };
            foreach (string n in existing)
            {
                var found = FindChild(canvas.transform, n);
                if (found != null) DestroyImmediate(found);
            }

            GameObject overlay = new GameObject("LeaderboardOverlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(canvas.transform, false);
            RectTransform ovRect = overlay.GetComponent<RectTransform>();
            ovRect.anchorMin = Vector2.zero;
            ovRect.anchorMax = Vector2.one;
            ovRect.offsetMin = Vector2.zero;
            ovRect.offsetMax = Vector2.zero;
            overlay.GetComponent<Image>().color = new Color(0, 0, 0, 0.65f);

            GameObject modal = new GameObject("LeaderboardModal", typeof(RectTransform), typeof(Image), typeof(Outline));
            modal.transform.SetParent(overlay.transform, false);
            RectTransform mRect = modal.GetComponent<RectTransform>();
            mRect.sizeDelta = new Vector2(680, 520);
            mRect.anchoredPosition = Vector2.zero;
            modal.GetComponent<Image>().color = _modalBg;
            Outline ol = modal.GetComponent<Outline>();
            ol.effectColor = _borderGold;
            ol.effectDistance = new Vector2(2, -2);

            VerticalLayoutGroup vlg = modal.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(25, 25, 20, 25);
            vlg.spacing = 10;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            CreateHeader(modal.transform);
            CreateSep("HeaderSeparator", modal.transform);
            CreateTableHeader(modal.transform);
            CreateSep("TableHeaderSeparator", modal.transform);

            string[][] rows = {
                new[] {"1", "\u0633\u0647\u0631\u0627\u0628 \u0645\u0631\u0627\u062F\u06CC", "\u067E\u0627\u0633\u0627\u0631\u06AF\u0627\u062F", "2850"},
                new[] {"2", "\u06A9\u06CC\u0627\u0631\u0634 \u0631\u0627\u062F", "\u0633\u067E\u0647\u0631 \u062A\u0647\u0631\u0627\u0646", "2640"},
                new[] {"3", "\u067E\u0648\u06CC\u0627 \u062F\u0631\u062E\u0634\u0627\u0646", "\u0627\u0631\u0648\u0646\u062F \u062E\u0648\u0632\u0633\u062A\u0627\u0646", "2410"},
                new[] {"4", "\u0633\u06CC\u0627\u0648\u0634 \u0632\u0646\u062F\u06CC", "\u06A9\u0648\u0647\u0633\u062A\u0627\u0646 \u0627\u0644\u0628\u0631\u0632", "2190"},
                new[] {"5", "\u062F\u0627\u0646\u06CC\u0627\u0644 \u067E\u0627\u0631\u0633\u0627", "\u0645\u0647\u0631\u06AF\u0627\u0646 \u067E\u0627\u0631\u0633", "1980"},
            };
            Color[] rowColors = {
                new Color(0.980f, 0.800f, 0.082f, 1f),
                Color.white,
                new Color(0.976f, 0.451f, 0.086f, 1f),
                Color.white,
                Color.white,
            };

            for (int i = 0; i < rows.Length; i++)
            {
                if (i > 0) CreateSep($"Sep_{i}", modal.transform);
                CreateRow(modal.transform, $"Row_{i + 1}", rows[i][0], rows[i][1], rows[i][2], rows[i][3], rowColors[i]);
            }

            Undo.RegisterCreatedObjectUndo(overlay, "Generate Leaderboard");
            Selection.activeGameObject = modal;
            Debug.Log("\u2705 Leaderboard UI Modal generated successfully!");
        }

        private void CreateHeader(Transform parent)
        {
            GameObject hdr = new GameObject("HeaderPanel", typeof(RectTransform));
            hdr.transform.SetParent(parent, false);
            LayoutElement hle = hdr.AddComponent<LayoutElement>();
            hle.preferredHeight = 50;
            HorizontalLayoutGroup hlg = hdr.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            GameObject closeBtn = new GameObject("CloseButton", typeof(RectTransform));
            closeBtn.transform.SetParent(hdr.transform, false);
            LayoutElement cle = closeBtn.AddComponent<LayoutElement>();
            cle.preferredWidth = 40;
            RtlText closeTmp = closeBtn.AddComponent<RtlText>();
            SetupTMP(closeTmp, "\u00D7", 28, Color.white, TextAlignmentOptions.Center);
            closeBtn.AddComponent<Button>().targetGraphic = closeTmp;

            GameObject spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(hdr.transform, false);
            spacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            GameObject title = new GameObject("TitleText", typeof(RectTransform));
            title.transform.SetParent(hdr.transform, false);
            LayoutElement tle = title.AddComponent<LayoutElement>();
            tle.preferredWidth = 450;
            RtlText titleTmp = title.AddComponent<RtlText>();
            SetupTMP(titleTmp, "\U0001F3C6 \u0628\u0631\u062A\u0631\u06CC\u0646\u200C\u0647\u0627\u06CC \u0644\u06CC\u06AF \u0641\u0648\u062A\u0628\u0627\u0644", 24, _borderGold, TextAlignmentOptions.Right);
          
        }

        private void CreateTableHeader(Transform parent)
        {
            GameObject th = new GameObject("TableHeaderRow", typeof(RectTransform));
            th.transform.SetParent(parent, false);
            LayoutElement thle = th.AddComponent<LayoutElement>();
            thle.preferredHeight = 35;
            HorizontalLayoutGroup hlg = th.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.reverseArrangement = true;

            CreateCell(th.transform, "Th_Rank", "\u0631\u062A\u0628\u0647", 70, _borderGold, true);
            CreateCell(th.transform, "Th_Name", "\u0646\u0627\u0645 \u0628\u0627\u0632\u06CC\u06A9\u0646", 200, _borderGold, true);
            CreateCell(th.transform, "Th_Club", "\u0628\u0627\u0634\u06AF\u0627\u0647", 220, _borderGold, true);
            CreateCell(th.transform, "Th_Score", "\u0627\u0645\u062A\u06CC\u0627\u0632", 100, _borderGold, true);
        }

        private void CreateRow(Transform parent, string name, string rank, string player, string club, string score, Color color)
        {
            GameObject row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            LayoutElement rle = row.AddComponent<LayoutElement>();
            rle.preferredHeight = 42;
            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.reverseArrangement = true;

            CreateCell(row.transform, $"{name}_Rank", rank, 70, color, false);
            CreateCell(row.transform, $"{name}_Name", player, 200, color, false);
            CreateCell(row.transform, $"{name}_Club", club, 220, color, false);
            CreateCell(row.transform, $"{name}_Score", score, 100, color, false);
        }

        private void CreateCell(Transform parent, string name, string text, float width, Color color, bool isBold)
        {
            GameObject cell = new GameObject(name, typeof(RectTransform));
            cell.transform.SetParent(parent, false);
            cell.AddComponent<LayoutElement>().preferredWidth = width;
            RtlText tmp = cell.AddComponent<RtlText>();
            SetupTMP(tmp, text, isBold ? 17 : 16, color, TextAlignmentOptions.Center);
          
        }

        private void CreateSep(string name, Transform parent)
        {
            GameObject sep = new GameObject(name, typeof(RectTransform), typeof(Image));
            sep.transform.SetParent(parent, false);
            sep.AddComponent<LayoutElement>().preferredHeight = 1;
            sep.GetComponent<Image>().color = _separator;
        }

        private void SetupTMP(RtlText tmp, string text, float size, Color color, TextAlignmentOptions align)
        {
            tmp.text = text;
      
        }

        private GameObject FindChild(Transform parent, string name)
        {
            foreach (Transform c in parent)
            {
                if (c.name == name) return c.gameObject;
                var f = FindChild(c, name);
                if (f != null) return f;
            }
            return null;
        }
    }
}
