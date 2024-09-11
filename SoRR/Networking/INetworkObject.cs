namespace SoRR.Networking
{
    public interface INetworkObject
    {
        NetworkId Id { get; }
    }
    public enum NetworkId : ulong;
}
