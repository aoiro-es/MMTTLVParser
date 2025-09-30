using System.Buffers.Binary;
using System.Net;
using System.Text;
using static MMTTLVParser.PacketDefinitions.MMT.Table.MHCommonDataTable.LogoDataByte;

namespace MMTTLVParser.PacketDefinitions.MMT.Table;

public abstract record Descriptor
{
    public ushort DescriptorTag { get; init; }
    public ushort DescriptorLength { get; init; }

    /// <summary>
    /// DescriptorTagとDescriptorLengthのバイト数
    /// </summary>
    public int TagAndLengthBytes { get; init; } = 3;

    public static Descriptor[] ParseMHBITDescriptors(ReadOnlySpan<byte> data, bool isFirstLoop)
    {
        var pos = 0;
        var descriptors = new List<Descriptor>();
        while (pos < data.Length)
        {
            var tag = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var descData = data[pos..];
            Descriptor? descriptor;

            // MH-BIT
            descriptor = tag switch
            {
                0x800d => new MHServiceListDescriptor(descData),
                0x8017 => new MHSIParameterDescriptor(descData, isFirstLoop),
                0x8018 => new MHBroadcasterNameDescriptor(descData),
                0x803e => new RelatedBroadcasterDescriptor(descData),
                _ => null
            };
            if (descriptor is null)
            {
                break;
            }
            descriptors.Add(descriptor);

            pos += descriptor.TagAndLengthBytes + descriptor.DescriptorLength;
        }
        return descriptors.ToArray();
    }

    public static Descriptor[] ParseDescriptors(ReadOnlySpan<byte> data, Type tableType)
    {
        var pos = 0;
        var descriptors = new List<Descriptor>();
        while (pos < data.Length)
        {
            var tag = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var descData = data[pos..];
            Descriptor? descriptor;
            if (tableType == typeof(MMTPackageTable))
            {
                // MPT
                descriptor = tag switch
                {
                    0x0001 => new MPUTimestampDescriptor(descData),
                    0x0002 => new DependencyDescriptor(descData),
                    0x8000 => new AssetGroupDescriptor(descData),
                    0x8003 => new MPUPresentationRegionDescriptor(descData),
                    0x8004 => new AccessControlDescriptor(descData),
                    0x8005 => new ScramblerDescriptor(descData),
                    0x8006 => new MessageAuthenticationMethodDescriptor(descData),
                    0x8007 => new EmergencyInformationDescriptor(descData),
                    0x8008 => new MHMPEG4AudioDescriptor(descData),
                    0x8009 => new MHMPEG4AudioExtensionDescriptor(descData),
                    0x800a => new MHHEVCDescriptor(descData),
                    0x8010 => new VideoComponentDescriptor(descData),
                    0x8011 => new MHStreamIdentifierDescriptor(descData),
                    0x8013 => new MHParentalRatingDescriptor(descData),
                    0x8014 => new MHAudioComponentDescriptor(descData),
                    0x8015 => new MHTargetRegionDescriptor(descData),
                    0x801b => new MHCAStartupDescriptor(descData),
                    0x8020 => new MHDataComponentDescriptor(descData),
                    0x8026 => new MPUExtendedTimestampDescriptor(descData),
                    0x8033 => new MHDownloadProtectionDescriptor(descData),
                    0x8034 => new ApplicationServiceDescriptor(descData),
                    0x8037 => new MHHierarchyDescriptor(descData),
                    0x8038 => new ContentCopyControlDescriptor(descData),
                    0x8039 => new ContentUsageControlDescriptor(descData),
                    0x8040 => new EmergencyNewsDescriptor(descData),
                    0xf004 => new MHStuffingDescriptor(descData),
                    0xf000 => new MHLinkageDescriptor(descData),
                    _ => null
                };
            }
            else if (tableType == typeof(CATable))
            {
                // CAT(MH)
                descriptor = tag switch
                {
                    0x8004 => new AccessControlDescriptor(descData),
                    0x8005 => new ScramblerDescriptor(descData),
                    0x8006 => new MessageAuthenticationMethodDescriptor(descData),
                    0x801b => new MHCAStartupDescriptor(descData),
                    0x8042 => new MHCAServiceDescriptor(descData),
                    _ => null
                };
            }
            else if (tableType == typeof(MHEventInformationTable))
            {
                // MH-EIT
                descriptor = tag switch
                {
                    0x8001 => new EventPackageDescriptor(descData),
                    0x800c => new MHEventGroupDescriptor(descData),
                    0x8010 => new VideoComponentDescriptor(descData),
                    0x8012 => new MHContentDescriptor(descData),
                    0x8013 => new MHParentalRatingDescriptor(descData),
                    0x8014 => new MHAudioComponentDescriptor(descData),
                    0x8016 => new MHSeriesDescriptor(descData),
                    0x8024 => new MHComponentGroupDescriptor(descData),
                    0x8038 => new ContentCopyControlDescriptor(descData),
                    0x8039 => new ContentUsageControlDescriptor(descData),
                    0x803f => new MultimediaServiceInfoDescriptor(descData),
                    0x8041 => new MHCAContractInfoDescriptor(descData),
                    0xf000 => new MHLinkageDescriptor(descData),
                    0xf001 => new MHShortEventDescriptor(descData),
                    0xf002 => new MHExtendedEventDescriptor(descData),
                    _ => null
                };
            }
            else if (tableType == typeof(MHBroadcasterInformationTable))
            {
                // MH-BIT
                // 専用の関数を使うこと
                throw new NotImplementedException();
            }
            else if (tableType == typeof(MHSoftwareDownloadTriggerTable))
            {
                // MH-SDTT
                descriptor = tag switch
                {
                    0x8027 => new MPUDownloadContentDescriptor(descData),
                    0x8028 => new MHNetworkDownloadContentDescriptor(descData),
                    0x8033 => new MHDownloadProtectionDescriptor(descData),
                    _ => null
                };
            }
            else if (tableType == typeof(MHServiceDescriptionTable))
            {
                // MH-SDT
                descriptor = tag switch
                {
                    0xf000 => new MHLinkageDescriptor(descData),
                    0x8019 => new MHServiceDescriptor(descData),
                    0x801a => new IPDataFlowDescriptor(descData),
                    0x8025 => new MHLogoTransmittionDescriptor(descData),
                    0x8038 => new ContentCopyControlDescriptor(descData),
                    0x8039 => new ContentUsageControlDescriptor(descData),
                    0x8041 => new MHCAContractInfoDescriptor(descData),
                    _ => null
                };
            }
            else if (tableType == typeof(LayoutConfigurationTable))
            {
                // LCT
                descriptor = tag switch
                {
                    0x8002 => new BackgroundColorDescriptor(descData),
                    _ => null
                };
            }
            else if (tableType == typeof(MHTimeOffsetTable))
            {
                // MH-TOT
                descriptor = tag switch
                {
                    0x8023 => new MHLocalTimeOffsetDescriptor(descData),
                    _ => null
                };
            }
            else if (tableType == typeof(MHSelectionInformationTable))
            {
                // MH-SIT
                descriptor = tag switch
                {
                    0x800c => new MHEventGroupDescriptor(descData),
                    0x8010 => new VideoComponentDescriptor(descData),
                    0x8012 => new MHContentDescriptor(descData),
                    0x8013 => new MHParentalRatingDescriptor(descData),
                    0x8014 => new MHAudioComponentDescriptor(descData),
                    0x8016 => new MHSeriesDescriptor(descData),
                    0x8018 => new MHBroadcasterNameDescriptor(descData),
                    0x8019 => new MHServiceDescriptor(descData),
                    0x803f => new MultimediaServiceInfoDescriptor(descData),
                    0xf001 => new MHShortEventDescriptor(descData),
                    0xf002 => new MHExtendedEventDescriptor(descData),
                    0xf004 => new MHStuffingDescriptor(descData),
                    0xf005 => new MHBroadcastIDDescriptor(descData),
                    0xf006 => new MHNetworkIdentificationDescriptor(descData),
                    _ => null
                };
            }
            else if (tableType == typeof(MHApplicationInformationTable))
            {
                descriptor = tag switch
                {
                    0x8029 => new MHApplicationDescriptor(descData),
                    0x802a => new MHTransportProtocolDescriptor(descData),
                    0x802b => new MHSimpleApplicationLocationDescriptor(descData),
                    0x802c => new MHApplicationBoundaryAndPermissionDescriptor(descData),
                    0x802d => new MHAutostartPriorityDescriptor(descData),
                    0x802e => new MHCacheControlInfoDescriptor(descData),
                    0x802f => new MHRandomizedLatencyDescriptor(descData),
                    0x803a => new MHExternalApplicationControlDescriptor(descData),
                    0x803b => new MHPlaybackApplicationDescriptor(descData),
                    0x803c => new MHSimplePlaybackApplicationLocationDescriptor(descData),
                    0x803d => new MHApplicationExpirationDescriptor(descData),
                    _ => null
                };
            }
            else if (tableType == typeof(DataAssetManagementTable))
            {
                // DAMT
                descriptor = tag switch
                {
                    0x801c => new MHTypeDescriptor(descData),
                    0x801d => new MHInfoDescriptor(descData),
                    0x801e => new MHExpireDescriptor(descData),
                    0x801f => new MHCompressionTypeDescriptor(descData),
                    0x8035 => new MPUNodeDescriptor(descData),
                    _ => null
                };
            }
            else if (tableType == typeof(DataContentConfigurationTable))
            {
                // DCCT
                descriptor = tag switch
                {
                    0x8030 => new LinkedPUDescriptor(descData),
                    0x8031 => new LockedCacheDescriptor(descData),
                    0x8032 => new UnlockedCacheDescriptor(descData),
                    0x8036 => new PUStructureDescriptor(descData),
                    _ => null
                };
            }
            else
            {
                throw new InvalidDataException();
            }

            if (descriptor is null)
            {
                break;
            }

            pos += descriptor.TagAndLengthBytes + descriptor.DescriptorLength;
            descriptors.Add(descriptor);
        }
        return descriptors.ToArray();
    }
}

public record AssetGroupDescriptor : Descriptor
{
    public byte GroupIdentification { get; init; }
    public byte SelectionLevel { get; init; }

    public AssetGroupDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        GroupIdentification = data[3];
        SelectionLevel = data[4];
    }
}

public record EventPackageDescriptor : Descriptor
{
    public byte[] MMTPackageIdByte { get; init; }
    public EventPackageDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        var length = data[3];
        MMTPackageIdByte = data[4..(4 + length)].ToArray();
    }
}

public record BackgroundColorDescriptor : Descriptor
{
    public byte[] BackgroundColor { get; init; }

    public BackgroundColorDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        BackgroundColor = data[3..6].ToArray();
    }
}

public record MPUPresentationRegionDescriptor : Descriptor
{
    public record RegionInfo(uint MPUSequenceNumber, byte LayoutNumber, byte RegionNumber);
    RegionInfo[] RegionInfos { get; init; }

    public MPUPresentationRegionDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];

        var pos = 3;
        var regions = new List<RegionInfo>();
        while (pos < 3 + DescriptorLength)
        {
            var mpuSequneceNumber = BinaryPrimitives.ReadUInt32BigEndian(data[pos..(pos + 4)]);
            var layoutNumber = data[pos + 4];
            var regionNumber = data[pos + 5];
            var lengthOfReserved = data[pos + 6];
            regions.Add(new RegionInfo(mpuSequneceNumber, layoutNumber, regionNumber));

            pos += 7 + lengthOfReserved;
        }
        RegionInfos = regions.ToArray();
    }
}

