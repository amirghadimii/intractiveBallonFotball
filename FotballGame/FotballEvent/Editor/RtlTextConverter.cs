using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using RTLTMPro;

public static class RtlTextConverter
{
    [MenuItem("GoalRush/Convert TMP to RTLTMPro")]
    static void ConvertTmpToRtl()
    {
        var allTmp = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        if (allTmp.Length == 0)
        {
            Debug.Log("<color=yellow>No TextMeshProUGUI components found in the scene.</color>");
            return;
        }

        Undo.IncrementCurrentGroup();
        int groupIndex = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Convert TMP to RTLTMPro");

        int count = 0;
        foreach (var tmp in allTmp)
        {
            var go = tmp.gameObject;

            Undo.RecordObject(go, "Convert TMP to RTLTMPro");

            string text = tmp.text;
            float fontSize = tmp.fontSize;
            FontStyles fontStyle = tmp.fontStyle;
            TextAlignmentOptions alignment = tmp.alignment;
            Color color = tmp.color;
            bool raycastTarget = tmp.raycastTarget;

            Object.DestroyImmediate(tmp);

            var rtl = go.AddComponent<RTLTextMeshPro>();
            rtl.text = text;
            rtl.fontSize = fontSize;
            rtl.fontStyle = fontStyle;
            rtl.alignment = alignment;
            rtl.color = color;
            rtl.raycastTarget = raycastTarget;

            Undo.RegisterCreatedObjectUndo(rtl, "Add RTLTextMeshPro");
            count++;
        }

        Undo.CollapseUndoOperations(groupIndex);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=green>✓ {count} TextMeshProUGUI component(s) converted to RTLTextMeshPro.</color>");
    }

    [MenuItem("GoalRush/Convert RTLTMPro to TMP")]
    static void ConvertRtlToTmp()
    {
        var allRtl = Object.FindObjectsByType<RTLTextMeshPro>(FindObjectsSortMode.None);
        if (allRtl.Length == 0)
        {
            Debug.Log("<color=yellow>No RTLTextMeshPro components found in the scene.</color>");
            return;
        }

        Undo.IncrementCurrentGroup();
        int groupIndex = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Convert RTLTMPro to TMP");

        int count = 0;
        foreach (var rtl in allRtl)
        {
            var go = rtl.gameObject;

            Undo.RecordObject(go, "Convert RTLTMPro to TMP");

            string text = rtl.text;
            float fontSize = rtl.fontSize;
            FontStyles fontStyle = rtl.fontStyle;
            TextAlignmentOptions alignment = rtl.alignment;
            Color color = rtl.color;
            bool raycastTarget = rtl.raycastTarget;

            Object.DestroyImmediate(rtl);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = fontStyle;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.raycastTarget = raycastTarget;
            tmp.isRightToLeftText = false;

            Undo.RegisterCreatedObjectUndo(tmp, "Add TextMeshProUGUI");
            count++;
        }

        Undo.CollapseUndoOperations(groupIndex);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=green>✓ {count} RTLTextMeshPro component(s) converted to TextMeshProUGUI.</color>");
    }
}
