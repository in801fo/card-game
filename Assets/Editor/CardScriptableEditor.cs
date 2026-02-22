
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardScriptable)), CanEditMultipleObjects]
public class CardScriptableEditor : Editor
{
    private FieldInfo Name;
    private FieldInfo Desc;
    private FieldInfo cardType;
    private FieldInfo damageAmount;
    private SerializedProperty cardEffects;
    private FieldInfo consequenceTarget;
    private FieldInfo numberOfAffectedPlayers;
    private SerializedProperty _affectedTags;
    private FieldInfo heals;
    private FieldInfo hasSoundEffect;
    private SerializedProperty onUseSFXArr;
    private FieldInfo maxCardUsages;

    private FieldInfo characterCardCompatibility;

    private bool showTags;
    private bool showEffects;
    private bool showSoundEffects;

    /// <summary>
    /// A Dictionary containing all opened custom editors for structured elements
    /// </summary>
    private Dictionary<Type, List<Editor>> cachedEditors = new Dictionary<Type, List<Editor>>();

    private void OnEnable()
    {

        //target = target as CardScriptable;

        Name = GetBackingField(target, "Name");
        Desc = GetBackingField(target, "Description");
        cardType = GetBackingField(target, "Type");
        damageAmount = GetBackingField(target, "damageAmount");
        cardEffects = serializedObject.FindProperty("cardEffects");
        consequenceTarget = GetBackingField(target, "consequenceTarget");
        numberOfAffectedPlayers = GetBackingField(target, "numberOfAffectedPlayers");
        heals = GetBackingField(target, "Heals");
        _affectedTags = serializedObject.FindProperty("affectedTags");
        hasSoundEffect = GetBackingField(target, "hasSoundEffect");
        onUseSFXArr = serializedObject.FindProperty("onUseSoundEffect");
        maxCardUsages = GetBackingField(target, "maxCardUsages");
        characterCardCompatibility = GetBackingField(target, "characterCardCompatibility");
    }


    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();
        Name.SetValue(target, EditorGUILayout.TextField("Name", GetFieldValue<string>(Name)));
        EditorGUILayout.LabelField("Description");
        Desc.SetValue(target, EditorGUILayout.TextArea(GetFieldValue<string>(Desc), new GUILayoutOption[]
        {
            GUILayout.Height(100),
        }));

        cardType.SetValue(target, (int)(cardTypeEnum)EditorGUILayout.EnumPopup("Card Type", GetFieldValue<cardTypeEnum>(cardType)));

        if (IsCardType(cardTypeEnum.CHARACTER))
        {
            EditorUtility.SetDirty(target);
            return;
        }


        EditorGUILayout.Separator();

        heals.SetValue(target, (bool)EditorGUILayout.Toggle("Heals", GetFieldValue<bool>(heals)));

        string damageLable = ((bool)heals.GetValue(target)) ? "Healing amount" : "Damage";

        damageAmount.SetValue(target, EditorGUILayout.Slider(damageLable, GetFieldValue<float>(damageAmount), 0, HpManager.maxHp));

        if(!IsCardType(cardTypeEnum.TRAP))
            maxCardUsages.SetValue(target, (int)EditorGUILayout.Slider("Max Card Usages", GetFieldValue<int>(maxCardUsages), 0, CardScriptable.maxCardUsagesConst));

        if (IsCardType(cardTypeEnum.ACTION))
            HandleCharacterCardCompatibility();
        

        EditorGUILayout.Separator();

        EditorGUILayout.HelpBox("If you want to affect players with specific tag (i.e. that satisfy specific requirements)\nchoose either SPECIFIC_GROUP_INC or SPECIFIC_GROUP_EX as consequence targets and input '-1'\n in the number of affected players", MessageType.Warning);

        EditorGUILayout.Separator();

        HandleConsequenceTarget();

        EditorGUILayout.Separator();

        HandleArray(cardEffects, "Inflicting Effects", "Effect", ref showEffects);

        EditorGUILayout.Separator();

        hasSoundEffect.SetValue(target, EditorGUILayout.Toggle("Has Sound Effect", GetFieldValue<bool>(hasSoundEffect)));

        //handle sfx array
        if (GetFieldValue<bool>(hasSoundEffect))
            HandleArray(onUseSFXArr, "Possible SFX on use", "SFX", ref showSoundEffects);

