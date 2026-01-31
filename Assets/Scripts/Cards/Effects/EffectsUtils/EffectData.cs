using System;
using Unity.Netcode;

[Serializable]
/// <summary>
/// The effect parameters
/// </summary>
public struct effectData : INetworkSerializable
{
    public effectsEnum effect;
    public bool doesTurns;
    public int turnsLeft;
    public float timeLeft;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref effect);
        serializer.SerializeValue(ref doesTurns);
        serializer.SerializeValue(ref turnsLeft);
        serializer.SerializeValue(ref timeLeft);
    }
}