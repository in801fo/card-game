
using System;
using Unity.Collections;
using Unity.Netcode;

/// <summary>
/// Struct that represents a Player
/// </summary>
public struct playerInfo : INetworkSerializable, IEquatable<playerInfo>
{
    //NAMES CAPPED AT 64 CHARS!!
    public FixedString64Bytes Name;
    public pronouns Pronouns;
    public ulong playerId;
    public ushort playerTagsMask;
    public ushort playerEffectsMask;

    public bool Equals(playerInfo other)
    {
        return base.Equals(other);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Pronouns);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref playerTagsMask);
        serializer.SerializeValue(ref playerEffectsMask);
    }

    public override string ToString()
    {
        return $"PlayerId: {playerId}\nPlayerName: {Name}\nPrnonouns: {Pronouns}\nPlayerTagMask: {playerTagsMask}";
    }

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