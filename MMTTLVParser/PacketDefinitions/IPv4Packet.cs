using MMTTLVParser.PacketDefinitions;
using System.Net;

public record IPv4Packet(int Version, int HeaderLength, byte ServiceType,
    ushort PacketLength, ushort Identifier, byte Flag, int FragmentOffset,
    byte Lifetime, byte Protocol, ushort HeaderChecksum,
    IPAddress SourceAddress, IPAddress DestinationAddress, byte[] ExtensionInformation) : Packet;