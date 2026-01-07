
using System;

/// <summary>
/// Struct that represents a Player
/// </summary>
public struct playerInfo
{
    public string Name;
    public pronouns Pronouns;
    public int playerHashCode;

    //TODO: add more info that needs to be carried out into the game from other screens, such as:
    //Chosen deck (if ever implemented ;) )
    //idk
}

public enum pronouns
{
    SHEHER,
    HEHIM,
    THEYTHEM

}