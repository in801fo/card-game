
using System;
using System.Collections.Generic;

public class EnumMaskHandler<T> where T: Enum
{   
    /// <summary>
    /// Given a mask this checks if the mask contains the corresponding value to the passed enum
    /// </summary>
    /// <param name="currentMask">The mask to check</param>
    /// <param name="enumValue">The value of the enum of which to check in the <c>currentMask</c></param>
    /// <returns></returns>
    public static bool HasEnumValueInMask(int currentMask, T enumValue)
    {
        int convertedValue = (int)Convert.ChangeType(enumValue, typeof(int));
        return (currentMask & convertedValue) == convertedValue;
    }

    /// <summary>
    /// Converts the provided mask into a list of elements of T
    /// </summary>
    /// <param name="mask">A ushort (16 bit integer) representing the binary string of T. 
    /// In T (which must be an Enum) we have each entry assigned an integer value. Each value is a power of 2, so to be able to represent
    /// a string of T values as a string of binary digits. 0000000000000011 = 3, the single digits, the ones
    /// set to 1 at least, represent a tag, in this case the string is saying that the player has 2 tags: <c>HAS_MERIDIONE</c> and <c>HAS_JUST_INFLICTED_DAMAGE.</c></param>
    /// <returns></returns>
    public static List<T> ExtractPlayerTagsFromMask(int mask)
    {
        List<T> enumElementsList = new List<T>();

        for (int i = 0; i < 32; i++)
        {
            /*
                Basically just moving the 1 across all the digits of mask (as a binary string)
                and checking if at the i-th position there is a 1 (doing the & (AND)).
            */
            if ((mask & (1 << i)) != 0) enumElementsList.Add((T)(object)(1 << i));
        }

        return enumElementsList;
    }

    public static void SafeAddTagToMask(ref int maskToAdd, T enumValue)
    {
        if (HasEnumValueInMask(maskToAdd, enumValue)) { RuntimeMsg.Info("Avoided tag mess!"); return; }

        maskToAdd += (int)Convert.ChangeType(enumValue, typeof(int));
    }

    public static void SafeRemoveTagFromMask(ref int maskToOperate, T enumValue)
    {
        if (!HasEnumValueInMask(maskToOperate, enumValue)) return;

        maskToOperate -= (int)Convert.ChangeType(enumValue, typeof(int));
    }
}