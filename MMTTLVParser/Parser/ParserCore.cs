using MMTTLVParser.PacketDefinitions;
using System.Buffers.Binary;

namespace MMTTLVParser.Parser;

public partial class Parser
{
    static private readonly Type _initialPacketType = typeof(TLVPacket);
    static private readonly byte[] _tlvPacketTypes = Enum.GetValues<TlvPacketType>().Select(v => (byte)v).ToArray();

    private readonly ParserFunctionController _parserFunctionController = new();
    private readonly ReassemblerController _reassemblerController = new();
    private readonly List<byte> _buffer = [];


    public List<PacketTreeNode>? Parse(ReadOnlySpan<byte> data)
    {
        byte[] concatenated = [.. _buffer, .. data];
        Span<byte> concatenatedSpan = concatenated.AsSpan();

        var pos = 0;
        var roots = new List<PacketTreeNode>();
        while (pos + 4 < concatenatedSpan.Length)
        {
            if (!IsValidTlvHeader(concatenatedSpan[pos..]))
            {
                var tlvStartPos = GetTlvStartPosition(concatenatedSpan[pos..]);
                if (tlvStartPos < 0)
                {
                    _buffer.Clear();
                    return null;
                }
                pos += tlvStartPos;
            }

            var length = BinaryPrimitives.ReadUInt16BigEndian(concatenatedSpan[(pos + 2)..(pos + 4)]);
            if (pos + length + 4 >= concatenatedSpan.Length)
            {
                // データの中にパケットがすべて含まれていない
                break;
            }

            var tlvPacketData = concatenatedSpan[pos..(pos + length + 4)];
            var root = new PacketTreeNode();
            CreatePacketNodes(root, _initialPacketType, tlvPacketData);
            roots.Add(root);
            pos += length + 4;
        }

        _buffer.RemoveRange(0, Math.Min(_buffer.Count, pos));
        _buffer.AddRange(concatenatedSpan[pos..]);
        return roots;
    }

    private void CreatePacketNodes(PacketTreeNode parent, Type packetType, ReadOnlySpan<byte> data)
    {
        var parserFunc = _parserFunctionController.GetParserFunctionDelegate(packetType);
        if (parserFunc is null)
        {
            parent.Status = PacketStatusEnum.Error;
            return;
        }

        ParserResultModel result;
        try
        {
            // ParserFunctionの中ではエラーはできる限り例外で処理する
            result = parserFunc(data);
        }
        catch
        {
            parent.Status = PacketStatusEnum.Error;
            return;
        }

        var node = new PacketTreeNode
        {
            Data = result.Header,
            Parent = parent,
            Status = result.Status
        };
        parent.Children.Add(node);

        if (result.Status == PacketStatusEnum.Fragmented)
        {
            var reassembler = _reassemblerController.GetReassembler(packetType);

            if (reassembler is null || result.Payloads is null || result.Payloads.Length != 1)
            {
                parent.Status = PacketStatusEnum.Error;
                return;
            }

            var payload = result.Payloads[0];
            var reassembledData = reassembler.Consume(node, payload, data);
            if (reassembledData is not null)
            {
                // 結合が完了した
                node.Status = PacketStatusEnum.Complete;
                CreatePacketNodes(node, reassembledData.PacketType, reassembledData.Data);
            }
            return;
        }


        if (result.Payloads is null)
        {
            return;
        }

        foreach (var payload in result.Payloads)
        {
            if (payload.PayloadType is null)
            {
                node.Status = PacketStatusEnum.Error;
                return;
            }
            CreatePacketNodes(node, payload.PayloadType, data[payload.Start..payload.End]);
        }
    }

    // MemoryExtensions.Containsのほうが最適化がかかっているので一度Spanを経由してから判定する
    public static bool IsValidTlvHeader(ReadOnlySpan<byte> data)
        => data[0] == 0x7f && _tlvPacketTypes.AsSpan().Contains(data[1]);

    public static int GetTlvStartPosition(ReadOnlySpan<byte> data)
    {
        for (int pos = 0; pos + 4 < data.Length; pos++)
        {
            var length = BinaryPrimitives.ReadUInt16BigEndian(data[(pos + 2)..(pos + 4)]);
            if (pos + length + 5 < data.Length
                && IsValidTlvHeader(data[pos..])
                && IsValidTlvHeader(data[(pos + length + 4)..]))
            {
                return pos;
            }
        }
        return -1;
    }
}
