
using System.Collections.Generic;

public class TagHandler
{
    public static bool HasTag(int currentMask, playerTagsEnum tag)
    {
        return (currentMask & (1 << ((int)tag - 1))) != 0;
    }

    /// <summary>
    /// Converts the provided mask into a list of playerTagsEnum
    /// </summary>
    /// <param name="mask">A ushort (16 bit integer) representing the binary string of the playerTagsEnum of the 
    /// currentPlayer. In the playerTagsEnum we have each entry assigned an integer value. Each value is a power of 2, so to be able to represent
    /// a string of playerTagsEnum values as a string of binary digits. 0000000000000011 = 3, the single digits, the ones
    /// set to 1 at least, represent a tag, in this case the string is saying that the player has 2 tags: <c>HAS_MERIDIONE</c> and <c>HAS_JUST_INFLICTED_DAMAGE.</c></param>
    /// <returns></returns>
    public static List<playerTagsEnum> ExtractPlayerTagsFromMask(ushort mask)
    {
        List<playerTagsEnum> playerTags = new List<playerTagsEnum>();
        //tmp
        string tmp = "";
        for (int i = 0; i < 16; i++)
        {
            /*
                Basically just moving the 1 across all the digits of mask (as a binary string)
                and checking if at the i-th position there is a 1 (doinf the & (AND)).
            */
            if ((mask & (1 << i)) != 0) playerTags.Add((playerTagsEnum)(1 << i));
            if ((mask & (1 << i)) != 0) tmp += $" {(playerTagsEnum)(1 << i)}";
        }

        return playerTags;
    }
}