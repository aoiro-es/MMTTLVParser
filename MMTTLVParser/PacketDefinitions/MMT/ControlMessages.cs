namespace MMTTLVParser.PacketDefinitions.MMT;

public record ControlMessages(DivisionIndexType DivisionIndex, bool LengthInformationExtensionFlag, bool AggregaateFlag, byte DivisionNumberCounter) : Packet;