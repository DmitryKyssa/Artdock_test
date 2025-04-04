using UnityEditor;

[CustomEditor(typeof(AbilitiesManager))]
public class AbilitiesManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty loadFromResourcesProp = serializedObject.FindProperty("_loadFromResources");
        SerializedProperty abilitiesProp = serializedObject.FindProperty("_abilities");

        EditorGUILayout.PropertyField(loadFromResourcesProp);

        if (!loadFromResourcesProp.boolValue)
        {
            EditorGUILayout.PropertyField(abilitiesProp, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}