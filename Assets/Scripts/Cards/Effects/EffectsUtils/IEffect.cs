using System;
using System.Collections.Generic;

public interface IEffect
{
    /// <summary>
    /// Amount of seconds which the effects last
    /// </summary>
    public static float secondsDuration { get; set; }

    /// <summary>
    /// Amount of turns which the effects last
    /// </summary>
    public static int turnDuration { get; set; }

    public static Action<List<ulong>, int, float, effectsEnum> OnEffectApplied { get; set; }
    public static effectsEnum effectAsEnum { get; set; }

    public static void Apply(List<ulong> playerIds)
    {
        OnEffectApplied?.Invoke(playerIds, turnDuration, secondsDuration, effectAsEnum);
    }

}
