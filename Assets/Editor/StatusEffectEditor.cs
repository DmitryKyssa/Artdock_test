using UnityEditor;
using UnityEngine;

public class StatusEffectEditor : EditorWindow
{
    private string _statusEffectName;
    private string _description;
    private TargetType _targetType;
    private bool _isEndless;
    private int _duration;
    private bool _isPeriodic;
    private float _period;
    private AffectedResourceType _affectedResource;
    private float _affectedResourceValuePerPeriod;

    //for editing the status effect
    private string _statusEffectNameToEdit;
    private string _descriptionToEdit;
    private TargetType _targetTypeToEdit;
    private bool _isEndlessToEdit;
    private int _durationToEdit;
    private bool _isPeriodicToEdit;
    private float _periodToEdit;
    private AffectedResourceType _affectedResourceToEdit;
    private float _affectedResourceValuePerPeriodToEdit;
    private StatusEffectData _statusEffectDataToEdit;
    private bool _hasLoaedStatusEffectData = false;

    [MenuItem("Tools/Status Effect Editor")]
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

        _targetType = (TargetType)EditorGUILayout.EnumPopup("Target Type", _targetType);

        _isEndless = EditorGUILayout.Toggle("Is Endless", _isEndless);
        if (!_isEndless)
        {
            _duration = EditorGUILayout.IntField("Duration", _duration);
        }
        _isPeriodic = EditorGUILayout.Toggle("Is Periodic", _isPeriodic);
        if (_isPeriodic)
        {
            _period = EditorGUILayout.FloatField("Period", _period);

            if (_period > _duration)
            {
                EditorGUILayout.HelpBox("Period cannot be less than Duration", MessageType.Warning);

                _period = _duration;
            }
        }
        _affectedResource = (AffectedResourceType)EditorGUILayout.EnumPopup("Affected Resource", _affectedResource);
        _affectedResourceValuePerPeriod = EditorGUILayout.FloatField("Affected Resource Value Per Period", _affectedResourceValuePerPeriod);

        if (GUILayout.Button("Create Status Effect"))
        {
            CreateStatusEffect();
        }

        GUILayout.Space(20);

        GUILayout.Label("Edit Existing Status Effect", EditorStyles.boldLabel);
        _statusEffectDataToEdit = (StatusEffectData)EditorGUILayout.ObjectField("Status Effect Data", _statusEffectDataToEdit, typeof(StatusEffectData), false);

        if (_statusEffectDataToEdit != null && !_hasLoaedStatusEffectData)
        {
            LoadStatusEffectData();
        }

        GUILayout.Space(20);

        if (_hasLoaedStatusEffectData)
        {
            _statusEffectNameToEdit = EditorGUILayout.TextField("Status Effect Name", _statusEffectNameToEdit);
            EditorGUILayout.LabelField("Description", EditorStyles.label);
            _descriptionToEdit = EditorGUILayout.TextArea(_descriptionToEdit, GUILayout.Height(60));

            _targetTypeToEdit = (TargetType)EditorGUILayout.EnumPopup("Target Type", _targetTypeToEdit);

            _isEndlessToEdit = EditorGUILayout.Toggle("Is Endless", _isEndlessToEdit);
            if (!_isEndlessToEdit)
            {
                _durationToEdit = EditorGUILayout.IntField("Duration", _durationToEdit);
            }
            _isPeriodicToEdit = EditorGUILayout.Toggle("Is Periodic", _isPeriodicToEdit);
            if (_isPeriodicToEdit)
            {
                _periodToEdit = EditorGUILayout.FloatField("Period", _periodToEdit);
                if (_periodToEdit > _durationToEdit)
                {
                    EditorGUILayout.HelpBox("Period cannot be less than Duration", MessageType.Warning);

                    _periodToEdit = _durationToEdit;
                }
            }
            _affectedResourceToEdit = (AffectedResourceType)EditorGUILayout.EnumPopup("Affected Resource", _affectedResourceToEdit);
            _affectedResourceValuePerPeriodToEdit = EditorGUILayout.FloatField("Affected Resource Value Per Period", _affectedResourceValuePerPeriodToEdit);

            if (GUILayout.Button("Update Status Effect"))
            {
                UpdateStatusEffect();
            }
        }
    }

    private void LoadStatusEffectData()
    {
        _statusEffectNameToEdit = _statusEffectDataToEdit.StatusEffectName;
        _descriptionToEdit = _statusEffectDataToEdit.Description;
        _targetTypeToEdit = _statusEffectDataToEdit.TargetType;
        _isEndlessToEdit = _statusEffectDataToEdit.IsEndless;
        _durationToEdit = _statusEffectDataToEdit.Duration;
        _isPeriodicToEdit = _statusEffectDataToEdit.IsPeriodic;
        _periodToEdit = _statusEffectDataToEdit.Period;
        _affectedResourceToEdit = _statusEffectDataToEdit.AffectedResource;
        _affectedResourceValuePerPeriodToEdit = _statusEffectDataToEdit.AffectedResourceValuePerPeriod;

        _hasLoaedStatusEffectData = true;
    }

    private void UpdateStatusEffect()
    {
        if (_statusEffectDataToEdit != null)
        {
            _statusEffectDataToEdit.StatusEffectName = _statusEffectNameToEdit;
            _statusEffectDataToEdit.Description = _descriptionToEdit;
            _statusEffectDataToEdit.TargetType = _targetTypeToEdit;
            _statusEffectDataToEdit.IsEndless = _isEndlessToEdit;
            _statusEffectDataToEdit.Duration = _isEndlessToEdit ? 0 : _durationToEdit;
            _statusEffectDataToEdit.IsPeriodic = _isPeriodicToEdit;
            _statusEffectDataToEdit.Period = _isPeriodicToEdit ? _periodToEdit : _durationToEdit;
            _statusEffectDataToEdit.AffectedResource = _affectedResourceToEdit;
            _statusEffectDataToEdit.AffectedResourceValuePerPeriod = _affectedResourceValuePerPeriodToEdit;

            EditorUtility.SetDirty(_statusEffectDataToEdit);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            _statusEffectDataToEdit = null;
            _hasLoaedStatusEffectData = false;
        }
    }

    private void CreateStatusEffect()
    {
        StatusEffectData statusEffectData = CreateInstance<StatusEffectData>();
        statusEffectData.name = _statusEffectName;
        statusEffectData.StatusEffectName = _statusEffectName;
        statusEffectData.Description = _description;
        statusEffectData.TargetType = _targetType;
        statusEffectData.IsEndless = _isEndless;
        statusEffectData.Duration = _isEndless ? 0 : _duration;
        statusEffectData.IsPeriodic = _isPeriodic;
        statusEffectData.Period = _isPeriodic ? _period : _duration;
        statusEffectData.AffectedResource = _affectedResource;
        statusEffectData.AffectedResourceValuePerPeriod = _affectedResourceValuePerPeriod;

        string path = $"Assets/Resources/StatusEffects/{statusEffectData}.asset";
        AssetDatabase.CreateAsset(statusEffectData, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
    }
}