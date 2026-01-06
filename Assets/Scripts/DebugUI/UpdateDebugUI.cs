using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class UpdateDebugUI : MonoBehaviour{
    [HideInInspector] public string text = "Decompile My Balls, Bitch";
    public static UpdateDebugUI Instance;

    private object mainSerializedObject;

    private List<FieldInfo> fieldCache = new List<FieldInfo>();

    private bool modifySubField;

    private List<string> avoidUpdateFields = new List<string>();
    public class ParsingException : Exception{

        private object failedObject;

        private Type type;

        public ParsingException(string msg, object failedObject) : base(msg){
            this.failedObject = failedObject;
        }

        public ParsingException(string msg) : base(msg){
        }

        public ParsingException(string msg, object failedObject, Type type) : base(msg){
            this.failedObject = failedObject;
            this.type = type;
        }
        
        public override string ToString(){
            return $"Failed to parse object {failedObject} to type {type}.";
        }
    }

    private void Awake(){
        if (!Instance) Instance = this;
        else Destroy(this);
    }

    public void StartVariableUpdate(object serializedObject, List<string> avoidUpdateFields = null)
    {
        if(avoidUpdateFields != null && avoidUpdateFields.Count > 0) this.avoidUpdateFields = avoidUpdateFields;
        this.mainSerializedObject = serializedObject;
    }

    /// <summary>
    /// Updates the value of <c>varName</c> with the value passed in <c>value</c>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="varName"></param>
    /// <param name="value"></param>
    public void UpdateValue<T>(string varName, T value){
        //if the varName contains a . that means that it's a "path"
        //to the actual variable to update

        //this means that the field shall not be updated
        if (avoidUpdateFields.Contains(varName)) return;

        modifySubField = varName.Contains('.');
        

        FieldInfo variable = null;
        int varIndex = 0;

        //if you don't need to update subfields
        if (!modifySubField){

            //search through the cached fields...
            varIndex = fieldCache.FindIndex(info => info.Name.Equals(varName) && info.DeclaringType.Name.Equals(mainSerializedObject.GetType().Name));
            //in the case in which you don't find it
            if (varIndex == -1){

                print("cacheMiss");
                //update the cache
                fieldCache.Clear();
                FieldInfo[] fields = DebugUIManager.mainObjectFields.ToArray();
                fieldCache = new List<FieldInfo>(fields);
                //search again...
                varIndex = fieldCache.FindIndex(info => info.Name.Equals(varName) && info.DeclaringType.Name.Equals(mainSerializedObject.GetType().Name));
            }
        }

        object fieldOwner = mainSerializedObject;

        if(!modifySubField) variable = fieldCache[varIndex];
        else variable = RetrieveField(varName, mainSerializedObject, out fieldOwner);

//        print("fieldOwner " + fieldOwner.GetType() + " " + fieldOwner + " var: " + variable);

        if (variable == null)
//            print("Governo ladro");
            return;
        

        object sanitizedValue = SanitizeInput(value.ToString(), variable.FieldType);

        //        print("sanitizedValue: " + sanitizedValue + " variabnleName: " + variable.Name + " variableType: " + variable.FieldType + " " + varName.Contains('.'));

        if (sanitizedValue == null) return;

        //if you have to modify a subfield (only one level of subfields is serialized therefore only
        //setting the first level of sub fields...)
        if (modifySubField){
            //print(varName);
            //modify the desired field on a copy of the main field (fieldOwner) with the sanitized value
            fieldOwner.GetType().GetField(variable.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .SetValue(fieldOwner, sanitizedValue);
            //use that copy to set the entire value of the original main field 
            mainSerializedObject.GetType().GetField(varName.Substring(0, varName.IndexOf('.')))
                .SetValue(mainSerializedObject, fieldOwner);
        }
        else variable.SetValue(fieldOwner, sanitizedValue);
    }

    /// <summary>
    /// Retrieves a field from an object, using its path, <c>varName</c>, the object that owns it, <c>currentObject</c>, and a variable to store the owner of the subfield, <c>fieldOwner</c>.
    /// </summary>
    /// <param name="varName"></param>
    /// <param name="currentObject"></param>
    /// <param name="fieldOwner"></param>
    /// <returns>The FieldInfo of the specified field/subfield</returns>
    private FieldInfo RetrieveField(string varName, object currentObject, out object fieldOwner){
        fieldOwner = null;
//        print("to retrive: " + varName + " currentObject: " + currentObject);
        //print("varName: " + varName + " currentObject: " + currentObject.GetType() + " currentObject.GetType().GetField(varName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy): " + currentObject.GetType().GetField(varName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy));
        //if no more . are present in the varName, that means that we've reached the sub object that owns the variable
        //which name is stored in varName
        if (!varName.Contains(".") && !string.IsNullOrEmpty(varName)){
            fieldOwner = currentObject;
            return currentObject.GetType().GetField(varName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        }

        if (string.IsNullOrEmpty(varName)) return null;

        //save the name of the current sub object
        string objectName = varName.Substring(0, varName.IndexOf('.'));
        //remove the current sub object from the path
        varName = varName.Remove(0, varName.IndexOf('.')+1);

        //check if the variable exists
        FieldInfo lookingFor = currentObject.GetType().GetField(objectName);
        
        //if it exists save if by getting the value
        if (lookingFor != null) currentObject = lookingFor.GetValue(currentObject);

        //do this again with the new values
        return RetrieveField(varName, currentObject, out fieldOwner);
    }

    private object SanitizeInput(string value, Type valueType){
        if(valueType.Equals(typeof(string))) return HandleString(value);

        value = new string((from c in value where !char.IsWhiteSpace(c) select c).ToArray());
        if (valueType.ToString().Equals("System.Single") || valueType.ToString().Equals("System.Int32")) return SanitizeNumber(value, valueType);
        if (valueType.Equals(typeof(bool))) return HandleBool(value);


        try{
            var converter = TypeDescriptor.GetConverter(valueType);
            return converter.ConvertFrom(value);
        }catch(Exception e){
            RuntimeMsg.Error(e, $"While attempting conversion of value {value} to type {valueType} an error occured. This error is not fatal.");
        }

        return null;
    }

    private object SanitizeNumber(string value, Type valueType){
        value = new string((from c in value where char.IsNumber(c) || c.Equals(',') || c.Equals('.') || c.Equals('-') select c).ToArray());
        //float.TryParse ignores the '.', even though it's the norm, idk
        if (value.Contains('.')) value = value.Replace('.', ',');
        /*a hash set creates a unique id for each element in it
        id doesn't allow for any duplicate of an item to be added*/
        //idk why I was doing this...
        /*HashSet<char> set = new HashSet<char>();

        value = new string(value.Where(c => set.Add(c)).ToArray());
*/
//        print("first sanitization: " + value);

        if (valueType.ToString().Equals("System.Single"))
            return HandleFloat(value);
        else return HandleInt(value);
    }

    private float HandleFloat(string value){
        if (float.TryParse(value, out float res)) return res;
        else RuntimeMsg.Error(new ParsingException("Unable to parse value, wtf did you write in there?!", value, typeof(float)));
        return new float();
    }
    private int HandleInt(string value){
        string numericalString = new string((from c in value where char.IsNumber(c) || c.Equals('-') select c).ToArray());
        if (int.TryParse(numericalString, out int res)) return res;
        else RuntimeMsg.Error(new ParsingException("Unable to parse value, wtf did you write in there?!", value, typeof(int)));
        return new int();
    }
    private bool HandleBool(string value){
        string sanitizedInput = RemoveAllButLetters(value);
        if (sanitizedInput.ToLower().Equals(true.ToString().ToLower())) return true;
        
        if (sanitizedInput.ToLower().Equals(false.ToString().ToLower())) return false;
        else RuntimeMsg.Error(new ParsingException("Unable to parse value, wtf did you write in there?!", value, typeof(bool)));
        return new bool();
    }
    private string HandleString(string value){
        return value;
    }

    private string RemoveAllButLetters(string str){
        return new string((from c in str where char.IsLetter(c) select c).ToArray());
    }

}
