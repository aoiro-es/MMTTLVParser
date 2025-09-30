namespace MMTTLVParser.PacketDefinitions.MMT.Table;

public abstract record M2ShortSectionMessageTable : Table;

public record MHTimeOffsetTable : M2ShortSectionMessageTable
{
    public DateTime JSTTime { get; init; }
    public Descriptor[] Descriptors { get; init; }
    public byte[] CRC32 { get; init; }

    public MHTimeOffsetTable(ReadOnlySpan<byte> data)
    {
        JSTTime = Utilities.ParseJSTandMJDbytes(data[..5]);
        var descriptrosLoopLength = ((data[5] & 0x0f) << 8) | data[6];
        Descriptors = Descriptor.ParseDescriptors(data[7..(7 + descriptrosLoopLength)], typeof(MHTimeOffsetTable));
        CRC32 = data[^4..].ToArray();
    }
}

public record MHDiscontinuityInformationTable : M2ShortSectionMessageTable
{
    public bool TransitionFlag { get; init; }

    public MHDiscontinuityInformationTable(ReadOnlySpan<byte> data)
    {
        TransitionFlag = Convert.ToBoolean((data[0] & 0x80) >> 7);
    }
}