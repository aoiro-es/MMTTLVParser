using MMTTLVParser.PacketDefinitions;
using MMTTLVParser.Parser;

namespace MMTTLVParser.Sample.Statistics;

internal class StatisticsFilter
{
    public Dictionary<TlvPacketType, int> _typeCounts { get; } = Enum.GetValues<TlvPacketType>().ToDictionary(t => t, t => 0);

    public void ProcessPackets(IEnumerable<PacketTreeNode> parentNodes)
    {
        foreach (var parent in parentNodes)
        {
            if (parent.Children.First().Data is TLVPacket tlv)
            {
                _typeCounts[tlv.PacketType]++;
            }
        }
    }
}