public record MPUTimestampDescriptor : Descriptor
{
    public record Timestamp(uint MPUSequenceNumber, ulong MPUPresentationTime);
    public Timestamp[] Timestamps { get; init; }

    public MPUTimestampDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];

        var pos = 3;
        var timestamps = new List<Timestamp>();
        while (pos < 3 + DescriptorLength)
        {
            var mpuSequenceNumber = BinaryPrimitives.ReadUInt32BigEndian(data[pos..(pos + 4)]);
            // NTP format
            var mpuPresentationTime = BinaryPrimitives.ReadUInt64BigEndian(data[(pos + 4)..(pos + 12)]);
            timestamps.Add(new Timestamp(mpuSequenceNumber, mpuPresentationTime));
            pos += 12;
        }
        Timestamps = timestamps.ToArray();
    }
}

public record DependencyDescriptor : Descriptor
{
    public record DependencyInfo(uint AssetIdScheme, byte[] AssetIdByte);
    public DependencyInfo[] DependencyInfos { get; init; }

    public DependencyDescriptor(ReadOnlySpan<byte> data)
    {
        TagAndLengthBytes = 4;

        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);
        var numDependencies = data[4];

        DependencyInfos = new DependencyInfo[numDependencies];
        var pos = 5;
        for (var i = 0; i < numDependencies; i++)
        {
            var assetIdScheme = BinaryPrimitives.ReadUInt32BigEndian(data[pos..(pos + 4)]);
            var assetIdLength = data[pos + 4];
            var assetIdByte = data[(pos + 5)..(pos + 5 + assetIdLength)].ToArray();
            DependencyInfos[i] = new DependencyInfo(assetIdScheme, assetIdByte);
            pos += 5 + assetIdLength;
        }
    }
}

public record AccessControlDescriptor : Descriptor
{
    public ushort CASystemId { get; init; }
    public MMTGeneralLocationInfo MMTGeneralLocationInfo { get; init; }
    public byte[] PrivateData { get; init; }

    public AccessControlDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        CASystemId = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        var (mgli, mgliLength) = MMTGeneralLocationInfo.ParseMMTGeneralLocationInfo(data[5..]);
        MMTGeneralLocationInfo = mgli;
        PrivateData = data[(5 + mgliLength)..(3 + DescriptorLength)].ToArray();
    }
}

public enum LayerTypeEnum : byte
{
    MMTPPacket = 0b01,
    IPPacket = 0b10
}

public record ScramblerDescriptor : Descriptor
{
    public enum ScrambleSystemType : byte
    {
        AESWith128bitKey = 0b00000001,
        CamelliaWith128bitKey = 0b00000010
    }

    public LayerTypeEnum LayerType { get; init; }
    public ScrambleSystemType ScrambleSystemId { get; init; }
    public byte[] PrivateData { get; init; }

    public ScramblerDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        LayerType = (LayerTypeEnum)((data[3] & 0xc0) >> 6);
        ScrambleSystemId = (ScrambleSystemType)data[4];
        PrivateData = data[5..(3 + DescriptorLength)].ToArray();
    }
}

public record MessageAuthenticationMethodDescriptor : Descriptor
{
    public LayerTypeEnum LayerType { get; init; }
    public byte MessageAuthenticationSystemId { get; init; }
    public byte[] PrivateData { get; init; }

    public MessageAuthenticationMethodDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        LayerType = (LayerTypeEnum)((data[3] & 0xc0) >> 6);
        MessageAuthenticationSystemId = data[4];
        PrivateData = data[5..(3 + DescriptorLength)].ToArray();
    }
}

public record EmergencyInformationDescriptor : Descriptor
{
    public record EmergencyInfo(ushort ServiceId, bool StartEndFlag, bool SignalLevel, ushort[] AreaCodes);

    public EmergencyInfo[] EmergencyInfos { get; init; }

    public EmergencyInformationDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        var pos = 3;
        var emergencyInfos = new List<EmergencyInfo>();
        while (pos < 3 + DescriptorLength)
        {
            var serviceId = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var startEndFlag = Convert.ToBoolean((data[pos + 2] & 0x80) >> 7);
            var signalLevel = Convert.ToBoolean((data[pos + 2] & 0x40) >> 6);
            var areaCodeLength = data[pos + 3];

            var areaCodes = new List<ushort>();
            foreach (var areaCodeByte in data[(pos + 4)..(pos + 4 + areaCodeLength)].ToArray().Chunk(2))
            {
                areaCodes.Add((ushort)((areaCodeByte[0] << 4) | ((areaCodeByte[1] & 0xf0) >> 4)));
            }
            emergencyInfos.Add(new EmergencyInfo(serviceId, startEndFlag, signalLevel, areaCodes.ToArray()));
            pos += 4 + areaCodeLength;
        }
        EmergencyInfos = emergencyInfos.ToArray();
    }
}

public record MHMPEG4AudioDescriptor : Descriptor
{
    public byte MPEG4AudioProfileAndLevel { get; init; }
    public MHMPEG4AudioDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        MPEG4AudioProfileAndLevel = data[3];
    }
}

public record MHMPEG4AudioExtensionDescriptor : Descriptor
{
    public bool ASCFlag { get; init; }
    public byte[] AudioProfileLevelIndication { get; init; }
    public byte[]? AudioSpecificConfig { get; init; }

    public MHMPEG4AudioExtensionDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        ASCFlag = Convert.ToBoolean((data[3] & 0x80) >> 7);
        var numOfLoops = data[3] & 0x0f;
        AudioProfileLevelIndication = data[4..(4 + numOfLoops)].ToArray();
        if (ASCFlag)
        {
            var ascSize = data[4 + numOfLoops];
            AudioSpecificConfig = data[(5 + numOfLoops)..(5 + numOfLoops + ascSize)].ToArray();
        }
    }
}

public record MHHEVCDescriptor : Descriptor
{
    public byte ProfileSpace { get; init; }
    public bool TimerFlag { get; init; }
    public byte ProfileIdc { get; init; }
    public uint ProfileCompatibilityIndication { get; init; }
    public bool ProgressiveSourceFlag { get; init; }
    public bool InterlacedSourceFlag { get; init; }
    public bool NonPackedConstraintFlag { get; init; }
    public bool FrameOnlyConstraintFlag { get; init; }
    public ulong Copied44Bits { get; init; }
    public byte LevelIdc { get; init; }
    public bool TemporalLayerSubsetFlag { get; init; }
    public bool HEVCStillPresentFlag { get; init; }
    public bool HEVC24hrPicturePresentFlag { get; init; }
    public bool SubPicHrdParamsNotPresentFlag { get; init; }
    public byte HDRWCGIdc { get; init; }
    public byte? TemporalIdMin { get; init; }
    public byte? TemporalIdMax { get; init; }

    public MHHEVCDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        ProfileSpace = (byte)((data[3] & 0xc0) >> 6);
        TimerFlag = Convert.ToBoolean((data[3] & 0x20) >> 5);
        ProfileIdc = (byte)(data[3] & 0x1f);
        ProfileCompatibilityIndication = BinaryPrimitives.ReadUInt32BigEndian(data[4..8]);
        ProgressiveSourceFlag = Convert.ToBoolean((data[8] & 0x80) >> 7);
        InterlacedSourceFlag = Convert.ToBoolean((data[8] & 0x40) >> 6);
        NonPackedConstraintFlag = Convert.ToBoolean((data[8] & 0x20) >> 5);
        FrameOnlyConstraintFlag = Convert.ToBoolean((data[8] & 0x10) >> 4);
        Copied44Bits = (((ulong)data[8] & 0x0f) << 40) | ((ulong)data[9] << 32) | ((ulong)data[10] << 24) | ((ulong)data[11] << 16) | ((ulong)data[12] << 8) | data[13];
        LevelIdc = data[14];
        TemporalLayerSubsetFlag = Convert.ToBoolean((data[15] & 0x80) >> 7);
        HEVCStillPresentFlag = Convert.ToBoolean((data[15] & 0x40) >> 6);
        HEVC24hrPicturePresentFlag = Convert.ToBoolean((data[15] & 0x20) >> 5);
        SubPicHrdParamsNotPresentFlag = Convert.ToBoolean((data[15] & 0x10) >> 4);
        HDRWCGIdc = (byte)(data[15] & 0x03);
        if (TemporalLayerSubsetFlag)
        {
            TemporalIdMin = (byte)((data[16] & 0xe0) >> 5);
            TemporalIdMax = (byte)((data[17] & 0xe0) >> 5);
        }
    }
}

public record MHLinkageDescriptor : Descriptor
{
    public enum LinkageTypeEnum : byte
    {
        InformationService = 0x01,
        ElectronicProgramGuideService = 0x02,
        CAAlternativeService = 0x03,
        TLVStreamIncludingAllNetworkOrBouquetSI = 0x04,
        AlternativeService = 0x05,
        INT = 0x0b
    }

    public ushort TlvStreamId { get; init; }
    public ushort OriginalNetworkId { get; init; }
    public ushort ServiceId { get; init; }
    public LinkageTypeEnum LinkageType { get; init; }
    public byte[] PrivateDataByte { get; init; }

    public MHLinkageDescriptor(ReadOnlySpan<byte> data)
    {
        TagAndLengthBytes = 4;

        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);
        TlvStreamId = BinaryPrimitives.ReadUInt16BigEndian(data[4..6]);
        OriginalNetworkId = BinaryPrimitives.ReadUInt16BigEndian(data[6..8]);
        ServiceId = BinaryPrimitives.ReadUInt16BigEndian(data[8..10]);
        LinkageType = (LinkageTypeEnum)data[10];
        PrivateDataByte = data[11..(4 + DescriptorLength)].ToArray();
    }
}

public record MHEventGroupDescriptor : Descriptor
{
    public enum GroupTypeEnum : byte
    {
        EventInCommon = 0x1,
        EventRelay = 0x2,
        EventTransfer = 0x3,
        EventRelayToTheOtherNetwork = 0x4,
        EventTransferFromTHeOtherNetwork = 0x5
    }
    public record EventInfo(ushort ServiceId, ushort EventId);
    public record OtherNetworkInfo(ushort OriginalNetworkId, ushort TlvStreamId, ushort ServiceId, ushort EventId);

    public GroupTypeEnum GroupType { get; init; }
    public EventInfo[] EventInfos { get; init; }
    public OtherNetworkInfo[]? OtherNetworkInfos { get; init; }
    public byte[]? PrivateDataByte { get; init; }

