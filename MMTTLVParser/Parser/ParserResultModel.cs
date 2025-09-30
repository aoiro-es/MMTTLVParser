using MMTTLVParser.PacketDefinitions;

namespace MMTTLVParser.Parser;

public record PayloadInfo(Type? PayloadType, int Start, int End);

public record ParserResultModel(Packet? Header, PacketStatusEnum Status, PayloadInfo[]? Payloads)
{
    public ParserResultModel(PacketStatusEnum status) : this(null, status, null) { }
}