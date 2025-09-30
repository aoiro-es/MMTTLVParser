namespace MMTTLVParser.PacketDefinitions;

public enum TlvPacketType : byte
{
    IPv4Packet = 0x01,
    IPv6Packet = 0x02,
    HeaderCompressedIPPacket = 0x03,
    TransmissionControlSignalPacket = 0xfe,
    NullPacket = 0xff
}

public record TLVPacket(ushort DataLength, TlvPacketType PacketType) : Packet;