    public MHEventGroupDescriptor(ReadOnlySpan<byte> data)
    {
        TagAndLengthBytes = 4;

        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        GroupType = (GroupTypeEnum)((data[3] & 0xf0) >> 4);
        var eventCount = data[4] & 0x0f;

        var pos = 5;
        var events = new List<EventInfo>();
        for (var i = 0; i < eventCount; i++)
        {
            var serviceId = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var eventId = BinaryPrimitives.ReadUInt16BigEndian(data[(pos + 2)..(pos + 4)]);
            events.Add(new EventInfo(serviceId, eventId));
            pos += 4;
        }
        EventInfos = events.ToArray();

        if (GroupType is GroupTypeEnum.EventRelayToTheOtherNetwork or GroupTypeEnum.EventTransferFromTHeOtherNetwork)
        {
            var networkInfos = new List<OtherNetworkInfo>();
            foreach (var networkBytes in data[pos..(3 + DescriptorLength)].ToArray().Chunk(8))
            {
                var originalNetworkId = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
                var tlvStreamId = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);
                var serviceId = BinaryPrimitives.ReadUInt16BigEndian(data[4..6]);
                var eventId = BinaryPrimitives.ReadUInt16BigEndian(data[6..8]);
                networkInfos.Add(new OtherNetworkInfo(originalNetworkId, tlvStreamId, serviceId, eventId));
            }
            OtherNetworkInfos = networkInfos.ToArray();
        }
        else
        {
            PrivateDataByte = data[pos..(3 + DescriptorLength)].ToArray();
        }
    }
}

public record MHServiceListDescriptor : Descriptor
{
    public ServiceIdTypePair[] ServiceList { get; init; }

    public MHServiceListDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];

        var tmpServiceList = new List<ServiceIdTypePair>();
        foreach (var idAndTypeBinary in data[3..(3 + DescriptorLength)].ToArray().Chunk(3))
        {
            var sid = BinaryPrimitives.ReadUInt16BigEndian(idAndTypeBinary.AsSpan(0, 2));
            var type = (ServiceType)idAndTypeBinary[2];
            tmpServiceList.Add(new(sid, type));
        }
        ServiceList = tmpServiceList.ToArray();
    }
}

public record MHShortEventDescriptor : Descriptor
{
    public string ISO639LanguageCode { get; init; }
    public string EventName { get; init; }
    public string Text { get; init; }

    public MHShortEventDescriptor(ReadOnlySpan<byte> data)
    {
        TagAndLengthBytes = 4;

        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);
        ISO639LanguageCode = Encoding.Latin1.GetString(data[4..7]);
        var eventNameLength = data[7];
        EventName = Encoding.UTF8.GetString(data[8..(8 + eventNameLength)]);
        var textLength = data[8 + eventNameLength];
        Text = Encoding.UTF8.GetString(data[(9 + eventNameLength)..(9 + eventNameLength + textLength)]);
    }
}

public record MHExtendedEventDescriptor : Descriptor
{
    public record ItemInfo(string ItemDescription, string Item);

    public byte DescriptorNumber { get; init; }
    public byte LastDescriptorNumber { get; init; }
    public string ISO639LanguageCode { get; init; }
    public ItemInfo[] ItemInfos { get; init; }
    public string Text { get; init; }

    public MHExtendedEventDescriptor(ReadOnlySpan<byte> data)
    {
        TagAndLengthBytes = 4;

        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);
        DescriptorNumber = (byte)((data[4] & 0xf0) >> 4);
        LastDescriptorNumber = (byte)(data[4] & 0x0f);
        ISO639LanguageCode = Encoding.Latin1.GetString(data[5..8]);
        var lengthOfItems = BinaryPrimitives.ReadUInt16BigEndian(data[8..10]);

        var pos = 10;
        var itemInfos = new List<ItemInfo>();
        while (pos < lengthOfItems + 10)
        {
            var itemDescriptionLength = data[pos];
            var description = Encoding.UTF8.GetString(data[(pos + 1)..(pos + 1 + itemDescriptionLength)]);
            pos += 1 + itemDescriptionLength;
            var itemLength = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var item = Encoding.UTF8.GetString(data[(pos + 2)..(pos + 2 + itemLength)]);
            itemInfos.Add(new ItemInfo(description, item));
            pos += 2 + itemLength;
        }
        ItemInfos = itemInfos.ToArray();

        var textLength = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
        Text = Encoding.UTF8.GetString(data[(pos + 2)..(pos + 2 + textLength)]);
    }
}

public record VideoComponentDescriptor : Descriptor
{
    public enum VideoSignalResolutionType : byte
    {
        NotSpecified = 0,
        _180 = 1,
        _240 = 2,
        _480 = 3,
        _720 = 4,
        _1080 = 5,
        _2160 = 6,
        _4320 = 7
    }

    public enum VideoSignalAspectRatioType : byte
    {
        NotSpecified = 0,
        _4_3 = 1,
        _16_9WithPanVector = 2,
        _16_9WithoutPanVector = 3,
        Over16_9 = 4
    }

    public enum VideoSignalFrameRateType : byte
    {
        NotSpecified = 0,
        _15 = 1,
        _23_976 = 2,
        _24 = 3,
        _25 = 4,
        _29_97 = 5,
        _30 = 6,
        _50 = 7,
        _59_94 = 8,
        _60 = 9,
        _100 = 10,
        _119_89 = 11,
        _120 = 12
    }

    public enum VideoSignalTransferCharacteristicType : byte
    {
        NotSpecified = 0,
        VUI_1 = 1,
        VUI_11 = 2,
        VUI_14 = 3,
        VUI_16 = 4,
        VUI_18 = 5,
    }

    public VideoSignalResolutionType VideoResolution { get; init; }
    public VideoSignalAspectRatioType VideoAspectRatio { get; init; }
    public bool VideoScanFlag { get; init; }
    public VideoSignalFrameRateType VideoFrameRate { get; init; }
    public ushort ComponentTag { get; init; }
    public VideoSignalTransferCharacteristicType VideoTransferCharasteristic { get; init; }
    public string ISO639LanguageCode { get; init; }
    public string Text { get; init; }

    public VideoComponentDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        VideoResolution = (VideoSignalResolutionType)((data[3] & 0xf0) >> 4);
        VideoAspectRatio = (VideoSignalAspectRatioType)(data[3] & 0x0f);
        VideoScanFlag = Convert.ToBoolean((data[4] & 0x80) >> 7);
        VideoFrameRate = (VideoSignalFrameRateType)(data[4] & 0x1f);
        ComponentTag = BinaryPrimitives.ReadUInt16BigEndian(data[5..7]);
        VideoTransferCharasteristic = (VideoSignalTransferCharacteristicType)((data[7] & 0xf0) >> 4);
        ISO639LanguageCode = Encoding.Latin1.GetString(data[8..11]);
        Text = Encoding.UTF8.GetString(data[11..(3 + DescriptorLength)]);
    }
}

public record MHStreamIdentifierDescriptor : Descriptor
{
    public ushort ComponentTag { get; init; }
    public MHStreamIdentifierDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        ComponentTag = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
    }
}

public record MHContentDescriptor : Descriptor
{
    public record ContentInfo(byte ContentNibbleLevel1, byte ContentNibbleLevel2, byte UserNibble1, byte UserNibble2);
    public ContentInfo[] ContentInfos { get; set; }

    public MHContentDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        var contents = new List<ContentInfo>();
        foreach (var contentByte in data[3..(3 + DescriptorLength)].ToArray().Chunk(2))
        {
            var nibbleLevel1 = (byte)((contentByte[0] & 0xf0) >> 4);
            var nibbleLevel2 = (byte)(contentByte[0] & 0x0f);
            var userNibble1 = (byte)((contentByte[1] & 0xf0) >> 4);
            var userNibble2 = (byte)(contentByte[1] & 0x0f);
            contents.Add(new ContentInfo(nibbleLevel1, nibbleLevel2, userNibble1, userNibble2));
        }
        ContentInfos = contents.ToArray();
    }
}

public record MHParentalRatingDescriptor : Descriptor
{
    public record RatingInfo(string CountryCode, byte Rating);
    public RatingInfo[] RatingInfos { get; init; }

    public MHParentalRatingDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        var ratings = new List<RatingInfo>();
        foreach (var ratingByte in data[3..(3 + DescriptorLength)].ToArray().Chunk(4))
        {
            var countryCode = Encoding.Latin1.GetString(ratingByte[..3]);
            var rating = ratingByte[3];
            ratings.Add(new RatingInfo(countryCode, rating));
        }
        RatingInfos = ratings.ToArray();
    }
}

public record MHAudioComponentDescriptor : Descriptor
{
    public enum ContentOfComponentType : byte
    {
        NotSpecified = 0x2,
        MPEG4AAC = 0x3,
        MPEG4ALS = 0x4
    }

    public enum QualityIndicatorType : byte
    {
        Mode1 = 0b01,
        Mode2 = 0b10,
        Mode3 = 0b11
    }

    public enum SamplingRateType : byte
    {
        _16kHz = 0b001,
        _22_05kHz = 0b010,
        _24kHz = 0b011,
        _32kHz = 0b101,
        _44_1kHz = 0b110,
        _48kHz = 0b111
    }

    public ContentOfComponentType StreamContent { get; init; }
    public byte ComponentType { get; init; }
    public ushort ComponentTag { get; init; }
    public byte StreamType { get; init; }
    public byte SimulcastGroupTag { get; init; }
    public bool ESMultiLingualFlag { get; init; }
    public bool MainComponentFlag { get; init; }
    public QualityIndicatorType QualityIndicator { get; init; }
    public SamplingRateType SamplingRate { get; init; }
    public string ISO639LanguageCode { get; init; }
    public string? ISO639LanguageCode2 { get; init; }
    public string Text { get; init; }

    public MHAudioComponentDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        StreamContent = (ContentOfComponentType)(data[3] & 0x0f);
        ComponentType = data[4];
        ComponentTag = BinaryPrimitives.ReadUInt16BigEndian(data[5..7]);
        StreamType = data[7];
        SimulcastGroupTag = data[8];
        ESMultiLingualFlag = Convert.ToBoolean((data[9] & 0x80) >> 7);
        MainComponentFlag = Convert.ToBoolean((data[9] & 0x40) >> 6);
        QualityIndicator = (QualityIndicatorType)((data[9] & 0x30) >> 4);
        SamplingRate = (SamplingRateType)((data[9] & 0x0e) >> 1);
        ISO639LanguageCode = Encoding.Latin1.GetString(data[10..13]);
        var pos = 13;
        if (ESMultiLingualFlag)
        {
            ISO639LanguageCode2 = Encoding.Latin1.GetString(data[pos..(pos + 3)]);
            pos += 3;
        }
        Text = Encoding.UTF8.GetString(data[pos..(3 + DescriptorLength)]);
    }
}

public record MHTargetRegionDescriptor : Descriptor
{
    public enum RegionSpecTypeEnum : byte
    {
        PrefecturalRegion = 0x01
    }

    public RegionSpecTypeEnum RegionSpecType { get; init; }

    // ARIB STD-B10で定義
    public byte[] TargetRegionSpec { get; init; }

    public MHTargetRegionDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        RegionSpecType = (RegionSpecTypeEnum)data[3];
        TargetRegionSpec = data[4..(3 + DescriptorLength)].ToArray();
    }
}

public record MHSeriesDescriptor : Descriptor
{
    public enum ProgramPatternType : byte
    {
        Irregular = 0x0,
        MultipleInAWeek = 0x1,
        AroundWeekly = 0x2,
        AroundMonthly = 0x3,
        MultipleOnTheSameDay = 0x4,
        DivisionOfLongHoursProgram = 0x5,
        RegularOrIrregularProgramForStrage = 0x6
    }

