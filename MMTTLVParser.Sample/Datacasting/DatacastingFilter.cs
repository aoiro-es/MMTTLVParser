using MMTTLVParser.PacketDefinitions.MMT;
using MMTTLVParser.PacketDefinitions.MMT.Table;
using MMTTLVParser.Parser;
using MMTTLVParser.Sample.Datacasting.Model;
using System.IO.Compression;
using static MMTTLVParser.Sample.Datacasting.IndexItem;

namespace MMTTLVParser.Sample.Datacasting;

public class DatacastingFilter(string fileName)
{
    private ApplicationInfoModel[]? _applicationInfos;
    private DataAssetManagementTable?[]? _damt;
    private DataDirectoryManagementTable?[]? _ddmt;
    private List<MPUDataNode> _targetFiles = new List<MPUDataNode>();

    // 厳密にはSeqNum以外にもAssetIDも含む必要がある
    private Dictionary<uint, IndexItem> _indexItems = new Dictionary<uint, IndexItem>();

    private string _name = fileName;

    private void OnDataTransmissionMessage(PacketTreeNode ptn)
    {
        var dtm = (DataTransmissionMessage)ptn.Data!;

        if (dtm.Table is DataDirectoryManagementTable ddmt)
        {
            // DataTransmissionSessionIdが更新された
            if (_ddmt is not null && _ddmt.Any(t => t is not null && t.DataTransmissionSessionId != ddmt.DataTransmissionSessionId))
                _ddmt = null;

            _ddmt ??= new DataDirectoryManagementTable[ddmt.LastSectionNumber + 1];
            _ddmt[ddmt.SectionNumber] ??= ddmt;
        }
        else if (dtm.Table is DataAssetManagementTable damt)
        {
            // DataTransmissionSessionIdが更新された
            if (_damt is not null && _damt.Any(t => t is not null && t.DataTransmissionSessionId != damt.DataTransmissionSessionId))
                _damt = null;

            _damt ??= new DataAssetManagementTable[damt.LastSectionNumber + 1];
            _damt[damt.SectionNumber] ??= damt;
        }

        // DAMTとDDMTが完成した
        if (_ddmt is not null && _damt is not null && _ddmt.All(t => t is not null) && _damt.All(t => t is not null))
        {
            // DAMTの記述子はMPU Node Descriptorしかない
            var mpuInfos = _damt.SelectMany(d => d!.MPUInfos)
                .Select(d => (Info: d, NodeDescriptor: d.MPUInfos.OfType<MPUNodeDescriptor>().FirstOrDefault())).ToArray();

            foreach (var cachedDdmt in _ddmt)
            {
                foreach (var directory in cachedDdmt!.DirectoryNodes)
                {
                    var absPath = Path.Combine(cachedDdmt!.BaseDirectoryPath, directory.DirectoryNodePath);
                    var mpuInfo = mpuInfos.FirstOrDefault(mi => mi.NodeDescriptor?.NodeTag == directory.NodeTag);
                    if (mpuInfo.NodeDescriptor is not null)
                    {
                        _targetFiles.Add(new MPUDataNode(absPath, mpuInfo.Info));
                    }
                }
            }
        }
    }

    private void OnM2SectionMessage(PacketTreeNode ptn)
    {
        var m2 = (M2SectionMessage)ptn.Data!;

        if (m2.Table is MHApplicationInformationTable ait)
        {
            if (_applicationInfos?.FirstOrDefault()?.MHAIT.VersionNumber != ait.VersionNumber)
            {
                _applicationInfos = ait.ApplicationInfos.Select(ai => new ApplicationInfoModel(ait, ai)).ToArray();
            }
        }
    }

    private void OnMMTPPacket(PacketTreeNode ptn)
    {
        if (ptn.Children.FirstOrDefault()?.Status != PacketStatusEnum.Complete)
        {
            return;
        }

        var mmtp = (MMTPPacket)ptn.Data!;

        // AssetIDのチェックが必要と思われる
        if (mmtp.PayloadType == MMTPPayloadType.MPU)
        {
            var mpuPayload = (MPUPayload)ptn.Children.Single().Data!;
            var targetFile = _targetFiles.FirstOrDefault(f => f.MPUInfo.MPUSequenceNumber == mpuPayload.MPUSequenceNumber);

            if (targetFile is not null)
            {
                var nonTimedMFUs = ptn.Children.Single().Children.Select(t => t.Data).OfType<NonTimedMFUPayload>().ToArray();
                var indexItemMFU = nonTimedMFUs.FirstOrDefault(m => m.ItemId == 0);

                // IndexItemを含む場合パースする
                if (indexItemMFU is not null)
                {
                    var indexItem = new IndexItem(indexItemMFU.MFUDataByte);
                    _indexItems.TryAdd(mpuPayload.MPUSequenceNumber, indexItem);
                }

                if (_indexItems.TryGetValue(mpuPayload.MPUSequenceNumber, out var cachedIndexItem))
                {
                    foreach (var mfu in nonTimedMFUs)
                    {
                        var targetItem = cachedIndexItem.Items.FirstOrDefault(i => i.ItemId == mfu.ItemId);
                        if (targetItem is not null)
                        {
                            var path = Path.Combine($"{_name}_datacasting", targetFile.Path, targetItem.FileName);
                            byte[] uncompressedData;
                            switch (targetItem.CompressionType)
                            {
                                case CompressionType.Zlib:
                                    using (var compressedMs = new MemoryStream(mfu.MFUDataByte))
                                    using (var zlibStream = new ZLibStream(compressedMs, CompressionMode.Decompress))
                                    using (var decompressedMs = new MemoryStream())
                                    {
                                        zlibStream.CopyTo(decompressedMs);
                                        uncompressedData = decompressedMs.ToArray();
                                    }
                                    break;
                                case CompressionType.Uncompressed:
                                    uncompressedData = mfu.MFUDataByte;
                                    break;
                                default:
                                    return;
                            }

                            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                            File.WriteAllBytes(path, uncompressedData);
                        }
                    }
                }
            }
        }
    }

    public void ProcessPackets(IEnumerable<PacketTreeNode> parentNodes)
    {
        foreach (var parent in parentNodes)
        {
            foreach (var node in parent.GetAllNodes())
            {
                var type = node.Data?.GetType();
                if (type == typeof(M2SectionMessage))
                {
                    OnM2SectionMessage(node);
                }
                else if (type == typeof(DataTransmissionMessage))
                {
                    OnDataTransmissionMessage(node);
                }
                else if (type == typeof(MMTPPacket))
                {
                    OnMMTPPacket(node);
                }
            }
        }
    }
}
