namespace MMTTLVParser.PacketDefinitions;

public record UDPPacket(ushort SourcePort, ushort DestinationPort,
    ushort DataLength, ushort Checksum) : Packet;
