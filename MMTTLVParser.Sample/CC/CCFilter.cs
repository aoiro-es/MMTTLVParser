using MMTTLVParser.PacketDefinitions.MMT;
using MMTTLVParser.PacketDefinitions.MMT.Table;
using MMTTLVParser.Parser;
using System.Buffers.Binary;
using System.Text;
using static MMTTLVParser.PacketDefinitions.MMT.Table.MHDataComponentDescriptor;

namespace MMTTLVParser.Sample.CC;

public class CCFilter(string fileName)
{
    private byte? _lastPltVersion;
    private byte? _lastMptVersion;
    private byte[]? _targetPackageId;
    private byte[]? _assetId;
    private uint? _lastSeqNum;
    private AdditionalAribSubtitleInfo? _additionalAribSubtitleInfo;

    private string _name = fileName;

    private List<byte[]> _mfuBuffer = [];

    private const string AssetType = "stpp";

    public void ProcessPackets(IEnumerable<PacketTreeNode> parentNodes)
    {
        foreach (var parent in parentNodes)
        {
            foreach (var node in parent.GetAllNodes())
            {
                var type = node.Data?.GetType();
                if (type == typeof(PAMessage))
                {
                    OnPAMessage(node);
                }
                else if (type == typeof(MMTPPacket))
                {
                    OnMMTPPacket(node);
                }
            }
        }
    }
    private void OnPAMessage(PacketTreeNode ptn)
    {
        var paMessage = (PAMessage)ptn.Data!;

        // 単一のPAMessageに複数のPLT、MPTが含まれることはない (ARIB STD-TR-B39)
        var plt = paMessage.TableInfos.OfType<PackageListTable>().FirstOrDefault();
        if (plt is not null)
        {
            if (plt.Version != _lastPltVersion || _targetPackageId is null)
            {
                _lastPltVersion = plt.Version;
                _targetPackageId = plt.PackageInfos.FirstOrDefault()?.MMTPackageId;
            }
        }

        var mpt = paMessage.TableInfos.OfType<MMTPackageTable>().FirstOrDefault();
        if (mpt is not null
            && _targetPackageId is not null
            && mpt.MMTPackageId.SequenceEqual(_targetPackageId)
            && mpt.Version != _lastMptVersion)
        {
            _lastMptVersion = mpt.Version;

            // 字幕は複数あるかも
            // TODO: subtitle_tagを参照したほうが良い
            var ccAssetInfo = mpt.AssetInfos.FirstOrDefault(ai => ai.AssetType == AssetType);
            _assetId = ccAssetInfo?.AssetId;

            var subtitleInfoFromMpt = ccAssetInfo?.AssetDescriptors.OfType<MHDataComponentDescriptor>()
                .Select(d => d.AdditionalDataComponentInfo).OfType<AdditionalAribSubtitleInfo>().FirstOrDefault();

            if (subtitleInfoFromMpt is not null)
            {
                _additionalAribSubtitleInfo = subtitleInfoFromMpt;
            }
        }
    }

    private void OnMMTPPacket(PacketTreeNode ptn)
    {
        if (ptn.Children.FirstOrDefault()?.Status != PacketStatusEnum.Complete)
        {
            return;
        }

        var mmtpPacket = (MMTPPacket)ptn.Data!;

        Span<byte> packetIdBytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(packetIdBytes, mmtpPacket.PacketId);

        if (mmtpPacket.PayloadType == MMTPPayloadType.MPU)
        {
            if (_assetId is not null && packetIdBytes.SequenceEqual(_assetId))
            {
                var mpuPayload = (MPUPayload)ptn.Children.Single().Data!;
                var timedMFUs = ptn.Children.Single().Children.Select(t => t.Data).OfType<TimedMFUPayload>();

                _mfuBuffer.AddRange(timedMFUs.Select(m => m.MFUDataByte));
                if (_mfuBuffer.Count > 0 && new CCMFUDataByte(_mfuBuffer[0]).LastSubsampleNumber == _mfuBuffer.Count - 1)
                {
                    WriteCC();
                }

                // 新しいMPUが到着した
                if (_lastSeqNum != mpuPayload.MPUSequenceNumber)
                {
                    _mfuBuffer.Clear();
                    _lastSeqNum = mpuPayload.MPUSequenceNumber;
                }
            }
        }
    }


    private void WriteCC()
    {
        if (_mfuBuffer.Count == 0 || _additionalAribSubtitleInfo is null)
        {
            return;
        }

        // ttml文章は必ず最初のMFUに含まれる
        var firstMfu = new CCMFUDataByte(_mfuBuffer[0]);

        // サンプルが足りない場合は保存しない
        if (firstMfu.LastSubsampleNumber != _mfuBuffer.Count - 1)
        {
            return;
        }

        var ttmlDocumentStr = Encoding.UTF8.GetString(firstMfu.DataByte);
        File.AppendAllText($"{_name}.ttml", ttmlDocumentStr);
    }
}