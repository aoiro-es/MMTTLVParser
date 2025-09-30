namespace MMTTLVParser.PacketDefinitions.MMT;

public record NonTimedMFUPayload(uint ItemId, byte[] MFUDataByte) : Packet;