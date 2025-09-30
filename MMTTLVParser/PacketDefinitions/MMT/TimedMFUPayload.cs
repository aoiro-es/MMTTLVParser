namespace MMTTLVParser.PacketDefinitions.MMT;

public record TimedMFUPayload(uint MovieFragmentSequenceNumber, uint SampleNumber,
    uint Offset, byte Priority, byte DependencyCounter, byte[] MFUDataByte) : Packet;
