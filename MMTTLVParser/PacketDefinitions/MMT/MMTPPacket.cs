using System.Buffers.Binary;

namespace MMTTLVParser.PacketDefinitions.MMT;

public enum MMTPFECType : byte
{
    NonProtectedMMTPPacketByALFEC = 0,
    SourcePacketAmongMMTPPacketsProtectedByALFEC = 1,
    RepairPacketAmongMMTPPacketsProtectedByALFEC = 2
}

public enum MMTPPayloadType : byte
{
    MPU = 0x00,
    ContainsOneOrMoreControlMessages = 0x02
}

public enum MultiExtensionHeaderType : ushort
{
    B61 = 0x0001,
    DownloadID = 0x0002,
    ItemFragmentation = 0x0003
}

public abstract record HeaderExtension;

public enum MMTScramblingControlType : byte
{
    Unscrambled = 0b00,
    ScrambledWithEvenKey = 0b10,
    ScrambledWithOddKey = 0b11
}

public record B61MMTPHeaderExtension : HeaderExtension
{
    public MMTScramblingControlType MMTScramblingControl { get; init; }
    public bool ScramblingSubsystemIdentificationControl { get; init; }
    public bool MessageAuthenticationControl { get; init; }
    public bool MMTScramblingInitialCounterValueControl { get; init; }
    public byte ScramblingSubsystemIdentifer { get; init; }
    public ushort PayloadLength { get; init; }
    public byte[]? MMTScramblingInitialCounterValue { get; init; }

    public B61MMTPHeaderExtension(ReadOnlySpan<byte> data)
    {
        MMTScramblingControl = (MMTScramblingControlType)((data[0] & 0x18) >> 3);
        ScramblingSubsystemIdentificationControl = Convert.ToBoolean((data[0] & 0x04) >> 2);
        MessageAuthenticationControl = Convert.ToBoolean((data[0] & 0x02) >> 1);
        MMTScramblingInitialCounterValueControl = Convert.ToBoolean(data[0] & 0x01);
        ScramblingSubsystemIdentifer = data[1];
        PayloadLength = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);
        if (MMTScramblingInitialCounterValueControl)
        {
            // IVは128bit
            MMTScramblingInitialCounterValue = data[4..20].ToArray();
        }
    }
}

public record DownloadIdHeaderExtension : HeaderExtension
{
    public uint DonwloadId { get; init; }

    public DownloadIdHeaderExtension(ReadOnlySpan<byte> data)
    {
        DonwloadId = BinaryPrimitives.ReadUInt32BigEndian(data[..4]);
    }
}

public record ItemFragmentationHeaderExtension : HeaderExtension
{
    public uint ItemFragmentNumber { get; init; }
    public uint LastItemFragmentNumber { get; init; }

    public ItemFragmentationHeaderExtension(ReadOnlySpan<byte> data)
    {
        ItemFragmentNumber = BinaryPrimitives.ReadUInt32BigEndian(data[..4]);
        LastItemFragmentNumber = BinaryPrimitives.ReadUInt32BigEndian(data[4..8]);
    }
}

public record MMTPPacket(byte Version, bool PacketCounterFlag, MMTPFECType FECType,
    bool ExtensionHeaderFlag, bool RAPFlag, MMTPPayloadType PayloadType,
    ushort PacketId, uint DeliveryTimeStamp, uint PacketSequenceNumber,
    // PacketCounterFlag == true
    uint? PacketCounter,
    // ExtensionHeaderFlag == true
    ushort? ExtensionType, ushort? ExtensionLength, HeaderExtension[]? ExtensionHeaders) : Packet;

