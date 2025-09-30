using MMTTLVParser.PacketDefinitions.MMT;

namespace MMTTLVParser.Parser.Reassemblers;

public class MPUPayloadReassembler : ReassemblerBase
{
    protected override DivisionIndexType GetDivisionIndex(PacketTreeNode node)
        => node.Data is MPUPayload mpu ? mpu.DivisionIndex : throw new InvalidDataException();

    protected override ReadOnlySpan<byte> SliceMiddle(PacketTreeNode node, PayloadInfo info, ReadOnlySpan<byte> payloadData)
        => payloadData[GetHeaderLength(node)..];

    protected override ReadOnlySpan<byte> SliceTail(PacketTreeNode node, PayloadInfo info, ReadOnlySpan<byte> payloadData)
        => payloadData[GetHeaderLength(node)..];

    private static int GetHeaderLength(PacketTreeNode node)
        => node.Data is MPUPayload mpu ? mpu.TimeDataFlag ? 14 : 4 : throw new InvalidDataException();
}

public class ControlMessagesReassembler : ReassemblerBase
{
    // PacketId ごとに先頭フラグメントから得た PayloadType を保持
    private readonly Dictionary<ushort, Type> _payloadTypes = [];

    protected override DivisionIndexType GetDivisionIndex(PacketTreeNode node)
        => node.Data is ControlMessages cm ? cm.DivisionIndex : throw new InvalidDataException();

    protected override void OnHeadCaptured(ushort key, PacketTreeNode node, PayloadInfo info, ReadOnlySpan<byte> payloadData)
    {
        if (info.PayloadType is not null)
        {
            _payloadTypes[key] = info.PayloadType;
        }
    }

    protected override Type ResolveResultType(ushort key, PacketTreeNode node, PayloadInfo info)
    {
        if (_payloadTypes.TryGetValue(key, out var t))
        {
            _payloadTypes.Remove(key);
            return t;
        }
        throw new InvalidDataException("Head fragment type not captured.");
    }
}