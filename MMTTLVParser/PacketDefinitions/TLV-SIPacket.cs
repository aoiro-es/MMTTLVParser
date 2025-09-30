using System.Buffers.Binary;
using System.Net;
using System.Text;
using static MMTTLVParser.PacketDefinitions.AMTPacket;
using static MMTTLVParser.PacketDefinitions.TLVNITPacket;

namespace MMTTLVParser.PacketDefinitions;

public abstract record TLVSIDescriptor;

public record ServiceIdTypePair(ushort ServiceId, ServiceType ServiceType);
public enum ServiceType : byte
{
    DigitalTVService = 0x01,
    DigitalAudioService = 0x02,
    EngineeringService = 0xa4,
    DataService = 0xc0,
    StorageTypeServiceUsingTLV = 0xc1,
    MultimediaService = 0xc2
}

public record UnIdentifiedDescriptor : TLVSIDescriptor
{
    public byte[] Data { get; init; }
    public UnIdentifiedDescriptor(ReadOnlySpan<byte> data)
    {
        Data = data.ToArray();
    }
}

public record NetworkNameDescriptor : TLVSIDescriptor
{
    public string NetworkName { get; init; }
    public NetworkNameDescriptor(ReadOnlySpan<byte> data)
    {
        if (data[0] != 0x40) throw new InvalidDataException();
        var length = data[1];
        NetworkName = Encoding.UTF8.GetString(data[2..(length + 2)]);
    }
}


public record ServiceListDescriptor : TLVSIDescriptor
{
    public ServiceIdTypePair[] ServiceList { get; init; }

    public ServiceListDescriptor(ReadOnlySpan<byte> data)
    {
        if (data[0] != 0x41) throw new InvalidDataException();
        var length = data[1];

        var tmpServiceList = new List<ServiceIdTypePair>();
        foreach (var idAndTypeBinary in data[2..(2 + length)].ToArray().Chunk(3))
        {
            var sid = BinaryPrimitives.ReadUInt16BigEndian(idAndTypeBinary.AsSpan(0, 2));
            var type = (ServiceType)idAndTypeBinary[2];
            tmpServiceList.Add(new(sid, type));
        }
        ServiceList = tmpServiceList.ToArray();
    }
}


public record SatelliteDeliverySystemDescriptor : TLVSIDescriptor
{
    public byte DescriptorTag { get; init; }
    public byte DescriptorLength { get; init; }
    public decimal Frequency { get; init; }
    public decimal OrbitalPosition { get; init; }
    public bool WestEastFlag { get; init; }
    public PolarizationType Polarization { get; init; }
    public ModulationSystem Modulation { get; init; }
    public decimal SymbolRate { get; init; }
    public FECInnerCodeType FECInner { get; init; }

    public SatelliteDeliverySystemDescriptor(ReadOnlySpan<byte> data)
    {
        DescriptorTag = data[0];
        DescriptorLength = data[1];
        Frequency = (decimal)Utilities.ParseBCDToInteger(data[2..6]) / 100000;
        OrbitalPosition = (decimal)Utilities.ParseBCDToInteger(data[6..8]) / 10;
        WestEastFlag = Convert.ToBoolean((data[8] & 0x80) >> 7);
        Polarization = (PolarizationType)((data[8] & 0x60) >> 5);
        Modulation = (ModulationSystem)(data[8] & 0x1f);
        SymbolRate = (decimal)Utilities.ParseBCDToInteger([.. data[9..12], (byte)(data[12] & 0xf0)]) / 10;
        FECInner = (FECInnerCodeType)(data[12] & 0x0f);
    }
    public enum PolarizationType : byte
    {
        Horizontal = 0b00,
        Vertical = 0b01,
        CounterClockwiseRotation = 0b10,
        ClockwiseRotation = 0b11
    }

    public enum ModulationSystem : byte
    {
        QPSK = 0b0_0001,
        WideBandSatelliteDigitalBroadcastingSystem = 0b0_1000,
        SatelliteDigitalAudioBroadcastingSystem = 0b0_1001,
        AdvancedNarrowBandCSDigitalBroadcastingSystem = 0b0_1010,
        AdvancedWideBandSatelliteDigitalBroadcastingSystem = 0b0_1011
    }

