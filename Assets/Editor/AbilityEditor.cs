using System.IO;
using UnityEditor;
using UnityEngine;

public class AbilityEditor : EditorWindow
{
    [Header("Creating new ability")]
    private string _abilityName = "New Ability";

    [Header("Editing existing ability")]
    private string _newAbilityName = "New Ability";
    private Ability _selectedAbility;

    [MenuItem("Window/Ability Editor")]
    public static void ShowWindow()
    {
        GetWindow<AbilityEditor>("Ability Editor");
    }

    private void OnGUI()
    {
        CreateAbilitySubMenu();
        EditAbility();
    }

    private void CreateAbilitySubMenu()
    {
        GUILayout.Label("Create a new Ability", EditorStyles.boldLabel);

        _abilityName = EditorGUILayout.TextField("Ability Name", _abilityName);

        if (GUILayout.Button("Create Ability"))
        {
            CreateAbilityAction();
        }
    }

    private void EditAbility()
    {
        GUILayout.Label("Edit an existing Ability", EditorStyles.boldLabel);

        _selectedAbility = (Ability)EditorGUILayout.ObjectField("Ability", _selectedAbility, typeof(Ability), false);

        if (_selectedAbility != null)
        {
            _newAbilityName = EditorGUILayout.TextField("Ability Name", _newAbilityName);

            if (GUILayout.Button("Save Ability"))
            {
                SaveAbilityAction();
            }
        }
    }

    private void SaveAbilityAction()
    {
        if (_selectedAbility == null)
        {
            Debug.LogWarning("No ability selected!");
            return;
        }

        string path = AssetDatabase.GetAssetPath(_selectedAbility);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Could not find the asset path for selected ability!");
            return;
        }

        string newPath = Path.Combine(Path.GetDirectoryName(path), _newAbilityName + ".asset");
        if (path != newPath)
        {
            string renameResult = AssetDatabase.RenameAsset(path, _newAbilityName);
            _selectedAbility.AbilityName = _newAbilityName;
            if (!string.IsNullOrEmpty(renameResult))
            {
                Debug.LogError($"Failed to rename asset: {renameResult}");
                return;
            }
        }

        EditorUtility.SetDirty(_selectedAbility);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = null;
        Selection.activeObject = _selectedAbility;

        Debug.Log($"Ability {_selectedAbility.AbilityName} saved.");
    }

    private void CreateAbilityAction()
    {
        Ability ability = CreateInstance<Ability>();
        ability.AbilityName = _abilityName;

        string path = $"Assets/ScriptableObjects/Abilities/{_abilityName}.asset";
        AssetDatabase.CreateAsset(ability, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = ability;
        Debug.Log($"Created new ability: {_abilityName}");

        _abilityName = "";
    }
}