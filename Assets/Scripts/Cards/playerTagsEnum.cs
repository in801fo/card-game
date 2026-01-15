/// <summary>
/// Number all entries with powers of two, otherwise it'll break :)
/// if you wanna add more entries (more than 16) you just need to change to a higher-bit-density int in
/// the playerInfo and also in the conversion algorithm 
/// </summary>
public enum playerTagsEnum
{
    /// <summary>
    /// One is tagged with this when they have a character card of a character originating from southern italy
    /// </summary>
    HAS_MERIDIONE = 1,
    /// <summary>
    /// One is tagged with this locally when they have just inflicted damage to the local player
    /// </summary>
    HAS_JUST_INFLICTED_DAMAGE = 2
}