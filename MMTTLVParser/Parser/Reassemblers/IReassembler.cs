namespace MMTTLVParser.Parser.Reassemblers;

public interface IReassembler
{
    ReassemblerResult? Consume(PacketTreeNode node, PayloadInfo info, ReadOnlySpan<byte> data);
}