    public ushort SeriesId { get; init; }
    public byte RepeatLabel { get; init; }
    public ProgramPatternType ProgramPattern { get; init; }
    public bool ExpireDateValidFlag { get; init; }
    public DateOnly ExpireDate { get; init; }
    public ushort EpisodeNumber { get; init; }
    public ushort LastEpisodeNumber { get; init; }
    public string SeriesName { get; init; }

    public MHSeriesDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        SeriesId = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        RepeatLabel = (byte)((data[5] & 0xf0) >> 4);
        ProgramPattern = (ProgramPatternType)((data[6] & 0x0e) >> 1);
        ExpireDateValidFlag = Convert.ToBoolean(data[6] & 0x01);
        ExpireDate = Utilities.ParseMJDBytes(data[7..9]);
        EpisodeNumber = (ushort)((data[9] << 4) | ((data[10] & 0xf0) >> 4));
        LastEpisodeNumber = (ushort)(((data[10] & 0x0f) << 8) | data[11]);
        SeriesName = Encoding.UTF8.GetString(data[12..(3 + DescriptorLength)]);
    }
}

public record MHSIParameterDescriptor : Descriptor
{
    // ARIB STD TR-B39
    public abstract record TableDescriptionByte;
    public record TableCycleDescription(byte TableCycle) : TableDescriptionByte;
    public record CycleGroupInfo(byte NumOfSegment, byte Cycle);
    public record MHEITDescription(byte MediaType, byte Pattern, byte ScheduleRange, ushort BaseCycle, CycleGroupInfo[] CycleGroupInfos);
    public record MHEITDescriptions(MHEITDescription[] Descriptions) : TableDescriptionByte;

    private static MHEITDescriptions ParseMHEITDescriptions(ReadOnlySpan<byte> data)
    {
        var pos = 0;
        var descriptions = new List<MHEITDescription>();
        while (pos < data.Length)
        {
            var mediaType = (byte)((data[pos] & 0xc0) >> 6);
            var pattern = (byte)((data[pos] & 0x30) >> 4);
            var scheduleRange = data[pos + 1];
            var baseCycle = (ushort)(data[pos + 2] << 4 | ((data[pos + 3] & 0xf0) >> 4));
            var cycleGroupCount = data[pos + 3] & 0x03;
            var cycleGroupInfos = data[(pos + 4)..(pos + 4 + 2 * cycleGroupCount)].ToArray().Chunk(2).Select(d => new CycleGroupInfo(d[0], d[1])).ToArray();

            descriptions.Add(new MHEITDescription(mediaType, pattern, scheduleRange, baseCycle, cycleGroupInfos));
            pos += 4 + 2 * cycleGroupCount;
        }
        return new MHEITDescriptions(descriptions.ToArray());
    }

    private static TableDescriptionInfo ParseTableDescriptionInfo(ReadOnlySpan<byte> data, bool isFirstLoop)
    {
        var tableId = data[0];
        var tableDescriptionLength = data[1];
        TableDescriptionByte? description;
        if (isFirstLoop)
        {
            description = tableId switch
            {
                0x40 or // TLV-NIT[actual]
                0x41 or // TLV-NIT[other]
                0x9d or // MH-BIT
                0x9f or // MH-SDT[actual]
                0xa0 or // MH-SDT[other]
                0x8b or // MH-EIT[p/f]
                0x9e    // MH-SDTT
                => new TableCycleDescription(data[2]),
                0x8c    // MH-EIT[schedule basic]
                => ParseMHEITDescriptions(data[2..(2 + tableDescriptionLength)]),
                _ => null
            };
        }
        else
        {
            description = tableId switch
            {
                0xfe or // AMT
                0xa2    // MH-CDT
                => new TableCycleDescription(data[2]),
                0x8c or // MH-EIT[schedule basic]
                0x94    // MH-EIT[schedule extended]
                => ParseMHEITDescriptions(data[2..(2 + tableDescriptionLength)]),
                _ => null
            };
        }

        if (description is null)
        {
            throw new InvalidDataException();
        }

        return new TableDescriptionInfo(tableId, description);
    }

    public record TableDescriptionInfo(byte TableId, TableDescriptionByte TableDescription);

    public byte ParameterVersion { get; init; }
    public DateOnly UpdateTime { get; init; }
    public TableDescriptionInfo[] TableDescriptions { get; init; }

    public MHSIParameterDescriptor(ReadOnlySpan<byte> data, bool isFirstLoop)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        ParameterVersion = data[3];
        UpdateTime = Utilities.ParseMJDBytes(data[4..6]);

        var pos = 6;
        var tableDescriptions = new List<TableDescriptionInfo>();
        while (pos < 3 + DescriptorLength)
        {
            var tableDescriptionLength = data[pos + 1];
            // table_idを含む
            var tableDescriptionByte = data[pos..(pos + 2 + tableDescriptionLength)];
            tableDescriptions.Add(ParseTableDescriptionInfo(tableDescriptionByte, isFirstLoop));
            pos += 2 + tableDescriptionLength;
        }
        TableDescriptions = tableDescriptions.ToArray();
    }
}

public record MHBroadcasterNameDescriptor : Descriptor
{
    public string Name { get; init; }

    public MHBroadcasterNameDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        Name = Encoding.UTF8.GetString(data[3..(3 + DescriptorLength)]);
    }
}

public record MHServiceDescriptor : Descriptor
{
    public ServiceType ServiceType { get; init; }
    public string ServiceProviderName { get; init; }
    public string ServiceName { get; init; }

    public MHServiceDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        ServiceType = (ServiceType)data[3];
        var serviceProviderNameLength = data[4];
        ServiceProviderName = Encoding.UTF8.GetString(data[5..(5 + serviceProviderNameLength)]);
        var serviceNameLength = data[5 + serviceProviderNameLength];
        ServiceName = Encoding.UTF8.GetString(data[(6 + serviceProviderNameLength)..(6 + serviceProviderNameLength + serviceNameLength)]);
    }
}

public record IPDataFlowDescriptor : Descriptor
{
    public record FlowInfo(byte IPDataFlowId, IPAddress SrcAddress, IPAddress DstAddress, ushort SrcPort, ushort DstPort);

    public bool IPVersion { get; init; }
    public byte NumberOfFlow { get; init; }
    public FlowInfo[] FlowInfos { get; init; }

    public IPDataFlowDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        IPVersion = Convert.ToBoolean((data[3] & 0x80) >> 7);
        NumberOfFlow = (byte)(data[3] & 0x7f);
        FlowInfos = new FlowInfo[NumberOfFlow];

        var pos = 4;
        for (var i = 0; i < NumberOfFlow; i++)
        {
            var ipDataFlowId = data[pos];
            IPAddress src, dst;
            if (IPVersion)
            {
                // IPv6
                src = new IPAddress(data[(pos + 1)..(pos + 17)]);
                dst = new IPAddress(data[(pos + 17)..(pos + 33)]);
                pos += 33;
            }
            else
            {
                // IPv4
                src = new IPAddress(data[(pos + 1)..(pos + 5)]);
                dst = new IPAddress(data[(pos + 5)..(pos + 9)]);
                pos += 9;
            }
            var srcPort = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var dstPort = BinaryPrimitives.ReadUInt16BigEndian(data[(pos + 2)..(pos + 4)]);
            pos += 4;
            FlowInfos[i] = new FlowInfo(ipDataFlowId, src, dst, srcPort, dstPort);
        }
    }
}

public record MHCAStartupDescriptor : Descriptor
{
    public ushort CASystemId { get; init; }
    public ushort CAProgramId { get; init; }
    public bool SecondLoadFlag { get; init; }
    public byte LoadIndicator { get; init; }
    public ushort? SecondCAProgramId { get; init; }
    public byte? SecondLoadIndicator { get; init; }
    public ushort[] ExclusionCAProgramIDs { get; init; }
    public byte[] LoadSecurityInfoByte { get; init; }
    public byte[] PrivateDataByte { get; init; }

    public MHCAStartupDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        CASystemId = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        CAProgramId = (ushort)(((data[5] & 0x1f) << 8) | data[6]);
        SecondLoadFlag = Convert.ToBoolean((data[7] & 0x80) >> 7);
        LoadIndicator = (byte)(data[7] & 0x7f);

        var pos = 8;
        if (SecondLoadFlag)
        {
            SecondCAProgramId = (ushort)(((data[pos] & 0x1f) << 8) | data[pos + 1]);
            SecondLoadIndicator = (byte)(data[pos + 3] & 0x7f);
            pos += 4;
        }
        var exclusionIdNum = data[pos];
        pos++;

        ExclusionCAProgramIDs = new ushort[exclusionIdNum];
        for (var i = 0; i < exclusionIdNum; i++)
        {
            ExclusionCAProgramIDs[i] = (ushort)(((data[pos] & 0x1f) << 8) | data[pos + 1]);
            pos += 2;
        }

        var loadSecurityLength = data[pos];
        LoadSecurityInfoByte = data[(pos + 1)..(pos + 1 + loadSecurityLength)].ToArray();
        pos += 1 + loadSecurityLength;

        PrivateDataByte = data[pos..(3 + DescriptorLength)].ToArray();
    }
}

public record MHDataComponentDescriptor : Descriptor
{
    public abstract record AdditionalInfo;

    public record AdditionalAribSubtitleInfo : AdditionalInfo
    {
        public enum TypeEnum : byte
        {
            ClosedCaption = 0b00,
            Superimposition = 0b01
        }
        public enum SubtitleFormatEnum : byte
        {
            ARIBTTML = 0b0000
        }
        public enum OperationMode : byte
        {
            LiveMode = 0b00,
            SegmentMode = 0b01,
            ProgramMode = 0b10
        }
        public enum TimeControlMode : byte
        {
            UTCInTTML = 0b0000,
            MHEITInTTML = 0b0001,
            ReferenceStarttimeInTTML = 0b0010,
            MPUTimestampInTTML = 0b0011,
            NTPInTTML = 0b0100,
            MPUTimestamp = 0b1000,
            WithoutTimeControl = 0b1111
        }
        public enum DataContentResolution : byte
        {
            _1920_1080 = 0b0000,
            _3840_2160 = 0b0001,
            _7680_4320 = 0b0010
        }
        public enum Compression : byte
        {
            UnCompression = 0b0000,
            EXIWithSchemaSpecification = 0b0001,
            EXIWithoutSchemaSpecification = 0b0010
        }

