using System.Net;

namespace MMTTLVParser.PacketDefinitions;
public record IPv6Packet(int Version, byte TrafficClass, uint FlowLabel,
    int PayloadLength, byte NextHeader, byte HopLimit,
    IPAddress SourceAddress, IPAddress DestinationAddress) : Packet;