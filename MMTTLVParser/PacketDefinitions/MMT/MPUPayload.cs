namespace MMTTLVParser.PacketDefinitions.MMT;

public enum FragmentType : byte
{
    MPUMetadata = 0,
    MovieFragmentMetadata = 1,
    // 通常の放送ではMFUのみが送出される
    MFU = 2
}

public enum DivisionIndexType : byte
{
    Undivided = 0b00,
    DividedIncludingHead = 0b01,
    DividedNotIncludingHeadOrTail = 0b10,
    DividedIncludingTail = 0b11
}

public record MPUPayload(ushort PayloadLength, FragmentType FragmentType, bool TimeDataFlag,
    DivisionIndexType DivisionIndex, bool AggregateFlag, byte DivisionNumberCounter,
    uint MPUSequenceNumber) : Packet;

