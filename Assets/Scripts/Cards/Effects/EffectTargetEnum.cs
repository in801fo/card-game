
/// <summary>
/// An enum defining entities to which to apply the consequence of whatever this enum was used for
/// </summary>
public enum consequenceTarget
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
    /// A specific group of players are affected
    /// </summary>
    SPECIFIC_GROUP_EX,
    /// <summary>
    /// A specific group of players are affected including local player
    /// </summary>
    SPECIFIC_GROUP_INC
}