using MMTTLVParser.PacketDefinitions.MMT;

namespace MMTTLVParser.Parser.Reassemblers;

public abstract class ReassemblerBase : IReassembler
{
    private readonly Dictionary<ushort, ReassemblerBuffer> _buffers = [];

    public ReassemblerResult? Consume(PacketTreeNode node, PayloadInfo info, ReadOnlySpan<byte> data)
    {
        if (node.Parent?.Data is not MMTPPacket mmtp)
            throw new InvalidDataException();

        var key = mmtp.PacketId;

        if (!_buffers.TryGetValue(key, out var buf))
        {
            buf = new ReassemblerBuffer();
            _buffers.Add(key, buf);
        }

        buf.RelatedNodes.Add(node);

        var payloadData = data[info.Start..info.End];
        var division = GetDivisionIndex(node);

        switch (division)
        {
            case DivisionIndexType.DividedIncludingHead:
                if (buf.Status == DivisionStatus.Initial)
                {
                    buf.DataBuffer.AddRange(SliceHead(node, info, payloadData));
                    buf.Status = DivisionStatus.InFragment;
                    OnHeadCaptured(key, node, info, payloadData);
                    return null;
                }
                buf.OnError();
                return null;

            case DivisionIndexType.DividedNotIncludingHeadOrTail:
                if (buf.Status == DivisionStatus.InFragment)
                {
                    buf.DataBuffer.AddRange(SliceMiddle(node, info, payloadData));
                    return null;
                }
                buf.OnError();
                return null;

            case DivisionIndexType.DividedIncludingTail:
                if (buf.Status == DivisionStatus.InFragment)
                {
                    buf.DataBuffer.AddRange(SliceTail(node, info, payloadData));
                    var completedData = buf.DataBuffer.ToArray();
                    buf.OnComplete();
                    var resultType = ResolveResultType(key, node, info);
                    return new ReassemblerResult(resultType, completedData);
                }
                buf.OnError();
                return null;

            default:
                return null;
        }
    }

    protected virtual void OnHeadCaptured(ushort key, PacketTreeNode node, PayloadInfo info, ReadOnlySpan<byte> payloadData) { }

    protected virtual ReadOnlySpan<byte> SliceHead(PacketTreeNode node, PayloadInfo info, ReadOnlySpan<byte> payloadData) => payloadData;
    protected virtual ReadOnlySpan<byte> SliceMiddle(PacketTreeNode node, PayloadInfo info, ReadOnlySpan<byte> payloadData) => payloadData;
    protected virtual ReadOnlySpan<byte> SliceTail(PacketTreeNode node, PayloadInfo info, ReadOnlySpan<byte> payloadData) => payloadData;

    protected abstract DivisionIndexType GetDivisionIndex(PacketTreeNode node);

    protected virtual Type ResolveResultType(ushort key, PacketTreeNode node, PayloadInfo info)
        => info.PayloadType ?? throw new InvalidDataException("PayloadType is null at completion.");
}