        public byte SubtitleTag { get; init; }
        public byte SubtitleInfoVersion { get; init; }
        public bool StartMPUSequenceNumberFlag { get; init; }
        public string ISO639LanguageCode { get; init; }
        public TypeEnum Type { get; init; }
        public SubtitleFormatEnum SubtitleFormat { get; init; }
        public OperationMode OPM { get; init; }
        public TimeControlMode TMD { get; init; }
        public byte DMF { get; init; }
        public DataContentResolution Resolution { get; init; }
        public Compression CompressionType { get; init; }
        public uint? StartMPUSequenceNumber { get; init; }
        // NTPFormat
        public ulong? ReferenceStartTime { get; init; }
        public byte? ReferenceStartTimeLeapIndicator { get; init; }
        public AdditionalAribSubtitleInfo(ReadOnlySpan<byte> data)
        {
            SubtitleTag = data[0];
            SubtitleInfoVersion = (byte)(data[1] & 0xf0);
            StartMPUSequenceNumberFlag = Convert.ToBoolean((data[1] & 0x08) >> 3);
            ISO639LanguageCode = Encoding.UTF8.GetString(data[2..5]);
            Type = (TypeEnum)((data[5] & 0xc0) >> 6);
            SubtitleFormat = (SubtitleFormatEnum)((data[5] & 0x3c) >> 2);
            OPM = (OperationMode)(data[5] & 0x03);
            TMD = (TimeControlMode)((data[6] & 0xf0) >> 4);
            DMF = (byte)(data[6] & 0x0f);
            Resolution = (DataContentResolution)((data[7] & 0xf0) >> 4);
            CompressionType = (Compression)(data[7] & 0x0f);
            var pos = 8;
            if (StartMPUSequenceNumberFlag)
            {
                StartMPUSequenceNumber = BinaryPrimitives.ReadUInt32BigEndian(data[pos..(pos + 4)]);
                pos += 4;
            }
            if (TMD == TimeControlMode.ReferenceStarttimeInTTML)
            {
                ReferenceStartTime = BinaryPrimitives.ReadUInt64BigEndian(data[pos..(pos + 8)]);
                ReferenceStartTimeLeapIndicator = (byte)((data[pos + 8] & 0xc0) >> 6);
            }
        }
    }

    public enum DataComponentSystem : ushort
    {
        ClosedCaptionCodingSystem = 0x0020,
        MultimediaCodingSystem = 0x0021
    };

    public DataComponentSystem DataComponentId { get; init; }
    public AdditionalInfo? AdditionalDataComponentInfo { get; init; }

    public MHDataComponentDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        DataComponentId = (DataComponentSystem)BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        AdditionalDataComponentInfo = DataComponentId switch
        {
            DataComponentSystem.ClosedCaptionCodingSystem => new AdditionalAribSubtitleInfo(data[5..(3 + DescriptorLength)]),
            _ => null
        };
    }
}

public record MHLocalTimeOffsetDescriptor : Descriptor
{
    public record LocalTimeOffsetInfo(string CountryCode, byte CountryRegionId, bool LocalTimeOffsetPolarity,
        TimeSpan LocalTimeOffset, DateTime TimeOfChange, TimeSpan NextTimeOffset);
    public LocalTimeOffsetInfo[] LocalTimeOffsetInfos { get; init; }

    public MHLocalTimeOffsetDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        var offsetInfos = new List<LocalTimeOffsetInfo>();
        foreach (var timeOffsetBytes in data[3..(3 + DescriptorLength)].ToArray().Chunk(13))
        {
            var offsetInfo = new LocalTimeOffsetInfo(
                CountryCode: Encoding.Latin1.GetString(data[..3]),
                CountryRegionId: (byte)((data[3] & 0xfc) >> 2),
                LocalTimeOffsetPolarity: Convert.ToBoolean(data[3] & 0x01),
                LocalTimeOffset: Utilities.ParseBCDOffset(data[4..6]),
                TimeOfChange: Utilities.ParseJSTandMJDbytes(data[6..11]),
                NextTimeOffset: Utilities.ParseBCDOffset(data[11..13])
            );
            offsetInfos.Add(offsetInfo);
        }
        LocalTimeOffsetInfos = offsetInfos.ToArray();
    }
}

public record MHComponentGroupDescriptor : Descriptor
{
    public record CAUnitInfo(byte CAUnitId, ushort[] ComponentTags);
    public record GroupInfo(byte ComponentGroupId, CAUnitInfo[] CAUnits, byte? TotalBitRate, string Text);

    public enum ComponentGroupTypeEnum : byte
    {
        MultiViewTVService = 0b000
    }

    public ComponentGroupTypeEnum ComponentGroupType { get; init; }
    public bool TotalBitRateFlag { get; init; }
    public GroupInfo[] GroupInfos { get; init; }

    public MHComponentGroupDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        ComponentGroupType = (ComponentGroupTypeEnum)((data[3] & 0xe0) >> 5);
        TotalBitRateFlag = Convert.ToBoolean((data[3] & 0x10) >> 4);
        var numOfGroup = data[3] & 0x0f;
        GroupInfos = new GroupInfo[numOfGroup];

        var pos = 4;
        for (var i = 0; i < numOfGroup; i++)
        {
            var componentGroupId = (byte)((data[pos] & 0xf0) >> 4);
            var numOfCAUnit = data[pos] & 0x0f;
            var CaUnits = new CAUnitInfo[numOfCAUnit];
            byte? totalBitrate = null;
            for (var j = 0; j < numOfCAUnit; j++)
            {
                var caUnitId = (byte)((data[pos + 1] & 0xf0) >> 4);
                var numOfComponent = data[pos + 1] & 0x0f;
                var componentTags = data[(pos + 2)..(pos + 2 + numOfComponent * 2)].ToArray()
                    .Chunk(2).Select(d => BinaryPrimitives.ReadUInt16BigEndian(d)).ToArray();
                CaUnits[j] = new CAUnitInfo(caUnitId, componentTags);
                pos += 2 + numOfComponent * 2;
            }
            if (TotalBitRateFlag)
            {
                totalBitrate = data[pos];
                pos++;
            }
            var textLength = data[pos];
            var text = Encoding.UTF8.GetString(data[pos..(pos + textLength)]);
            GroupInfos[i] = new GroupInfo(componentGroupId, CaUnits, totalBitrate, text);
        }
    }
}

public record MHLogoTransmittionDescriptor : Descriptor
{
    public abstract record LogoData;
    public record LogoDataSystem1(ushort LogoId, ushort LogoVersion, ushort DownloadDataId, LogoSectionInfo[] LogoSectionInfos) : LogoData;
    public record LogoDataSystem2(ushort LogoId) : LogoData;
    public record LogoDataSimpleSystem(string Logo) : LogoData;

    public record LogoSectionInfo(LogoTypeEnum LogoType, byte StartSectionNumber, byte NumOfSections);

    public enum LogoTransmissionTypeEnum : byte
    {
        TransmissionSystem1 = 0x01,
        TransmissionSystem2 = 0x02,
        SimpleLogoSystem = 0x03
    }

    public LogoTransmissionTypeEnum LogoTransmissionType { get; init; }
    public LogoData? Logo { get; init; }

    public MHLogoTransmittionDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        LogoTransmissionType = (LogoTransmissionTypeEnum)data[3];
        if (LogoTransmissionType == LogoTransmissionTypeEnum.TransmissionSystem1)
        {
            var logoId = (ushort)(((data[4] & 0x01) << 8) | data[5]);
            var logoVersion = (ushort)(((data[6] & 0x0f) << 8) | data[7]);
            var downloadDataId = BinaryPrimitives.ReadUInt16BigEndian(data[8..10]);
            var logoSectionInfos = new List<LogoSectionInfo>();
            foreach (var logoSectionBytes in data[10..(3 + DescriptorLength)].ToArray().Chunk(3))
            {
                logoSectionInfos.Add(new LogoSectionInfo((LogoTypeEnum)logoSectionBytes[0], logoSectionBytes[1], logoSectionBytes[2]));
            }
            Logo = new LogoDataSystem1(logoId, logoVersion, downloadDataId, logoSectionInfos.ToArray());
        }
        else if (LogoTransmissionType == LogoTransmissionTypeEnum.TransmissionSystem2)
        {
            var logoId = (ushort)(((data[4] & 0x01) << 8) | data[5]);
            Logo = new LogoDataSystem2(logoId);
        }
        else if (LogoTransmissionType == LogoTransmissionTypeEnum.SimpleLogoSystem)
        {
            var logoText = Encoding.UTF8.GetString(data[4..(3 + DescriptorLength)]);
            Logo = new LogoDataSimpleSystem(logoText);
        }
    }
}

public record MPUExtendedTimestampDescriptor : Descriptor
{
    public record AccessUnit(ushort DtsPtsOffset, ushort? PtsOffset);
    public record ExtendedTimestamp(uint MPUSequenceNumber, byte MPUPresentationTimeLeapIndicator, ushort MPUDecodingTimeOffset, AccessUnit[] OffsetInfos);

    public byte PtsOffsetType { get; init; }
    public bool TimescaleFlag { get; init; }
    public uint? Timescale { get; init; }
    public ushort? DefaultPtsOffset { get; init; }
    public ExtendedTimestamp[] TimestampInfos { get; init; }

    public MPUExtendedTimestampDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        PtsOffsetType = (byte)((data[3] & 0x06) >> 1);
        TimescaleFlag = Convert.ToBoolean(data[3] & 0x01);

        var pos = 4;
        if (TimescaleFlag)
        {
            Timescale = BinaryPrimitives.ReadUInt32BigEndian(data[pos..(pos + 4)]);
            pos += 4;
        }
        if (PtsOffsetType == 1)
        {
            DefaultPtsOffset = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            pos += 2;
        }

        var timestamps = new List<ExtendedTimestamp>();
        while (pos < 3 + DescriptorLength)
        {
            var mpuSequenceNumber = BinaryPrimitives.ReadUInt32BigEndian(data[pos..(pos + 4)]);
            var mpuPResentationTimeLeapIndicator = (byte)((data[pos + 4] & 0xc0) >> 6);
            var mpuDecodingTimeOffset = BinaryPrimitives.ReadUInt16BigEndian(data[(pos + 5)..(pos + 7)]);
            var numOfAu = data[pos + 7];

            var aus = new List<AccessUnit>();
            pos += 8;
            for (var j = 0; j < numOfAu; j++)
            {
                ushort? ptsOffset = null;
                var dtsPtsOffset = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
                pos += 2;

                if (PtsOffsetType == 2)
                {
                    ptsOffset = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
                    pos += 2;
                }
                aus.Add(new AccessUnit(dtsPtsOffset, ptsOffset));
            }
            timestamps.Add(new ExtendedTimestamp(mpuSequenceNumber, mpuPResentationTimeLeapIndicator, mpuDecodingTimeOffset, aus.ToArray()));
        }
        TimestampInfos = timestamps.ToArray();
    }
}
public record TextInfo(string ISO639LanguageCode, string Text);

public record MPUDownloadContentDescriptor : Descriptor
{
    public record ItemInfo(uint ItemId, uint ItemSize, byte[] ItemInfoByte);

    public bool Reboot { get; init; }
    public bool AddOn { get; init; }
    public bool CompatibilityFlag { get; init; }
    public bool ItemInfoFlag { get; init; }
    public bool TextInfoFlag { get; init; }
    public uint ComponentSize { get; init; }
    public uint DownloadId { get; init; }
    public uint TimeOutValueDAM { get; init; }
    public uint LeakRate { get; init; }
    public ushort ComponentTag { get; init; }
    public byte[]? CompatibilityDescriptor { get; init; }
    public ItemInfo[]? ItemInfos { get; init; }
    public byte[] PrivateDataByte { get; init; }
    public TextInfo? Text { get; init; }

