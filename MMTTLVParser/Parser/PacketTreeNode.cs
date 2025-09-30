using MMTTLVParser.PacketDefinitions;

namespace MMTTLVParser.Parser;

public class PacketTreeNode : IDisposable
{
    private bool _disposed = false;

    /// <summary>
    /// ヘッダのデータ
    /// </summary>
    public Packet? Data { get; set; }

    /// <summary>
    /// 上位階層のパケット
    /// </summary>
    public PacketTreeNode? Parent { get; set; }

    /// <summary>
    /// 下位階層のパケット
    /// </summary>
    public List<PacketTreeNode> Children { get; set; } = [];

    public PacketStatusEnum Status { get; set; }

    public IEnumerable<PacketTreeNode> GetAllNodes()
    {
        yield return this;

        foreach (var child in Children)
        {
            foreach (var descendant in child.GetAllNodes())
            {
                yield return descendant;
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Data = null;
            Parent = null;
            foreach (var child in Children)
            {
                child.Dispose();
            }
            Children.Clear();
            _disposed = true;
        }
    }
}
