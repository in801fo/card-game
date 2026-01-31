using UnityEditor;
using UnityEditor.Rendering;


[CustomEditor(typeof(effectData))]
public class EffectDataEditor : Editor
{
    SerializedProperty doesTurns;
    SerializedProperty Time;
    SerializedProperty Turns;
    SerializedProperty representingEffect;

    private void OnEnable()
    {
        doesTurns = serializedObject.FindProperty("doesTurns");
        Time = serializedObject.FindProperty("timeLeft");
        Turns = serializedObject.FindProperty("turnsLeft");
        representingEffect = serializedObject.FindProperty("representingEffect");
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();

        representingEffect.SetEnumValue((effectsEnum)EditorGUILayout.EnumPopup("shit", representingEffect.GetEnumValue<effectsEnum>()));

        if (!doesTurns.boolValue)
            Time.floatValue = EditorGUILayout.FloatField("Time", Time.floatValue);
        else
            Turns.intValue = EditorGUILayout.IntField("Turns", Turns.intValue);

        EditorUtility.SetDirty(target);
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}
