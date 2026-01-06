using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class ConvertEnumToEffectInvocation
{

    private static Dictionary<effectsEnum, Type> effectsDict = new Dictionary<effectsEnum, Type>()
    {
        {effectsEnum.BLINDNESS, typeof(Blindness)},
        {effectsEnum.CONFUSION, typeof(Confusion)}
    };

    public static void DoIt(effectsEnum effect)
    {
        if (!effectsDict[effect].GetInterfaces().Contains(typeof(IEffect)))
        {
            RuntimeMsg.Error("effects list contains intruder", $"Effects dictionary contains {effectsDict[effect].FullName}, which does not implement IEffect interface.");
            return;
        }

        MethodInfo applyMethod = effectsDict[effect].GetMethod("Apply", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        if (applyMethod == null) applyMethod = typeof(IEffect).GetMethod("Apply");
        applyMethod.Invoke(null, null);
    }
    
}