using System;
public class Confusion : IEffect
{
    public static float secondsDuration { get; set; }
    public static int turnDuration { get; set; }
    public static Action<effectTarget, int, float, effectsEnum> OnEffectApplied { get; set; }
    public static effectTarget target { get; set; }
    public static effectsEnum effectAsEnum = effectsEnum.CONFUSION;

}