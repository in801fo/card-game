using System;

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

    public static Action<consequenceTarget, int, float, effectsEnum> OnEffectApplied { get; set; }
    public static consequenceTarget target { get; set; }

    public static effectsEnum effectAsEnum { get; set; }

    public static void Apply()
    {
        RuntimeMsg.Info("Applied Effect!", $"Effect {effectAsEnum} applied!");
        OnEffectApplied?.Invoke(target, turnDuration, secondsDuration, effectAsEnum);
    }

    /*public List<Player> GetEffectArea(){

        code to return the right list of affected entities
    
    }
    */
}
