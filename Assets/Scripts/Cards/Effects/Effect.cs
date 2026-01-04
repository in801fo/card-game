using System;
using UnityEngine;

public abstract class Effect
{
    /// <summary>
    /// Amount of seconds which the effects last
    /// </summary>
    public float secondsDuration;

    /// <summary>
    /// Amount of turns which the effects last
    /// </summary>
    public int turnDuration;

    public Action<effectTarget, int, float, effectsEnum> OnEffectApplied; 
    public abstract void Apply();

    public effectTarget target;

    public Effect(effectTarget target, int turnDuration, float secondsDuration)
    {
        this.target = target;
        this.turnDuration = turnDuration;
        this.secondsDuration = secondsDuration;
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
