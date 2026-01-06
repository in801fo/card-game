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

    public static Action<effectTarget, int, float, effectsEnum> OnEffectApplied { get; set; }
    public static effectTarget target { get; set; }

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

public enum effectTarget
{
    /// <summary>
    /// Affects only the local player
    /// </summary>
    LOCAL,
    /// <summary>
    /// All players except for local are affected
    /// </summary>
    ALL_EX,
    /// <summary>
    /// All players included the local player
    /// </summary>
    ALL_INC,
    /// <summary>
    /// A specific player is affected
    /// </summary>
    SPECIFIC_SINGLE,

    /// <summary>
    /// A specific group of people are affected
    /// </summary>
    SPECIFIC_GROUP
}