    public enum FECInnerCodeType : byte
    {
        CodingRate1_2 = 0b0001,
        CodingRate2_3 = 0b0010,
        CodingRate3_4 = 0b0011,
        CodingRate5_6 = 0b0100,
        CodingRate7_8 = 0b0101,
        WideBandSatelliteDigitalBroadcastingSystem = 0b1000,
        SatelliteDigitalAudioBroadcastingSystem = 0b1001,
        AdvancedNarrowBandCSDigitalBroadcastingSystem = 0b1010,
        AdvancedWideBandSatelliteDigitalBroadcastingSystem = 0b1011,
        WithoutInnerCode = 0b1111
    }
}

public record RemoteControlKeyDescriptor : TLVSIDescriptor
{
    public record RemoteControlKeyPair(byte RemoteControlKeyId, ushort ServiceId);

    public byte NumOfRemoteControlKeyId { get; init; }
    public RemoteControlKeyPair[] RemoteControlKeyData { get; init; }

    public RemoteControlKeyDescriptor(ReadOnlySpan<byte> data)
    {
        if (data[0] != 0xcd) throw new InvalidDataException();
        var length = data[1];
        NumOfRemoteControlKeyId = data[2];
        var tmpRemoteKeyData = new List<RemoteControlKeyPair>();
        for (var i = 0; i < NumOfRemoteControlKeyId; i++)
        {
            tmpRemoteKeyData.Add(new(data[3 + 5 * i], BinaryPrimitives.ReadUInt16BigEndian(data[(4 + 5 * i)..(6 + 5 * i)])));
        }
        RemoteControlKeyData = tmpRemoteKeyData.ToArray();
    }
}

public record SystemManagementDescriptor : TLVSIDescriptor
{
    public BroadcastingOrNonBroadcastingType BroadcastingOrNonBroadcasting { get; init; }
    public BroadcastingStandardType BroadcastingStandard { get; init; }
    public byte DetailedIdentification { get; init; }
    public byte[] AdditionalBroadcastingInfo { get; init; }

    public SystemManagementDescriptor(ReadOnlySpan<byte> data)
    {
        if (data[0] != 0xfe) throw new InvalidDataException();
        var length = data[1];
        BroadcastingOrNonBroadcasting = (BroadcastingOrNonBroadcastingType)(data[2] & 0xc0);
        BroadcastingStandard = (BroadcastingStandardType)(data[2] & 0x3f);
        DetailedIdentification = data[3];
        AdditionalBroadcastingInfo = data[4..(2 + length)].ToArray();
    }

    public enum BroadcastingOrNonBroadcastingType : byte
    {
        Broadcasting = 0b00,
        NonBroadcasting1 = 0b01,
        NonBroadcasting2 = 0b10,
    }

    public enum BroadcastingStandardType : byte
    {
        NarrowbandCSDigitalBroadcasting = 0b000001,
        BSDigitalBroadcasting = 0b000010,
        DigitalTerrestrialTelevisionBroadcasting = 0b000011,
        WideBandCSDigitalBroadcasting = 0b000100,
        DigitalTerrestrialSoundBroadcasting = 0b000101,
        AdvancedNarrowbandCSDigitalBroadcasting = 0b000111,
        AdvancedBSDigitalBroadcasting = 0b001000,
        AdvancedWideBandCSDigitalBroadcasting = 0b001001,
        VHighMultimediaBroadcasting = 0b001010,
        VLowMultimediaBroadcasting = 0b001011,
    }
}

public record TLVNITPacket(ushort NetworkDescriptorsLength, TLVSIDescriptor[] Descriptor1, ushort TLVStreamLoopLength, TLVStreamDescriptorInformation[] TLVStreamDescriptors) : Packet
{
    public record TLVStreamDescriptorInformation(ushort TlvStreamId, ushort OriginalNetworkId,
        ushort TlvStreamDescriptorsLength, TLVSIDescriptor[] Descriptor);
}

public record AMTPacket(ushort NumOfServiceId, AddressData[] AddressDatas) : Packet
{
    public record AddressData(ushort ServiceId, bool IpVersion, ushort ServiceLoopLength,
        IPAddress SrcAddress, byte SrcAddressMask, IPAddress DstAddress, byte DstAddressMask,
        byte[] PrivateDataByte);
}

public record TLVSIPacket(byte TableId, bool SectionSyntaxIndicator, ushort SectionLength,
    ushort TableIdExtension, byte VersionNumber, bool CurrentNextIndicator, byte SectionNumber,
    byte LastSectionNumber, byte[] CRC) : Packet;