        //makes is so that the editor, on closure, writes the modified data to disk
        EditorUtility.SetDirty(target);

        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }

    private void HandleCharacterCardCompatibility()
    {
        CardScriptable value = GetFieldValue<CardScriptable>(characterCardCompatibility);
        if (value != null && value.Type != cardTypeEnum.CHARACTER) {
            value = null;
            Debug.LogError("Cannot set Character Card Compatibility Field to non Character Type Card Scriptable");
        }
        characterCardCompatibility.SetValue(target, (CardScriptable)EditorGUILayout.ObjectField("Character Card Max Comp.", value, typeof(CardScriptable), false));
    }

    private void HandleConsequenceTarget()
    {
        consequenceTarget.SetValue(target, (consequenceTarget)EditorGUILayout.EnumPopup("Target", GetFieldValue<consequenceTarget>(consequenceTarget)));

        if ((consequenceTarget)consequenceTarget.GetValue(target) == global::consequenceTarget.SPECIFIC_GROUP_INC ||
            ((consequenceTarget)consequenceTarget.GetValue(target) == global::consequenceTarget.SPECIFIC_GROUP_EX))
        {
            numberOfAffectedPlayers.SetValue(target, EditorGUILayout.IntField("Number Of Affected Players", GetFieldValue<int>(numberOfAffectedPlayers)));

            if ((int)numberOfAffectedPlayers.GetValue(target) == -1)
                HandleArray(_affectedTags, "Affect players with tags", "Affected Tag", ref showTags);
        }
    }

    private T GetFieldValue<T>(FieldInfo fieldInfo)
    {
        return (T)fieldInfo.GetValue(target);
    }

    /// <summary>
    /// Handles the creation of an array of type <c>property.GetType()</c>. 
    /// </summary>
    /// <param name="property"></param>
    /// <param name="arrayLable"></param>
    /// <param name="elementLable"></param>
    /// <param name="foldoutTracker"></param>
    private void HandleArray(SerializedProperty property, string arrayLable, string elementLable, ref bool foldoutTracker)
    {
        EditorGUI.indentLevel = 1;
        foldoutTracker = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutTracker, arrayLable);

        if (foldoutTracker)
        {
            property.arraySize = EditorGUILayout.IntField(property.arraySize);
            HandleArrayElements(property.arraySize, property, elementLable);
        }
        /*else
        {
            Type propType = property.GetType();
            if (cachedEditors.ContainsKey(propType))
                DestroyArrayElements(GetElementFromCustomEditorsDict(propType));
        }*/

        EditorGUI.indentLevel = 0;
        EditorGUILayout.EndFoldoutHeaderGroup();

    }

    /*private void DestroyArrayElements(List<Editor> editors)
    {
        
        for (int i = 0; i < editors.Count; i++)
        {
            DestroyImmediate(editors[i]);
        }
    }*/

    private bool IsCardType(cardTypeEnum value)
    {
        return (cardTypeEnum)cardType.GetValue(target) == value;
    }

    private void HandleArrayElements(int arrSize, SerializedProperty property, string arrayElementLable)
    {
        for (int i = 0; i < arrSize; i++)
        {
            EditorGUI.indentLevel = 2;
            SerializedProperty arrayElem = property.GetArrayElementAtIndex(i);

            //generate, if present the custom editor for the provided object
            //Editor elemEditor = Editor.CreateEditor(arrayElem.objectReferenceValue);

            //if generation failed use the default property field
            //if (!elemEditor)
            //{
                GUILayout.Label(arrayElementLable + " " + i);
                EditorGUILayout.PropertyField(arrayElem, true);
                //return;
            //}

            //AddElementToCustomEditorsDict(property.GetType(), elemEditor);

        }

    }

    /// <summary>
    /// Adds element to the <c>cachedEditors</c> dictionary 
    /// </summary>
    /// <param name="t"></param>
    /// <param name="editor"></param>
    /*private void AddElementToCustomEditorsDict(Type t, Editor editor)
    {
        if (cachedEditors.ContainsKey(t)) cachedEditors[t].Add(editor);
        else cachedEditors.Add(t, new List<Editor>() { editor });
    }*/

    /*private List<Editor> GetElementFromCustomEditorsDict(Type t)
    {
        if (cachedEditors.ContainsKey(t))
            return cachedEditors[t];

        else return null;
    }*/

    //nasty nasty reflection!!
    //but unity is worse than me with its login so 
    //I think we're even!!
    private string GetBackingFieldName(string propertyName)
    {
        //Backing fields are fields generated by c#'s compiler when a field has getters and setters defined.
        //They are useful for unity as it doesn't support the serialization of properties (fields with getters and setters)
        //Here I'm taking advantage of them to be able to serialize properties with private sets to the editor.
        return string.Format("<{0}>k__BackingField", propertyName);

    }

    private FieldInfo GetBackingField(object obj, string propertyName)
    {
        return obj.GetType().GetField(GetBackingFieldName(propertyName), BindingFlags.Instance | BindingFlags.NonPublic);
    }
}