    public MPUDownloadContentDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        Reboot = Convert.ToBoolean((data[3] & 0x80) >> 7);
        AddOn = Convert.ToBoolean((data[3] & 0x40) >> 6);
        CompatibilityFlag = Convert.ToBoolean((data[3] & 0x20) >> 5);
        ItemInfoFlag = Convert.ToBoolean((data[3] & 0x10) >> 4);
        TextInfoFlag = Convert.ToBoolean((data[3] & 0x08) >> 3);
        ComponentSize = BinaryPrimitives.ReadUInt32BigEndian(data[4..8]);
        DownloadId = BinaryPrimitives.ReadUInt32BigEndian(data[8..12]);
        TimeOutValueDAM = BinaryPrimitives.ReadUInt32BigEndian(data[12..16]);
        LeakRate = ((uint)data[16] << 14) | ((uint)data[17] << 6) | (((uint)data[18] & 0xfc) >> 2);
        ComponentTag = BinaryPrimitives.ReadUInt16BigEndian(data[19..21]);

        var pos = 21;
        if (CompatibilityFlag)
        {
            var length = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            CompatibilityDescriptor = data[pos..(pos + length)].ToArray();
            pos += length;
        }

        if (ItemInfoFlag)
        {
            var numOfItems = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            pos += 2;
            ItemInfos = new ItemInfo[numOfItems];
            for (var i = 0; i < numOfItems; i++)
            {
                var itemId = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
                var itemSize = BinaryPrimitives.ReadUInt16BigEndian(data[(pos + 2)..(pos + 4)]);
                var itemInfoLength = data[pos + 4];
                var itemInfo = data[(pos + 5)..(pos + 5 + itemInfoLength)].ToArray();
                ItemInfos[i] = new ItemInfo(itemId, itemSize, itemInfo);
            }
        }

        var privateDataLength = data[pos];
        PrivateDataByte = data[(pos + 1)..(pos + 1 + privateDataLength)].ToArray();
        pos += 1 + privateDataLength;
        if (TextInfoFlag)
        {
            var iso639 = Encoding.Latin1.GetString(data[pos..(pos + 3)]);
            var length = data[pos + 4];
            var text = Encoding.UTF8.GetString(data[(pos + 5)..(pos + 5 + length)]);
            Text = new TextInfo(iso639, text);
        }
    }
}

public record MHNetworkDownloadContentDescriptor : Descriptor
{
    public enum AddressTypeEnum : byte
    {
        IPv4 = 0x00,
        IPv6 = 0x01,
        URL = 0x02
    }

    public abstract record AddressInfo;
    public record IPv4AddressInfo(IPAddress IPv4Address, ushort PortNumber) : AddressInfo;
    public record IPv6AddressInfo(IPAddress IPv6Address, ushort PortNumber) : AddressInfo;
    public record URLAddressInfo(string URL) : AddressInfo;

    public bool Reboot { get; init; }
    public bool AddOn { get; init; }
    public bool CompatibilityFlag { get; init; }
    public bool TextInfoFlag { get; init; }
    public uint ComponentSize { get; init; }
    public byte SessionProtocolNumber { get; init; }
    public uint SessionId { get; init; }
    public byte Retry { get; init; }
    public uint ConnectTimer { get; init; }
    public AddressTypeEnum AddressType { get; init; }
    public AddressInfo? Address { get; init; }
    public byte[]? CompatibilityDescriptor { get; init; }
    public byte[] PrivateDataByte { get; init; }
    public TextInfo? Text { get; init; }

    public MHNetworkDownloadContentDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        Reboot = Convert.ToBoolean((data[3] & 0x80) >> 7);
        AddOn = Convert.ToBoolean((data[3] & 0x40) >> 6);
        CompatibilityFlag = Convert.ToBoolean((data[3] & 0x20) >> 5);
        TextInfoFlag = Convert.ToBoolean((data[3] & 0x10) >> 4);
        ComponentSize = BinaryPrimitives.ReadUInt32BigEndian(data[4..8]);
        SessionProtocolNumber = data[8];
        SessionId = BinaryPrimitives.ReadUInt32BigEndian(data[9..13]);
        Retry = data[14];
        ConnectTimer = ((uint)data[15] << 16) | ((uint)data[16] << 8) | data[17];
        AddressType = (AddressTypeEnum)data[18];

        var pos = 19;
        if (AddressType == AddressTypeEnum.IPv4)
        {
            Address = new IPv4AddressInfo(new IPAddress(data[pos..(pos + 4)]),
                BinaryPrimitives.ReadUInt16BigEndian(data[(pos + 4)..(pos + 6)]));
            pos += 6;
        }
        else if (AddressType == AddressTypeEnum.IPv6)
        {
            Address = new IPv6AddressInfo(new IPAddress(data[pos..(pos + 16)]),
                BinaryPrimitives.ReadUInt16BigEndian(data[(pos + 16)..(pos + 18)]));
            pos += 18;
        }
        else if (AddressType == AddressTypeEnum.URL)
        {
            var length = data[pos];
            Address = new URLAddressInfo(Encoding.UTF8.GetString(data[(pos + 1)..(pos + 1 + length)]));
            pos += 1 + length;
        }

        if (CompatibilityFlag)
        {
            var length = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            CompatibilityDescriptor = data[pos..(pos + length)].ToArray();
            pos += length;
        }

        var privateDataLength = data[pos];
        PrivateDataByte = data[(pos + 1)..(pos + 1 + privateDataLength)].ToArray();
        pos += 1 + privateDataLength;

        if (TextInfoFlag)
        {
            var iso639 = Encoding.Latin1.GetString(data[pos..(pos + 3)]);
            var length = data[pos + 4];
            var text = Encoding.UTF8.GetString(data[(pos + 5)..(pos + 5 + length)]);
            Text = new TextInfo(iso639, text);
        }
    }
}

public record MHDownloadProtectionDescriptor : Descriptor
{
    public byte DLSystemId { get; init; }
    public MMTGeneralLocationInfo MMTGeneralLocationInfo { get; init; }
    public byte EncryptProtocolNumber { get; init; }
    public byte[] EncryptInfo { get; init; }

    public MHDownloadProtectionDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        DLSystemId = data[3];
        var (mgli, length) = MMTGeneralLocationInfo.ParseMMTGeneralLocationInfo(data[4..]);
        MMTGeneralLocationInfo = mgli;
        var pos = 4 + length;
        EncryptProtocolNumber = data[pos];
        EncryptInfo = data[(pos + 1)..(3 + DescriptorLength)].ToArray();
    }
}

public record ApplicationServiceDescriptor : Descriptor
{
    public record EMTLocationAndTagInfo(byte EMTTag, MMTGeneralLocationInfo EMTLocationInfo);

    public enum ApplicationFormatType : byte
    {
        ARIBHTML5 = 0x1
    }
    public enum DataContentResolution : byte
    {
        _1920_1080 = 0b0000,
        _3840_2160 = 0b0001,
        _7680_4320 = 0b0010
    }

    public ApplicationFormatType ApplicationFormat { get; init; }
    public DataContentResolution DocumentResolution { get; init; }
    public bool DefaultAITFlag { get; init; }
    public bool DTMessageFlag { get; init; }
    public MMTGeneralLocationInfo AMTLocationInfo { get; init; }
    public MMTGeneralLocationInfo? DTMessageLocationInfo { get; init; }
    public EMTLocationAndTagInfo[] EMTLocationInfos { get; init; }
    public byte[] PrivateData { get; init; }

    public ApplicationServiceDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        ApplicationFormat = (ApplicationFormatType)((data[3] & 0xf0) >> 4);
        DocumentResolution = (DataContentResolution)((data[4] & 0xf0) >> 4);
        DefaultAITFlag = Convert.ToBoolean((data[5] & 0x80) >> 7);
        DTMessageFlag = Convert.ToBoolean((data[5] & 0x40) >> 6);
        var emtNum = data[5] & 0x0f;

        var pos = 6;
        var length = 0;
        (AMTLocationInfo, length) = MMTGeneralLocationInfo.ParseMMTGeneralLocationInfo(data[pos..]);
        pos += length;

        if (DTMessageFlag)
        {
            (DTMessageLocationInfo, length) = MMTGeneralLocationInfo.ParseMMTGeneralLocationInfo(data[pos..]);
            pos += length;
        }

        EMTLocationInfos = new EMTLocationAndTagInfo[emtNum];
        for (var j = 0; j < emtNum; j++)
        {
            var emtTag = data[pos];
            var (emtLocationInfo, eliLength) = MMTGeneralLocationInfo.ParseMMTGeneralLocationInfo(data[(pos + 1)..]);
            pos += 1 + eliLength;
            EMTLocationInfos[j] = new EMTLocationAndTagInfo(emtTag, emtLocationInfo);
        }

        PrivateData = data[pos..(3 + DescriptorLength)].ToArray();
    }
}

public record MHHierarchyDescriptor : Descriptor
{
    public enum HierarchyTypeEnum : byte
    {
        SpatialScalableCoding = 1,
        PictureQualityScalableCoding = 2,
        TemporalScalableCoding = 3,
        DataPartitioning = 4,
        ExtendedBitstream = 5,
        PrivateStream = 6,
        MultiViewProfile = 7,
        MixedScalableCoding = 8,
        MVCVideoSubBitstream = 9,
        BaseLayerOrSubBitstream = 15 // Base layer, or MVC basic view point sub-bitstream, or AVC video sub-bitstream of MVC, or HEVC temporal sub-bitstream
    }

    public bool TemporalScalabilityFlag { get; init; }
    public bool SpatialScalabilityFlag { get; init; }
    public bool QualityScalabilityFlag { get; init; }
    public HierarchyTypeEnum HierarchyType { get; init; }
    public byte HierarchyLayerIndex { get; init; }
    public bool TrefPresentFlag { get; init; }
    public byte HierarchyEmbeddedLayerIndex { get; init; }
    public byte HierarchyChannel { get; init; }

    public MHHierarchyDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        TemporalScalabilityFlag = Convert.ToBoolean((data[3] & 0x40) >> 6);
        SpatialScalabilityFlag = Convert.ToBoolean((data[3] & 0x20) >> 5);
        QualityScalabilityFlag = Convert.ToBoolean((data[3] & 0x10) >> 4);
        HierarchyType = (HierarchyTypeEnum)(data[3] & 0x0f);
        HierarchyLayerIndex = (byte)(data[3] & 0x3f);
        TrefPresentFlag = Convert.ToBoolean((data[4] & 0x80) >> 7);
        HierarchyEmbeddedLayerIndex = (byte)(data[4] & 0x3f);
        HierarchyChannel = (byte)(data[5] & 0x3f);
    }
}

public record ContentCopyControlDescriptor : Descriptor
{
    public enum ControlInfoType : byte
    {
        CanCopyWithoutRestriction = 0b00,
        CopyOnce = 0b10,
        ProhibitedToCopy = 0b11
    }

    public record ComponentControlInfo(ushort ComponentTag, ControlInfoType DigitalRecordingControlData, bool MaximumBitrateFlag, byte? MaximumBitrate);

    public ControlInfoType DigitalRecordingControlData { get; init; }
    public bool MaximumBitRateFlag { get; init; }
    public bool ComponentControlFlag { get; init; }
    public byte? MaximumBitrate { get; init; }
    public ComponentControlInfo[]? ComponentControls { get; init; }
    public ContentCopyControlDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        DigitalRecordingControlData = (ControlInfoType)((data[3] & 0xc0) >> 6);
        MaximumBitRateFlag = Convert.ToBoolean((data[3] & 0x20) >> 5);
        ComponentControlFlag = Convert.ToBoolean((data[3] & 0x10) >> 4);
        var pos = 5;

