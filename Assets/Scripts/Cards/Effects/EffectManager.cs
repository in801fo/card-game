using System;
using System.Collections.Generic;
using UnityEngine;

public static class EffectManager
{
    public static void ApplyEffects(List<effectsEnum> effects)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            ConvertEnumToEffectInvocation.DoIt(effects[i]);
        }
    }
}