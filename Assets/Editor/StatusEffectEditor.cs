using UnityEditor;
using UnityEngine;

public class StatusEffectEditor : EditorWindow
{
    [MenuItem("Window/Status Effect Editor")]
    public static void ShowWindow()
    {
        GetWindow<StatusEffectEditor>("Status Effect Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Status Effect Editor", EditorStyles.boldLabel);
    }
}