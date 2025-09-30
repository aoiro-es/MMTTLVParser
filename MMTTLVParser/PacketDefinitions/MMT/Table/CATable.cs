using System.Buffers.Binary;

namespace MMTTLVParser.PacketDefinitions.MMT.Table;

public record CATable : Table
{
    public byte TableId { get; init; }
    public byte Version { get; init; }
    public ushort Length { get; init; }
    public Descriptor[] Descriptors { get; init; }

    public CATable(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        Version = data[1];
        Length = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);
        Descriptors = Descriptor.ParseDescriptors(data[4..(4 + Length)], typeof(CATable));
    }
}