        if (MaximumBitRateFlag)
        {
            MaximumBitrate = data[pos];
            pos += 1;
        }

        if (ComponentControlFlag)
        {
            var length = data[pos];
            var maxPos = length + pos;
            pos++;

            var controls = new List<ComponentControlInfo>();
            while (pos < maxPos)
            {
                var componentTag = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
                var digitalRecordingControlData = (ControlInfoType)((data[pos + 2] & 0xc0) >> 6);
                var maximumBitrateFlag = Convert.ToBoolean((data[pos + 2] & 0x20) >> 5);
                byte? maximumBitrate = null;
                pos += 4;

                if (maximumBitrateFlag)
                {
                    maximumBitrate = data[pos + 4];
                    pos++;
                }
                controls.Add(new ComponentControlInfo(componentTag, digitalRecordingControlData, maximumBitrateFlag, maximumBitrate));
            }
            ComponentControls = controls.ToArray();
        }
    }
}

public record ContentUsageControlDescriptor : Descriptor
{
    public enum RetentionTimeType : byte
    {
        OneHourAndAHalf = 0b111,
        ThreeHours = 0b110,
        SixHours = 0b101,
        TwelveHours = 0b100,
        OneDay = 0b011,
        TwoDays = 0b010,
        OneWeek = 0b001,
        WithoutLimit = 0b000
    }

    public bool RemoteViewMode { get; init; }
    public bool CopyRestrictionMode { get; init; }
    public bool ImageContraintToken { get; init; }
    public bool RetentionMode { get; init; }
    public RetentionTimeType RetentionState { get; init; }
    public bool EncryptionMode { get; init; }

    public ContentUsageControlDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        RemoteViewMode = Convert.ToBoolean((data[3] & 0x80) >> 7);
        CopyRestrictionMode = Convert.ToBoolean((data[3] & 0x40) >> 6);
        ImageContraintToken = Convert.ToBoolean((data[3] & 0x20) >> 5);
        RetentionMode = Convert.ToBoolean((data[4] & 0x10) >> 4);
        RetentionState = (RetentionTimeType)((data[4] & 0x0e) >> 1);
        EncryptionMode = Convert.ToBoolean(data[4] & 0x01);
    }
}

public record EmergencyNewsDescriptor : Descriptor
{
    // NTP Format
    public ulong TransmitTimestamp { get; init; }
    public EmergencyNewsDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        TransmitTimestamp = BinaryPrimitives.ReadUInt64BigEndian(data[3..11]);
    }
}

public record MHCAContractInfoDescriptor : Descriptor
{
    public ushort CASystemId { get; init; }
    public byte CAUnitId { get; init; }
    public ushort[] ComponentTags { get; init; }
    public byte[] ContractVertificationInfo { get; init; }
    public string FeeName { get; init; }
    public MHCAContractInfoDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        CASystemId = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        CAUnitId = (byte)((data[5] & 0xf0) >> 4);
        var numOfComponent = data[5] & 0x0f;
        ComponentTags = data[6..(6 + 2 * numOfComponent)].ToArray().Chunk(2).Select(d => BinaryPrimitives.ReadUInt16BigEndian(d)).ToArray();

        var pos = 6 + 2 * numOfComponent;
        var cvinfoLength = data[pos];
        ContractVertificationInfo = data[(pos + 1)..(pos + 1 + cvinfoLength)].ToArray();
        pos += 1 + cvinfoLength;

        var feeNameLength = data[pos];
        FeeName = Encoding.UTF8.GetString(data[(pos + 1)..(pos + 1 + feeNameLength)]);
    }
}

public record MHCAServiceDescriptor : Descriptor
{
    public ushort CASystemId { get; init; }
    public byte CABroadcasterGroupId { get; init; }
    public byte MessageControl { get; init; }
    public ushort[] ServiceIds { get; init; }
    public MHCAServiceDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        CASystemId = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        CABroadcasterGroupId = data[5];
        MessageControl = data[6];
        ServiceIds = data[7..(3 + DescriptorLength)].ToArray().Chunk(2).Select(d => BinaryPrimitives.ReadUInt16BigEndian(d)).ToArray();
    }
}

public record RelatedBroadcasterDescriptor : Descriptor
{
    public record BroadcasterIdInfo(ushort NetworkId, byte BroadcasterId);

    public BroadcasterIdInfo[] BroadcasterIds { get; init; }
    public byte[] AffiliationIds { get; init; }
    public ushort[] OriginalNetworkIds { get; init; }
    public RelatedBroadcasterDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        var numOfBroadcasterId = (data[3] & 0xf0) >> 4;
        var numOfAffiliationId = data[3] & 0x0f;
        var numOfOriginalNetworkId = (data[4] & 0xf0) >> 4;

        BroadcasterIds = new BroadcasterIdInfo[numOfBroadcasterId];
        AffiliationIds = new byte[numOfAffiliationId];
        OriginalNetworkIds = new ushort[numOfOriginalNetworkId];

        var pos = 5;
        for (var i = 0; i < numOfBroadcasterId; i++)
        {
            var networkId = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var broadcasterId = data[pos + 2];
            BroadcasterIds[i] = new BroadcasterIdInfo(networkId, broadcasterId);
            pos += 3;
        }

        for (var i = 0; i < numOfAffiliationId; i++)
        {
            AffiliationIds[i] = data[pos];
            pos++;
        }

        for (var i = 0; i < numOfOriginalNetworkId; i++)
        {
            OriginalNetworkIds[i] = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            pos += 2;
        }
    }
}

public record MultimediaServiceInfoDescriptor : Descriptor
{
    public abstract record ServiceInfo;
    public record SubtitleServiceInfo(ushort ComponentTag, string ISO639LanguageCode, string Text) : ServiceInfo;
    public record MultimediaServiceInfo(bool AssociatedContentsFlag) : ServiceInfo;

    public ushort DataComponentId { get; init; }
    public ServiceInfo? Service { get; init; }
    public byte[] SelectorByte { get; init; }
    public MultimediaServiceInfoDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        DataComponentId = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        var pos = 5;
        if (DataComponentId == 0x0020)
        {
            // Subtitle
            var componentTag = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var ISO639 = Encoding.Latin1.GetString(data[(pos + 2)..(pos + 5)]);
            var textLength = data[pos + 5];
            var text = Encoding.UTF8.GetString(data[(pos + 6)..(pos + 6 + textLength)]);
            Service = new SubtitleServiceInfo(componentTag, ISO639, text);
            pos += 6 + textLength;
        }
        else if (DataComponentId == 0x0021)
        {
            // Multimedia
            Service = new MultimediaServiceInfo(Convert.ToBoolean((data[pos] & 0x80) >> 7));
            pos++;
        }
        var selectorLength = data[pos];
        SelectorByte = data[(pos + 1)..(pos + 1 + selectorLength)].ToArray();
    }
}

public record MHStuffingDescriptor : Descriptor
{
    public MHStuffingDescriptor(ReadOnlySpan<byte> data)
    {
        TagAndLengthBytes = 4;
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);
        for (var i = 0; i < DescriptorLength; i++)
        {
            if (data[i + 4] != 0x00) throw new InvalidDataException();
        }
    }
}

public record MHBroadcastIDDescriptor : Descriptor
{
    public ushort OriginalNetworkId { get; init; }
    public ushort TlvStreamId { get; init; }
    public ushort EventId { get; init; }
    public byte BroadcasterId { get; init; }

    public MHBroadcastIDDescriptor(ReadOnlySpan<byte> data)
    {
        TagAndLengthBytes = 4;
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);
        OriginalNetworkId = BinaryPrimitives.ReadUInt16BigEndian(data[4..6]);
        TlvStreamId = BinaryPrimitives.ReadUInt16BigEndian(data[6..8]);
        EventId = BinaryPrimitives.ReadUInt16BigEndian(data[8..10]);
        BroadcasterId = data[10];
    }
}

public record MHNetworkIdentificationDescriptor : Descriptor
{
    public string CountryCode { get; init; }
    public ushort MediaType { get; init; }
    public ushort NetworkId { get; init; }
    public byte[] PrivateData { get; init; }
    public MHNetworkIdentificationDescriptor(ReadOnlySpan<byte> data)
    {
        TagAndLengthBytes = 4;
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);
        CountryCode = Encoding.Latin1.GetString(data[4..7]);
        MediaType = BinaryPrimitives.ReadUInt16BigEndian(data[7..9]);
        NetworkId = BinaryPrimitives.ReadUInt16BigEndian(data[9..11]);
        PrivateData = data[11..(3 + DescriptorLength)].ToArray();
    }
}

// Descriptors in MH-AIT

public record ApplicationProfileInfo(ushort ApplicationProfile, byte VersionMajor, byte VersionMinor, byte VersionMicro);

public enum VisibilityType : byte
{
    Invisible = 0b00,
    InvisibleButVisibleThroughAPI = 0b01,
    Visible = 0b11
}

public record MHApplicationDescriptor : Descriptor
{
    public ApplicationProfileInfo[] ApplicationProfiles { get; init; }
    public bool ServiceBoundFlag { get; init; }
    public VisibilityType Visibility { get; init; }
    public bool PresentApplicationPriority { get; init; }
    public byte ApplicationPriority { get; init; }
    public byte[] TransportProtocolLabel { get; init; }

    public MHApplicationDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        var applicationProfilesLength = data[3];
        ApplicationProfiles = data[4..(4 + applicationProfilesLength)].ToArray()
            .Chunk(5).Select(d => new ApplicationProfileInfo(BinaryPrimitives.ReadUInt16BigEndian(d[..2]), d[2], d[3], d[4])).ToArray();
        var pos = 4 + applicationProfilesLength;
        ServiceBoundFlag = Convert.ToBoolean((data[pos] & 0x80) >> 7);
        Visibility = (VisibilityType)((data[pos] & 0x60) >> 5);
        PresentApplicationPriority = Convert.ToBoolean(data[pos] & 0x01);
        ApplicationPriority = data[pos + 1];
        TransportProtocolLabel = data[(pos + 2)..(3 + DescriptorLength)].ToArray();
    }
}

public record MHTransportProtocolDescriptor : Descriptor
{
    public enum ProtocolType : ushort
    {
        HTTPOrHTTPS = 0x0003,
        DataCarousel = 0x0004,
        MMTNonTimed = 0x0005
    }

    public record SelectorByteForHTTPorMMTNonTimed
    {
        public string URLBase { get; init; }
        public string[] URLExtension { get; init; }
        public SelectorByteForHTTPorMMTNonTimed(ReadOnlySpan<byte> data)
        {
            var urlBaseLength = data[0];
            URLBase = Encoding.UTF8.GetString(data[1..(1 + urlBaseLength)]);
            var urlExtensionCount = data[1 + urlBaseLength];
            URLExtension = new string[urlExtensionCount];

            var pos = 1 + urlBaseLength;
            for (var j = 0; j < urlExtensionCount; j++)
            {
                var urlExtLength = data[pos];
                URLExtension[j] = Encoding.UTF8.GetString(data[(pos + 1)..(pos + 1 + urlExtLength)]);
            }
        }
    }

    public ProtocolType ProtocolId { get; init; }
    public byte TransportProtocolLabel { get; init; }
    public SelectorByteForHTTPorMMTNonTimed? SelectorByte { get; init; }

