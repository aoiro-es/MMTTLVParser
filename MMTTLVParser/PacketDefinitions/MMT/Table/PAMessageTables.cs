using System.Buffers.Binary;
using System.Net;
using System.Text;

namespace MMTTLVParser.PacketDefinitions.MMT.Table;

public abstract record PAMessageTable : Table;

public abstract record MMTGeneralLocationInfo
{
    public byte LocationType { get; init; }

    /// <summary>
    /// MMTGeneralLocationInfoとその長さを返す
    /// 長さは種別から判断するので切り出して渡す必要はない
    /// </summary>
    public static (MMTGeneralLocationInfo, int Lnegth) ParseMMTGeneralLocationInfo(ReadOnlySpan<byte> data)
        => data[0] switch
        {
            0x00 => (new SameIpMMTGeneralLocationInfo(data[..3]), 3),
            0x01 => (new IPv4MMTGeneralLocationInfo(data[..13]), 13),
            0x02 => (new IPv6MMTGeneralLocationInfo(data[..37]), 37),
            0x03 => (new MPEG2TSOfBroadcastingGeneralLocationInfo(data[..7]), 7),
            0x04 => (new MPEG2TSOfIpv6DataFlowGeneralLocationInfo(data[..37]), 37),
            0x05 => (new UrlGeneralLocationInfo(data[..(data[0 + 1] + 2)]), data[0 + 1] + 2),
            _ => throw new InvalidDataException()
        };
}

public record SameIpMMTGeneralLocationInfo : MMTGeneralLocationInfo
{
    public ushort PacketId { get; init; }
    public SameIpMMTGeneralLocationInfo(ReadOnlySpan<byte> data)
    {
        LocationType = data[0];
        PacketId = BinaryPrimitives.ReadUInt16BigEndian(data[1..3]);
    }
}

public record IPv4MMTGeneralLocationInfo : MMTGeneralLocationInfo
{
    public IPAddress Ipv4SrcAddr { get; init; }
    public IPAddress Ipv4DstAddr { get; init; }
    public ushort DstPort { get; init; }
    public ushort PacketId { get; init; }
    public IPv4MMTGeneralLocationInfo(ReadOnlySpan<byte> data)
    {
        LocationType = data[0];
        Ipv4SrcAddr = new IPAddress(data[1..5]);
        Ipv4DstAddr = new IPAddress(data[5..9]);
        DstPort = BinaryPrimitives.ReadUInt16BigEndian(data[9..11]);
        PacketId = BinaryPrimitives.ReadUInt16BigEndian(data[11..13]);
    }
}

public record IPv6MMTGeneralLocationInfo : MMTGeneralLocationInfo
{
    public IPAddress Ipv6SrcAddr { get; init; }
    public IPAddress Ipv6DstAddr { get; init; }
    public ushort DstPort { get; init; }
    public ushort PacketId { get; init; }
    public IPv6MMTGeneralLocationInfo(ReadOnlySpan<byte> data)
    {
        LocationType = data[0];
        Ipv6SrcAddr = new IPAddress(data[1..17]);
        Ipv6DstAddr = new IPAddress(data[17..33]);
        DstPort = BinaryPrimitives.ReadUInt16BigEndian(data[33..35]);
        PacketId = BinaryPrimitives.ReadUInt16BigEndian(data[35..37]);
    }
}

public record MPEG2TSOfBroadcastingGeneralLocationInfo : MMTGeneralLocationInfo
{
    public ushort NetworkId { get; init; }
    public ushort Mpeg2TransportStreamId { get; init; }
    public ushort Mpeg2Pid { get; init; }
    public MPEG2TSOfBroadcastingGeneralLocationInfo(ReadOnlySpan<byte> data)
    {
        LocationType = data[0];
        NetworkId = BinaryPrimitives.ReadUInt16BigEndian(data[1..3]);
        Mpeg2TransportStreamId = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        Mpeg2Pid = (ushort)((data[5] & 0x1f) << 8 | data[6]);
    }
}

public record MPEG2TSOfIpv6DataFlowGeneralLocationInfo : MMTGeneralLocationInfo
{
    public IPAddress Ipv6SrcAddr { get; init; }
    public IPAddress Ipv6DstAddr { get; init; }
    public ushort DstPort { get; init; }
    public ushort Mpeg2Pid { get; init; }
    public MPEG2TSOfIpv6DataFlowGeneralLocationInfo(ReadOnlySpan<byte> data)
    {
        LocationType = data[0];
        Ipv6SrcAddr = new IPAddress(data[1..17]);
        Ipv6DstAddr = new IPAddress(data[17..33]);
        DstPort = BinaryPrimitives.ReadUInt16BigEndian(data[33..35]);
        Mpeg2Pid = (ushort)((data[35] & 0x1f) << 8 | data[36]);
    }
}

