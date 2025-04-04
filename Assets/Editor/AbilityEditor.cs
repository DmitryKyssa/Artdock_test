using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AbilityEditor : EditorWindow
{
    private string _abilityName = "New Ability";
    private Sprite _sprite;
    private string _description;
    private TargetType _targetType;
    private EffectZone _effectZone;
    private float _areaOfEffectRadius;
    private float _castTime;
    private SpentResourceType _spentResource;
    private int _resourceCost;
    private AffectedResourceType _affectedResource;
    private int _affectedResourceValue;
    private ReceivedResourceType _receivedResource;
    private int _receivedResourceValue;
    private float _cooldownAfterUsing;
    private Condition _condition;
    private int _conditionValue;

    private AudioClip _audioClip;
    private bool _isOffsetFromStart;
    private float _offsetFromStart;

    private List<AnimationData> _animationDatas = new List<AnimationData>();
    private AnimationClip _animationClip;
    private TargetType _animationTargetType;
    private AnimationData _fromSavedAnimationData;

    private List<StatusEffectData> _statusEffectDatas = new List<StatusEffectData>();
    private StatusEffectData _newStatusEffectData;

    private SFXData _sfxData;
    private SFXData _fromSavedSFXData;

    private List<VFXData> _vfxDatas = new List<VFXData>();
    private ParticleSystem _particleSystem;
    private bool _isOffsetFromStartVFX;
    private float _offsetFromStartVFX;
    private TargetType _vfxTargetType;
    private VFXData _fromSavedVFXData;

    private string _newAbilityName = "Rename ability";
    private AbilityData _selectedAbility;

    private Vector2 _scrollPosition = Vector2.zero;
    private AbilityData _abilityForSaving;

    [MenuItem("Window/Ability Editor")]
    public static void ShowWindow()
    {
        GetWindow<AbilityEditor>("Ability Editor");
    }

    private void OnEnable()
    {
        _abilityForSaving = CreateInstance<AbilityData>();
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 20));
        CreateAbilitySubMenu();
        DrawLine();
        EditAbility();
        EditorGUILayout.EndScrollView();
    }

    private void CreateAbilitySubMenu()
    {
        GUILayout.Label("Create a new Ability", EditorStyles.boldLabel);

        _abilityName = EditorGUILayout.TextField("Ability Name", _abilityName);
        _sprite = (Sprite)EditorGUILayout.ObjectField("Ability Sprite", _sprite, typeof(Sprite), false);

        EditorGUILayout.LabelField("Description", EditorStyles.label);
        _description = EditorGUILayout.TextArea(_description, GUILayout.Height(60));

        TargetAndEffectZone();

        _castTime = EditorGUILayout.FloatField("Cast Time", _castTime);

        _spentResource = (SpentResourceType)EditorGUILayout.EnumPopup("Spent Resource", _spentResource);
        _resourceCost = EditorGUILayout.IntField("Resource Cost", _resourceCost);

        _affectedResource = (AffectedResourceType)EditorGUILayout.EnumPopup("Affected Resource", _affectedResource);
        _affectedResourceValue = EditorGUILayout.IntField("Affected Resource Value", _affectedResourceValue);

        _receivedResource = (ReceivedResourceType)EditorGUILayout.EnumPopup("Received Resource", _receivedResource);
        _receivedResourceValue = EditorGUILayout.IntField("Received Resource Value", _receivedResourceValue);

        _cooldownAfterUsing = EditorGUILayout.FloatField("Cooldown After Using", _cooldownAfterUsing);

        _condition = (Condition)EditorGUILayout.EnumPopup("Condition", _condition);
        if (_condition != Condition.None)
        {
            _conditionValue = EditorGUILayout.IntField("Condition Value", _conditionValue);
        }

        DrawLine();
        SFXDataCreate();
        DrawLine();
        AnimationDataCreate();
        DrawLine();
        VFXDataCreate();
        DrawLine();
        StatusEffectDataCreate();
        DrawLine();

        if (GUILayout.Button("Create Ability"))
        {
            CreateAbilityAction();
        }

        if (GUILayout.Button("Reset"))
        {
            _abilityForSaving = CreateInstance<AbilityData>();
            _abilityName = "New Ability";
            _sprite = null;
            _description = string.Empty;
            _targetType = TargetType.Self;
            _effectZone = EffectZone.SingleTarget;
            _areaOfEffectRadius = 0f;
            _castTime = 0f;
            _spentResource = SpentResourceType.Stamina;
            _resourceCost = 0;
            _affectedResource = AffectedResourceType.HP;
            _affectedResourceValue = 0;
            _receivedResource = ReceivedResourceType.None;
            _receivedResourceValue = 0;
            _cooldownAfterUsing = 0f;
            _condition = Condition.None;
            _conditionValue = 0;

            _sfxData = null;
            _audioClip = null;
            _isOffsetFromStart = false;
            _offsetFromStart = 0f;

            _animationDatas.Clear();
            _animationClip = null;
            _animationTargetType = TargetType.Self;

            _vfxDatas.Clear();
            _particleSystem = null;
            _isOffsetFromStartVFX = false;
            _offsetFromStartVFX = 0f;
        }
    }

    private static void DrawLine()
    {
        GUI.color = Color.red;
        GUILayout.Box("", GUILayout.Height(5), GUILayout.ExpandWidth(true));
        GUI.color = Color.white;
    }

    private void StatusEffectDataCreate()
    {
        GUILayout.Label("Status Effect Data List", EditorStyles.boldLabel);

        for (int i = 0; i < _statusEffectDatas.Count; i++)
        {
            _statusEffectDatas[i] = (StatusEffectData)EditorGUILayout.ObjectField("Status Effect Data", _statusEffectDatas[i], typeof(StatusEffectData), false);

            if (GUILayout.Button("Remove"))
            {
                _statusEffectDatas.RemoveAt(i);
                break;
            }
        }

        _newStatusEffectData = (StatusEffectData)EditorGUILayout.ObjectField("Add Status Effect Data", _newStatusEffectData, typeof(StatusEffectData), false);
        if (GUILayout.Button("Add new Status Effect Data"))
        {
            if (_newStatusEffectData != null)
            {
                _abilityForSaving.StatusEffectDatas.Add(_newStatusEffectData);
                _statusEffectDatas.Add(_newStatusEffectData);
                _newStatusEffectData = null;
                Debug.Log($"Added new Status Effect Data: {_abilityForSaving.StatusEffectDatas[^1].StatusEffectName}");
            }
            _newStatusEffectData = null;
        }
    }

    private void VFXDataCreate()
    {
        GUILayout.Label("VFX Data List", EditorStyles.boldLabel);

        for (int i = 0; i < _vfxDatas.Count; i++)
        {
            _vfxDatas[i].ParticleSystem = (ParticleSystem)EditorGUILayout.ObjectField("Particle System", _vfxDatas[i].ParticleSystem, typeof(ParticleSystem), false);
            _vfxDatas[i].TargetType = (TargetType)EditorGUILayout.EnumPopup("VFX Target Type", _vfxDatas[i].TargetType);

            if (GUILayout.Button("Remove"))
            {
                _vfxDatas.RemoveAt(i);
                break;
            }
        }

        if (GUILayout.Button("Add VFX Data"))
        {
            OneElementCreate();
            if (_vfxDatas.Count > 0)
            {
                VFXData newVFXData = _vfxDatas[^1];
                string path = $"Assets/ScriptableObjects/VFX/{_abilityName}_{_vfxDatas.Count}.asset";
                AssetDatabase.CreateAsset(newVFXData, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Debug.Log($"Saved new VFX Data: {newVFXData.name}");
            }
        }

        _fromSavedVFXData = (VFXData)EditorGUILayout.ObjectField("VFX Data from assets", _fromSavedVFXData, typeof(VFXData), false);
        if (GUILayout.Button("Add an existing VFX Data"))
        {
            if (_fromSavedVFXData != null)
            {
                _vfxDatas.Add(_fromSavedVFXData);
            }
            _fromSavedVFXData = null;
        }

        void OneElementCreate()
        {
            VFXData vfxData = CreateInstance<VFXData>();
            vfxData.ParticleSystem = _particleSystem;
            vfxData.TargetType = _vfxTargetType;
            vfxData.IsOffsetFromStart = _isOffsetFromStartVFX;
            vfxData.OffsetFromStart = _offsetFromStartVFX;
            _vfxDatas.Add(vfxData);
        }
    }

    private void AnimationDataCreate()
    {
        GUILayout.Label("Animation Data List", EditorStyles.boldLabel);

        for (int i = 0; i < _animationDatas.Count; i++)
        {
            _animationDatas[i].AnimationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", _animationDatas[i].AnimationClip, typeof(AnimationClip), false);
            _animationDatas[i].TargetType = (TargetType)EditorGUILayout.EnumPopup("Animation Target Type", _animationDatas[i].TargetType);

            if (GUILayout.Button("Remove"))
            {
                _animationDatas.RemoveAt(i);
                break;
            }
        }

        if (GUILayout.Button("Add new Animation Data"))
        {
            OneElementCreate();
            SaveNewAnimationData();
        }

        _fromSavedAnimationData = (AnimationData)EditorGUILayout.ObjectField("Animation Data from assets", _fromSavedAnimationData, typeof(AnimationData), false);
        if (GUILayout.Button("Add an existing Animation Data"))
        {
            if (_fromSavedAnimationData != null)
            {
                _animationDatas.Add(_fromSavedAnimationData);
            }
            _fromSavedAnimationData = null;
        }

        void OneElementCreate()
        {
            AnimationData animationData = CreateInstance<AnimationData>();
            animationData.AnimationClip = _animationClip;
            animationData.TargetType = _animationTargetType;
            _animationDatas.Add(animationData);
        }

        void SaveNewAnimationData()
        {
            if (_animationDatas.Count > 0)
            {
                AnimationData newAnimationData = _animationDatas[^1];
                string path = $"Assets/ScriptableObjects/Animation/{_abilityName}_{_animationDatas.Count}.asset";
                AssetDatabase.CreateAsset(newAnimationData, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Debug.Log($"Saved new Animation Data: {newAnimationData.name}");
            }
        }
    }

    private void SFXDataCreate()
    {
        GUILayout.Label("SFX Data", EditorStyles.boldLabel);

        if (_sfxData == null)
        {
            _sfxData = CreateInstance<SFXData>();
        }

        _audioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", _audioClip, typeof(AudioClip), false);
        _isOffsetFromStart = EditorGUILayout.Toggle("Is Offset From Start", _isOffsetFromStart);

        if (_isOffsetFromStart)
        {
            _offsetFromStart = EditorGUILayout.FloatField("Offset From Start", _offsetFromStart);
        }

        _sfxData.AudioClip = _audioClip;
        _sfxData.IsOffsetFromStart = _isOffsetFromStart;
        _sfxData.OffsetFromStart = _offsetFromStart;

        _fromSavedSFXData = (SFXData)EditorGUILayout.ObjectField("SFX Data from assets", _fromSavedSFXData, typeof(SFXData), false);
        if (GUILayout.Button("Add an existing SFX Data"))
        {
            if (_fromSavedSFXData != null)
            {
                _sfxData = _fromSavedSFXData;
            }
            _fromSavedSFXData = null;
        }
    }

    private void TargetAndEffectZone()
    {
        _targetType = (TargetType)EditorGUILayout.EnumPopup("Target Type", _targetType);
        if (_targetType == TargetType.Enemy || _targetType == TargetType.Ally || _targetType == TargetType.Self)
        {
            _effectZone = EffectZone.SingleTarget;
            _areaOfEffectRadius = 0f;
        }
        else if (_targetType == TargetType.All)
        {
            _effectZone = EffectZone.AllLocation;
            _areaOfEffectRadius = 0f;
        }
        else
        {
            EffectZone[] allowedValues = Enum.GetValues(typeof(EffectZone))
                .Cast<EffectZone>()
                .Where(value => value != EffectZone.SingleTarget && value != EffectZone.AllLocation)
                .ToArray();

            int selectedIndex = Array.IndexOf(allowedValues, _effectZone);
            if (selectedIndex == -1)
            {
                selectedIndex = 0;
            }
            selectedIndex = EditorGUILayout.Popup("Effect Zone", selectedIndex, allowedValues.Select(v => v.ToString()).ToArray());
            _effectZone = allowedValues[selectedIndex];
            _areaOfEffectRadius = EditorGUILayout.FloatField("Area of Effect Radius", _areaOfEffectRadius);
        }
    }

    private void EditAbility()
    {
        GUILayout.Label("Edit an existing Ability", EditorStyles.boldLabel);

        _selectedAbility = (AbilityData)EditorGUILayout.ObjectField("New ability name", _selectedAbility, typeof(AbilityData), false);

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
        if(_abilityName != _abilityForSaving.AbilityName)
        {
            _abilityForSaving = CreateInstance<AbilityData>();
        }

        _abilityForSaving.AbilityName = _abilityName;
        _abilityForSaving.Sprite = _sprite;
        _abilityForSaving.Description = _description;
        _abilityForSaving.TargetType = _targetType;
        _abilityForSaving.EffectZone = _effectZone;
        _abilityForSaving.AreaOfEffectRadius = _areaOfEffectRadius;
        _abilityForSaving.CastTime = _castTime;
        _abilityForSaving.SpentResource = _spentResource;
        _abilityForSaving.ResourceCost = _resourceCost;
        _abilityForSaving.AffectedResource = _affectedResource;
        _abilityForSaving.AffectedResourceValue = _affectedResourceValue;
        _abilityForSaving.ReceivedResource = _receivedResource;
        _abilityForSaving.ReceivedResourceValue = _receivedResourceValue;
        _abilityForSaving.CooldownAfterUsing = _cooldownAfterUsing;
        _abilityForSaving.Condition = _condition;
        _abilityForSaving.ConditionValue = _conditionValue;
        for (int i = 0; i < _animationDatas.Count; i++)
        {
            _abilityForSaving.AnimationDatas.Add(_animationDatas[i]);
        }

        if (_sfxData != null)
        {
            if (AssetDatabase.Contains(_sfxData))
            {
                var newSFXData = CreateInstance<SFXData>();
                newSFXData.AudioClip = _sfxData.AudioClip;
                newSFXData.IsOffsetFromStart = _sfxData.IsOffsetFromStart;
                newSFXData.OffsetFromStart = _sfxData.OffsetFromStart;

                newSFXData.name = $"{_abilityName}_SFX";
                string pathSFX = $"Assets/ScriptableObjects/SFX/{newSFXData.name}.asset";

                AssetDatabase.CreateAsset(newSFXData, pathSFX);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                _abilityForSaving.SFXData = newSFXData;
            }
        }

        for (int i = 0; i < _vfxDatas.Count; i++)
        {
            _abilityForSaving.VFXDatas.Add(_vfxDatas[i]);
        }

        for (int i = 0; i < _statusEffectDatas.Count; i++)
        {
            _abilityForSaving.StatusEffectDatas.Add(_statusEffectDatas[i]);
        }

        string path = $"Assets/ScriptableObjects/Abilities/{_abilityName}.asset";
        AssetDatabase.CreateAsset(_abilityForSaving, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = _abilityForSaving;
        Debug.Log($"Created new ability: {_abilityName}");
    }
}