namespace MMTTLVParser.Parser.Reassemblers;

public class ReassemblerBuffer
{
    public List<byte> DataBuffer { get; } = [];
    public DivisionStatus Status { get; set; } = DivisionStatus.Initial;
    public List<PacketTreeNode> RelatedNodes { get; } = [];

    public void OnComplete()
    {
        DataBuffer.Clear();
        Status = DivisionStatus.Initial;
        RelatedNodes.Clear();
    }

    public void OnError()
    {
        foreach (var node in RelatedNodes)
        {
            node.Status = PacketStatusEnum.Error;
        }
        DataBuffer.Clear();
        Status = DivisionStatus.Initial;
        RelatedNodes.Clear();
    }
}