public record UrlGeneralLocationInfo : MMTGeneralLocationInfo
{
    public Uri Url { get; init; }
    public UrlGeneralLocationInfo(ReadOnlySpan<byte> data)
    {
        LocationType = data[0];
        var length = data[1];
        Url = new Uri(Encoding.UTF8.GetString(data[2..(2 + length)]));
    }
}

public record MMTPackageTable : PAMessageTable
{
    public byte TableId { get; init; }
    public byte Version { get; init; }
    public ushort Length { get; init; }
    public MPTModeType MPTMode { get; init; }
    public byte[] MMTPackageId { get; init; }
    public byte[] MPTDescriptors { get; init; }
    public byte NumberOfAssets { get; init; }
    public AssetInfo[] AssetInfos { get; init; }

    public record AssetInfo(byte IdentifierType, uint AssetIdScheme, byte[] AssetId,
        string AssetType, bool AssetClockRelationFlag, MMTGeneralLocationInfo[] MMTGeneralLocationInfo,
        Descriptor[] AssetDescriptors);

    public enum MPTModeType : byte
    {
        ProcessedAccordingToTheOrderOfSubset = 0b00,
        AnySubsetWithTheSameVersion = 0b01,
        Arbitrarily = 0b10
    }

    public MMTPackageTable(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        Version = data[1];
        Length = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);
        MPTMode = (MPTModeType)((data[4] & 0x03) >> 6);

        var mmtPackageIdLength = data[5];
        var pos = 6;
        MMTPackageId = data[pos..(pos + mmtPackageIdLength)].ToArray();
        var mptDescriptorsLength = BinaryPrimitives.ReadUInt16BigEndian(data[(pos + mmtPackageIdLength)..(pos + mmtPackageIdLength + 2)]);
        pos += mmtPackageIdLength + 2;
        MPTDescriptors = data[pos..(pos + mptDescriptorsLength)].ToArray();
        pos += mptDescriptorsLength;
        NumberOfAssets = data[pos];

        var assetInfos = new List<AssetInfo>();
        pos++;
        for (int i = 0; i < NumberOfAssets; i++)
        {
            var identifierType = data[pos];
            var assetIdScheme = BinaryPrimitives.ReadUInt32BigEndian(data[(pos + 1)..(pos + 5)]);
            var assetIdLength = data[pos + 5];
            var assetIdByte = data[(pos + 6)..(pos + 6 + assetIdLength)].ToArray();

            pos += 6 + assetIdLength;
            var assetType = Encoding.UTF8.GetString(data[pos..(pos + 4)]);
            var assetClockRelationFlag = Convert.ToBoolean(data[pos + 4] & 0x01);
            var locationCount = data[pos + 5];

            pos += 6;
            var locationInfos = new List<MMTGeneralLocationInfo>();
            for (int j = 0; j < locationCount; j++)
            {
                var (info, length) = MMTGeneralLocationInfo.ParseMMTGeneralLocationInfo(data[pos..]);
                pos += length;
                locationInfos.Add(info);
            }
            var assetDescriptorsLength = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var assetDescriptors = Descriptor.ParseDescriptors(data[(pos + 2)..(pos + 2 + assetDescriptorsLength)], typeof(MMTPackageTable));
            assetInfos.Add(new AssetInfo(identifierType, assetIdScheme, assetIdByte, assetType, assetClockRelationFlag, locationInfos.ToArray(), assetDescriptors));
            pos += 2 + assetDescriptorsLength;
        }
        AssetInfos = assetInfos.ToArray();
    }
}