    public MHTransportProtocolDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        ProtocolId = (ProtocolType)BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        TransportProtocolLabel = data[5];
        if (ProtocolId is ProtocolType.HTTPOrHTTPS or ProtocolType.MMTNonTimed)
        {
            SelectorByte = new SelectorByteForHTTPorMMTNonTimed(data[6..(3 + DescriptorLength)]);
        }
    }
}

public record MHSimpleApplicationLocationDescriptor : Descriptor
{
    public string InitialPath { get; init; }
    public MHSimpleApplicationLocationDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        InitialPath = Encoding.UTF8.GetString(data[3..(3 + DescriptorLength)]);
    }
}

public record MHApplicationBoundaryAndPermissionDescriptor : Descriptor
{
    public record BoundaryAndPermissionInfo(ushort[] PermissionBitmaps, string[] ManagedURLs);
    public BoundaryAndPermissionInfo[] BoundaryAndPermissionInfos { get; init; }
    public MHApplicationBoundaryAndPermissionDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        var pos = 3;
        var infos = new List<BoundaryAndPermissionInfo>();
        while (pos < 3 + DescriptorLength)
        {
            var permissionBitmapCount = data[pos];
            var permissionBitmaps = data[(pos + 1)..(pos + 1 + permissionBitmapCount * 2)].ToArray().Chunk(2).Select(d => BinaryPrimitives.ReadUInt16BigEndian(d)).ToArray();
            pos += 1 + permissionBitmapCount * 2;
            var managedUrlCount = data[pos];

            pos++;
            var urls = new List<string>();
            for (var j = 0; j < managedUrlCount; j++)
            {
                var urlLength = data[pos];
                urls.Add(Encoding.UTF8.GetString(data[(pos + 1)..(pos + 1 + urlLength)]));
                pos += 1 + urlLength;
            }
            infos.Add(new BoundaryAndPermissionInfo(permissionBitmaps, urls.ToArray()));
        }
        BoundaryAndPermissionInfos = infos.ToArray();
    }
}

public record MHAutostartPriorityDescriptor : Descriptor
{
    public byte AutostartPriority { get; init; }
    public MHAutostartPriorityDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        AutostartPriority = data[3];
    }
}

public record MHCacheControlInfoDescriptor : Descriptor
{
    public ushort ApplicationSize { get; init; }
    public byte CachePriority { get; init; }
    public bool PackageFlag { get; init; }
    public byte ApplicationVersion { get; init; }
    public DateOnly ExpireDate { get; init; }
    public MHCacheControlInfoDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        ApplicationSize = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        CachePriority = data[5];
        PackageFlag = Convert.ToBoolean((data[6] & 0x80) >> 7);
        ApplicationVersion = (byte)(data[6] & 0x7f);
        if (data[7] == 0xff && data[8] == 0xff)
        {
            ExpireDate = DateOnly.MaxValue;
        }
        else
        {
            ExpireDate = Utilities.ParseMJDBytes(data[7..9]);
        }
    }
}

public record MHRandomizedLatencyDescriptor : Descriptor
{
    public ushort Range { get; init; }
    public byte Rate { get; init; }
    public bool RandomizationEndTimeFlag { get; init; }
    public DateTime RandomizationEndTime { get; init; }
    public MHRandomizedLatencyDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        Range = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        Rate = data[5];
        RandomizationEndTimeFlag = Convert.ToBoolean((data[6] & 0x80) >> 7);
        if (RandomizationEndTimeFlag)
        {
            RandomizationEndTime = Utilities.ParseJSTandMJDbytes(data[7..12]);
        }
    }
}

public record MHExternalApplicationControlDescriptor : Descriptor
{
    public record TargetApplicationInfo(ushort TargetApplicationClass, ApplicationIdentifier[] ApplicationIdentifier);
    public record OverlayControlledAreaInfo(byte OverlayControlledAreaTag, ushort HorizontalPos, ushort VerticalPos, ushort HorizontalSize, ushort VerticalSize);

    public bool SpecificScopeFlag { get; init; }
    public TargetApplicationInfo? TargetApplication { get; init; }
    public ushort[] PermissionBitmaps { get; init; }
    public bool OverlayAdmissionPolarity { get; init; }
    public OverlayControlledAreaInfo[] OverlayControlledAreas { get; init; }
    public ApplicationIdentifier[] BlockedApplications { get; init; }

    public MHExternalApplicationControlDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        SpecificScopeFlag = Convert.ToBoolean((data[3] & 0x80) >> 7);
        var pos = 4;
        if (SpecificScopeFlag)
        {
            var targetApplicationClass = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var targetApplicationCount = data[pos + 2];
            var targetApplications = data[(pos + 3)..(pos + 3 + 6 * targetApplicationCount)].ToArray().Chunk(6).Select(d => new ApplicationIdentifier(d)).ToArray();
            TargetApplication = new TargetApplicationInfo(targetApplicationClass, targetApplications);
            pos += 3 + 6 * targetApplicationCount;
        }
        var permissionBitmapCount = data[pos];
        PermissionBitmaps = data[(pos + 1)..(pos + 1 + 2 * permissionBitmapCount)].ToArray().Chunk(2).Select(d => BinaryPrimitives.ReadUInt16BigEndian(d)).ToArray();
        pos += 1 + 2 * permissionBitmapCount;

        OverlayAdmissionPolarity = Convert.ToBoolean((data[pos] & 0x80) >> 7);
        var overlayControlledAreaCount = data[pos] & 0x0f;
        pos++;

        OverlayControlledAreas = data[pos..(pos + 9 * overlayControlledAreaCount)].ToArray().Chunk(9)
            .Select(d => new OverlayControlledAreaInfo(d[0], BinaryPrimitives.ReadUInt16BigEndian(d[1..3]),
            BinaryPrimitives.ReadUInt16BigEndian(d[3..5]), BinaryPrimitives.ReadUInt16BigEndian(d[5..7]),
            BinaryPrimitives.ReadUInt16BigEndian(d[7..9]))).ToArray();
        pos += 9 * overlayControlledAreaCount;

        var blockedApplicationCount = data[pos];
        pos++;

        BlockedApplications = data[pos..(pos + 6 * blockedApplicationCount)].ToArray().Chunk(6)
            .Select(d => new ApplicationIdentifier(d)).ToArray();
    }
}

public record MHPlaybackApplicationDescriptor : Descriptor
{
    public ApplicationProfileInfo[] ApplicationProfiles { get; init; }
    public bool ServiceBoundFlag { get; init; }
    public VisibilityType Visibility { get; init; }
    public byte ApplicationPriority { get; init; }
    public byte[] TransportProtocolLabel { get; init; }
    public MHPlaybackApplicationDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        var applicationProfilesLength = data[3];
        ApplicationProfiles = data[4..(4 + applicationProfilesLength)].ToArray().Chunk(5)
            .Select(d => new ApplicationProfileInfo(BinaryPrimitives.ReadUInt16BigEndian(d[..2]), d[2], d[3], d[4])).ToArray();
        var pos = 4 + applicationProfilesLength;

        ServiceBoundFlag = Convert.ToBoolean((data[pos] & 0x80) >> 7);
        Visibility = (VisibilityType)((data[pos] & 0x60) >> 5);
        ApplicationPriority = data[pos + 1];
        TransportProtocolLabel = data[(pos + 2)..(3 + DescriptorLength)].ToArray();
    }
}

public record MHSimplePlaybackApplicationLocationDescriptor : Descriptor
{
    public string InitialPath { get; init; }
    public MHSimplePlaybackApplicationLocationDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        InitialPath = Encoding.UTF8.GetString(data[3..(3 + DescriptorLength)]);
    }
}

public record MHApplicationExpirationDescriptor : Descriptor
{
    public DateTime ExpirationDateAndTime { get; init; }
    public MHApplicationExpirationDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        ExpirationDateAndTime = Utilities.ParseJSTandMJDbytes(data[3..8]);
    }
}

// Descriptors in DAMT
public record MHTypeDescriptor : Descriptor
{
    public string Text { get; init; }
    public MHTypeDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        Text = Encoding.UTF8.GetString(data[3..(3 + DescriptorLength)]);
    }
}

public record MHInfoDescriptor : Descriptor
{
    public string ISO639LanguageCode { get; init; }
    public string Text { get; init; }
    public MHInfoDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        ISO639LanguageCode = Encoding.Latin1.GetString(data[3..6]);
        Text = Encoding.UTF8.GetString(data[6..(3 + DescriptorLength)]);
    }
}

public record MHExpireDescriptor : Descriptor
{
    public enum TimeModeType : byte
    {
        UTCTime = 0x01,
        PassedSeconds = 0x04
    }
    public abstract record TimeInfo;
    public record UTCTimeInfo(ulong UTCTime) : TimeInfo;
    public record SecondTimeInfo(uint PassedSeconds) : TimeInfo;

    public TimeModeType TimeMode { get; init; }
    public TimeInfo? Time { get; init; }
    public MHExpireDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        TimeMode = (TimeModeType)data[3];
        Time = TimeMode switch
        {
            TimeModeType.UTCTime => new UTCTimeInfo(BinaryPrimitives.ReadUInt64BigEndian(data[4..12])),
            TimeModeType.PassedSeconds => new SecondTimeInfo(BinaryPrimitives.ReadUInt32BigEndian(data[5..9])),
            _ => null
        };
    }
}

public record MHCompressionTypeDescriptor : Descriptor
{
    public byte CompressionType { get; init; }
    public uint OriginalSize { get; init; }
    public MHCompressionTypeDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        CompressionType = data[3];
        OriginalSize = BinaryPrimitives.ReadUInt32BigEndian(data[4..8]);
    }
}

public record MPUNodeDescriptor : Descriptor
{
    public ushort NodeTag { get; init; }
    public MPUNodeDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        NodeTag = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
    }
}

// Descriptors in DCMT
public record LinkedPUDescriptor : Descriptor
{
    public byte[] LinkedPUTags { get; init; }
    public LinkedPUDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        var numOfLinkedPu = data[3];
        LinkedPUTags = data[4..(4 + numOfLinkedPu)].ToArray();
    }
}

public record LockedCacheDescriptor : Descriptor
{
    public ushort[] NodeTags { get; init; }
    public LockedCacheDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        var numOfLockedCacheNode = data[3];
        NodeTags = data[4..(4 + 2 * numOfLockedCacheNode)].ToArray().Chunk(2).Select(d => BinaryPrimitives.ReadUInt16BigEndian(d)).ToArray();
    }
}

public record UnlockedCacheDescriptor : Descriptor
{
    public ushort[] NodeTags { get; init; }
    public UnlockedCacheDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        var numOfUnlockedCacheNode = data[3];
        NodeTags = data[4..(4 + 2 * numOfUnlockedCacheNode)].ToArray().Chunk(2).Select(d => BinaryPrimitives.ReadUInt16BigEndian(d)).ToArray();
    }
}

public record PUStructureDescriptor : Descriptor
{
    public uint[] MPUSequenceNumbers { get; init; }
    public PUStructureDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        DescriptorLength = data[2];
        var numOfMpu = data[3];
        MPUSequenceNumbers = data[4..(4 + 4 * numOfMpu)].ToArray().Chunk(4).Select(d => BinaryPrimitives.ReadUInt32BigEndian(d)).ToArray();
    }
}