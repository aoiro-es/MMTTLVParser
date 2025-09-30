namespace MMTTLVParser.PacketDefinitions;

public enum CompressedIPHeaderType : byte
{
    PartialIPv4AndUDPheader = 0x20,
    IPv4HeaderIdentifier = 0x21,
    PartialIPv6AndUDPheader = 0x60,
    NoCompressedHeader = 0x61
}

public record CompressedIPPacket(ushort ContextId, byte SequenceNumber, CompressedIPHeaderType HeaderTypeOfContextIdentification, byte[]? PartialHeader) : Packet;
