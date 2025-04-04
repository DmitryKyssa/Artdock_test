using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;

public class StatusEffectEditor : EditorWindow
{
    private string _statusEffectName;
    private string _description;
    private bool _isEndless;
    private int _duration;
    private bool _isPeriodic;
    private float _period;
    private AffectedResourceType _affectedResource;
    private int _affectedResourceValuePerPeriod;

    [MenuItem("Window/Status Effect Editor")]
    public static void ShowWindow()
    {
        GetWindow<StatusEffectEditor>("Status Effect Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Status Effect Editor", EditorStyles.boldLabel);

        _statusEffectName = EditorGUILayout.TextField("Status Effect Name", _statusEffectName);

        EditorGUILayout.LabelField("Description", EditorStyles.label);
        _description = EditorGUILayout.TextArea(_description, GUILayout.Height(60));

        _isEndless = EditorGUILayout.Toggle("Is Endless", _isEndless);
        if (!_isEndless)
        {
            _duration = EditorGUILayout.IntField("Duration", _duration);
        }
        _isPeriodic = EditorGUILayout.Toggle("Is Periodic", _isPeriodic);
        if (_isPeriodic)
        {
            _period = EditorGUILayout.FloatField("Period", _period);
        }
        _affectedResource = (AffectedResourceType)EditorGUILayout.EnumPopup("Affected Resource", _affectedResource);
        _affectedResourceValuePerPeriod = EditorGUILayout.IntField("Affected Resource Value Per Period", _affectedResourceValuePerPeriod);

        if (GUILayout.Button("Create Status Effect"))
        {
            CreateStatusEffect();
        }
    }

    private void CreateStatusEffect()
    {
        StatusEffectData statusEffectData = CreateInstance<StatusEffectData>();
        statusEffectData.name = _statusEffectName;
        statusEffectData.StatusEffectName = _statusEffectName;
        statusEffectData.Description = _description;
        statusEffectData.IsEndless = _isEndless;
        statusEffectData.Duration = _isEndless ? 0 : _duration;
        statusEffectData.IsPeriodic = _isPeriodic;
        statusEffectData.Period = _isPeriodic ? _period : 0;
        statusEffectData.AffectedResource = _affectedResource;
        statusEffectData.AffectedResourceValuePerPeriod = _affectedResourceValuePerPeriod;

        string path = $"Assets/ScriptableObjects/StatusEffects/{statusEffectData}.asset";
        AssetDatabase.CreateAsset(statusEffectData, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
    }
}