public record PackageListTable : PAMessageTable
{
    public record PackageInfo(byte[] MMTPackageId, MMTGeneralLocationInfo MMTGeneralLocationInfo);
    public abstract record LocationInfo
    {
        public static (LocationInfo, int Length) ParseLocationInfo(int LocationType, ReadOnlySpan<byte> data)
        => LocationType switch
        {
            0x01 => (new Ipv4LocationInfo(new IPAddress(data[..4]),
                        new IPAddress(data[4..8]),
                        BinaryPrimitives.ReadUInt16BigEndian(data[8..10])), 10),
            0x02 => (new Ipv6Locationinfo(new IPAddress(data[..16]),
                        new IPAddress(data[16..32]),
                        BinaryPrimitives.ReadUInt16BigEndian(data[32..34])), 34),
            0x05 => (new UrlLocationInfo(new Uri(Encoding.UTF8.GetString(data[1..(data[0] + 1)]))), data[0] + 1),
            _ => throw new InvalidDataException()
        };
    }
    public record Ipv4LocationInfo(IPAddress Ipv4SrcAddr, IPAddress Ipv4DstAddr, ushort DstPort) : LocationInfo;
    public record Ipv6Locationinfo(IPAddress Ipv6SrcAddr, IPAddress Ipv6DstAddr, ushort DstPort) : LocationInfo;
    public record UrlLocationInfo(Uri Url) : LocationInfo;
    public record IpDeliveryInfo(uint TransportFileId, byte LocationType, LocationInfo Location, Descriptor Descriptor);

    public byte TableId { get; init; }
    public byte Version { get; init; }
    public ushort Length { get; init; }
    public PackageInfo[] PackageInfos { get; init; }
    public IpDeliveryInfo[] IpDeliveryInfos { get; init; }

    public PackageListTable(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        Version = data[1];
        Length = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);
        var numOfPackage = data[4];
        var pos = 5;
        var packageInfos = new List<PackageInfo>();
        for (int i = 0; i < numOfPackage; i++)
        {
            var mmtPackageIdLength = data[pos];
            var mmtPackageId = data[(pos + 1)..(pos + 1 + mmtPackageIdLength)].ToArray();
            pos += 1 + mmtPackageIdLength;
            var (locationInfo, length) = MMTGeneralLocationInfo.ParseMMTGeneralLocationInfo(data[pos..]);
            pos += length;
            packageInfos.Add(new PackageInfo(mmtPackageId, locationInfo));
        }
        PackageInfos = packageInfos.ToArray();

        var numOfIpDelivery = data[pos];
        var ipDeliveryInfos = new List<IpDeliveryInfo>();
        pos++;
        for (int i = 0; i < numOfIpDelivery; i++)
        {
            var transportFileId = BinaryPrimitives.ReadUInt32BigEndian(data[pos..(pos + 4)]);
            var locationType = data[pos + 5];
            var (location, length) = LocationInfo.ParseLocationInfo(data[pos + 6], data[(pos + 7)..]);
            pos += length + 7;
            var descriptorLoopLength = data[pos];

            // Descriptorは標準文書には記述されていない
            var descriptor = data[(pos + 1)..(pos + 1 + descriptorLoopLength)];

            // ipDeliveryInfos.Add(new IpDeliveryInfo(transportFileId, locationType, location, null));
            pos += 1 + descriptorLoopLength;
        }
        IpDeliveryInfos = ipDeliveryInfos.ToArray();
    }
}

// 実際には使われない
public record LayoutConfigurationTable : PAMessageTable
{
    public record RegionInfo(byte RegionNumber, byte LeftTopPosX, byte LeftTopPosY, byte RightDownPosX, byte RightDownPosY, byte LayerOrder);
    public record LayoutInfo(byte LayoutNumber, byte DeviceId, RegionInfo[] RegionInfos);

    public byte TableId { get; init; }
    public byte Version { get; init; }
    public ushort Length { get; init; }
    public LayoutInfo[] LayoutInfos { get; init; }
    public Descriptor[] Descriptors { get; init; }

    public LayoutConfigurationTable(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        Version = data[1];
        Length = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);
        var numberOfLoop = data[4];
        var pos = 5;

        var layouts = new List<LayoutInfo>();
        for (int i = 0; i < numberOfLoop; i++)
        {
            var layoutNumber = data[pos];
            var deviceId = data[pos + 1];
            var numberOfRegion = data[pos + 2];
            pos += 3;
            var regions = new List<RegionInfo>();
            for (int j = 0; j < numberOfRegion; j++)
            {
                regions.Add(new RegionInfo(data[pos], data[pos + 1], data[pos + 2], data[pos + 3], data[pos + 4], data[pos + 5]));
                pos += 6;
            }
            layouts.Add(new LayoutInfo(layoutNumber, deviceId, regions.ToArray()));
        }
        LayoutInfos = layouts.ToArray();

        Descriptors = Descriptor.ParseDescriptors(data[pos..(4 + Length)], typeof(LayoutConfigurationTable));
    }
}