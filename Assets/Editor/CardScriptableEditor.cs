
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

    //private CardScriptable target;

    private bool showTags;
    private bool showEffects;
    private bool showSoundEffects;

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
    }


    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();
        Name.SetValue(target, EditorGUILayout.TextField("Name",GetFieldValue<string>(Name)));
        EditorGUILayout.LabelField("Description");
        Desc.SetValue(target, EditorGUILayout.TextArea(GetFieldValue<string>(Desc), new GUILayoutOption[]
        {
            GUILayout.Height(100),
        }));

        cardType.SetValue(target, (int)(cardTypeEnum)EditorGUILayout.EnumPopup("Card Type", GetFieldValue<cardTypeEnum>(cardType)));

        if (IsCharacterCard())
        {
            EditorUtility.SetDirty(target);
            return;
        }

        EditorGUILayout.Separator();

        heals.SetValue(target, (bool)EditorGUILayout.Toggle("Heals", GetFieldValue<bool>(heals)));

        string damageLable = ((bool)heals.GetValue(target)) ? "Healing amount" : "Damage";

        damageAmount.SetValue(target, EditorGUILayout.Slider(damageLable, GetFieldValue<float>(damageAmount), 0, HpManager.maxHp));

        maxCardUsages.SetValue(target, (int)EditorGUILayout.Slider("Max Card Usages", GetFieldValue<int>(maxCardUsages), 0, CardScriptable.maxCardUsagesConst));

        consequenceTarget.SetValue(target, (consequenceTarget)EditorGUILayout.EnumPopup("Target", GetFieldValue<consequenceTarget>(consequenceTarget)));
        //do card effects here


        if ((consequenceTarget)consequenceTarget.GetValue(target) == global::consequenceTarget.SPECIFIC_GROUP_INC ||
            ((consequenceTarget)consequenceTarget.GetValue(target) == global::consequenceTarget.SPECIFIC_GROUP_EX))
        {
            numberOfAffectedPlayers.SetValue(target, EditorGUILayout.IntField("Number Of Affected Players", GetFieldValue<int>(numberOfAffectedPlayers)));
            
            if ((int)numberOfAffectedPlayers.GetValue(target) == -1)
                HandleArray(_affectedTags, "Affect players with tags", "Affected Tag", ref showTags);
        }

        EditorGUILayout.Separator();

        HandleArray(cardEffects, "Inflicting Effects", "Effect", ref showEffects);

        EditorGUILayout.Separator();

        hasSoundEffect.SetValue(target, EditorGUILayout.Toggle("Has Sound Effect", GetFieldValue<bool>(hasSoundEffect)));

        if (GetFieldValue<bool>(hasSoundEffect))
            HandleArray(onUseSFXArr, "Possible SFX on use", "SFX", ref showSoundEffects);
        
        //makes is so that the editor, on closure, writes the modified data on disk
        EditorUtility.SetDirty(target);
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("affectedTags"));//affectedTags.SetValue(target, (playerTagsEnum)EditorGUILayout.Foldout("Affected tags", (playerTagsEnum)affectedTags.GetValue(target)));


        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }

    private T GetFieldValue<T>(FieldInfo fieldInfo)
    {
        return (T)fieldInfo.GetValue(target);
    }

    private void HandleArray(SerializedProperty property, string arrayLable, string elementLable, ref bool foldoutTracker)
    {
        EditorGUI.indentLevel = 1;
        foldoutTracker = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutTracker, arrayLable);

        if (foldoutTracker)
        {
            property.arraySize = EditorGUILayout.IntField(property.arraySize);
            HandleArrayElements(property.arraySize, property, elementLable);
        }

        EditorGUI.indentLevel = 0;
        EditorGUILayout.EndFoldoutHeaderGroup();

    }

    private bool IsCharacterCard()
    {
        return (cardTypeEnum)cardType.GetValue(target) == cardTypeEnum.CHARACTER;
    }

    private void HandleArrayElements(int arrSize, SerializedProperty property, string arrayElementLable)
    {
        for (int i = 0; i < arrSize; i++)
        {
            var affectedTagsElem = property.GetArrayElementAtIndex(i);
            EditorGUI.indentLevel = 2;
            EditorGUILayout.PropertyField(affectedTagsElem, new GUIContent(arrayElementLable + " " + i));
        }

    }

    //nasty nasty reflection!!
    //but unity is worse than me with its login so 
    //I think we're even!!
    private string GetBackingFieldName(string propertyName)
    {
        //Backing fields are fields generated by c#'s compiler when a field has getters and setters defined.
        //They are usefull for unity as it doesn't support the serialization of properties (fields with getters and setters)
        //Here I'm taking advantage of them to be able to serialize properties with private sets to the editor.
        return string.Format("<{0}>k__BackingField", propertyName);

    }

    private FieldInfo GetBackingField(object obj, string propertyName)
    {
        return obj.GetType().GetField(GetBackingFieldName(propertyName), BindingFlags.Instance | BindingFlags.NonPublic);
    }
}
