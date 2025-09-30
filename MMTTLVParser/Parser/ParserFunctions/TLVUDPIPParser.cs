using MMTTLVParser.PacketDefinitions;
using MMTTLVParser.PacketDefinitions.MMT;
using System.Buffers.Binary;
using System.Net;

namespace MMTTLVParser.Parser.ParserFunctions;

public partial class ParserFunctionCollection
{
    [ParserFunction(typeof(TLVPacket))]
    public static ParserResultModel TLVPacketParser(ReadOnlySpan<byte> data)
    {
        if (data[0] != 0x7f)
            throw new InvalidDataException("Invalid TLV Packet Header");

        var packetType = (TlvPacketType)data[1];
        var dataLength = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);

        if (dataLength != data.Length - 4)
            throw new InvalidDataException("TLV Packet Length Mismatch");

        var packet = new TLVPacket(dataLength, packetType);

        var type = packetType switch
        {
            TlvPacketType.IPv4Packet => typeof(IPv4Packet),
            TlvPacketType.IPv6Packet => typeof(IPv6Packet),
            TlvPacketType.HeaderCompressedIPPacket => typeof(CompressedIPPacket),
            TlvPacketType.TransmissionControlSignalPacket => typeof(TLVSIPacket),
            TlvPacketType.NullPacket => typeof(NullPacket),
            _ => null
        };
        if (type is null)
        {
            throw new InvalidDataException("Unsupported TLV Packet Type");
        }

        return new ParserResultModel(packet, PacketStatusEnum.Complete, [new PayloadInfo(type, 4, data.Length)]);
    }

    [ParserFunction(typeof(IPv4Packet))]
    public static ParserResultModel IPv4PacketParser(ReadOnlySpan<byte> data)
    {
        var version = (data[0] & 0xf0) >> 4;
        if (version != 4)
            throw new InvalidDataException("Invalid IPv4 Packet Header");

        var headerLength = data[0] & 0x0f;
        var packet = new IPv4Packet(
            Version: version,
            HeaderLength: headerLength,
            ServiceType: data[1],
            PacketLength: BinaryPrimitives.ReadUInt16BigEndian(data[2..4]),
            Identifier: BinaryPrimitives.ReadUInt16BigEndian(data[4..6]),
            Flag: (byte)((data[6] & 0xe0) >> 5),
            FragmentOffset: (data[6] & 0x1f) << 8 | data[7],
            Lifetime: data[8],
            Protocol: data[9],
            HeaderChecksum: BinaryPrimitives.ReadUInt16BigEndian(data[10..12]),
            SourceAddress: new(data[12..16]),
            DestinationAddress: new(data[16..20]),
            ExtensionInformation: data[20..(4 * headerLength)].ToArray()
        );

        var type = packet.Protocol switch
        {
            0x11 => typeof(UDPPacket),
            _ => null
        };
        if (type is null)
        {
            throw new InvalidDataException("Unsupported IPv4 Protocol");
        }

        return new ParserResultModel(packet, PacketStatusEnum.Complete, [new PayloadInfo(type, (4 * packet.HeaderLength), data.Length)]);
    }

    [ParserFunction(typeof(IPv6Packet))]
    public static ParserResultModel IPv6PacketParser(ReadOnlySpan<byte> data)
    {
        var version = (data[0] & 0xf0) >> 4;

        if (version != 6)
            throw new InvalidDataException("Invalid IPv6 Packet Header");

        var packet = new IPv6Packet(
            Version: version,
            TrafficClass: (byte)(((data[0] & 0x0f) << 4) | (data[1] & 0xf0) >> 4),
            FlowLabel: (uint)(BinaryPrimitives.ReadUInt16BigEndian(data[..4]) & 0x000fffff),
            PayloadLength: BinaryPrimitives.ReadUInt16BigEndian(data[4..6]),
            NextHeader: data[6],
            HopLimit: data[7],
            SourceAddress: new IPAddress(data[8..24]),
            DestinationAddress: new IPAddress(data[24..40])
        );

        // 拡張ヘッダが来ることはない？
        var type = packet.NextHeader switch
        {
            0x11 => typeof(UDPPacket),
            _ => null
        };
        if (type is null)
        {
            throw new InvalidDataException("Unsupported IPv6 Next Header");
        }

        return new ParserResultModel(packet, PacketStatusEnum.Complete, [new PayloadInfo(type, 40, data.Length)]);
    }

    [ParserFunction(typeof(CompressedIPPacket))]
    public static ParserResultModel CompressedIPPacketParser(ReadOnlySpan<byte> data)
    {
        var packet = new CompressedIPPacket(
            ContextId: (ushort)(data[0] << 8 | data[1] & 0xf0 >> 4),
            SequenceNumber: (byte)(data[1] & 0x0f),
            HeaderTypeOfContextIdentification: (CompressedIPHeaderType)data[2],
            PartialHeader: (CompressedIPHeaderType)data[2] switch
            {
                CompressedIPHeaderType.PartialIPv4AndUDPheader => data[3..23].ToArray(),
                CompressedIPHeaderType.IPv4HeaderIdentifier => data[3..5].ToArray(),
                CompressedIPHeaderType.PartialIPv6AndUDPheader => data[3..45].ToArray(),
                CompressedIPHeaderType.NoCompressedHeader => null,
                _ => throw new Exception("Invalid CompressedIPPacket")
            }
        );

        var totalHeaderLength = (3 + (packet.PartialHeader?.Length ?? 0));
        return new ParserResultModel(packet, PacketStatusEnum.Complete, [new PayloadInfo(typeof(MMTPPacket), totalHeaderLength, data.Length)]);
    }

    [ParserFunction(typeof(NullPacket))]
    public static ParserResultModel NullPacketParser(ReadOnlySpan<byte> data)
    {
        if (data.ContainsAnyExcept((byte)0xff))
        {
            throw new InvalidDataException();
        }
        var packet = new NullPacket(data.Length);
        return new ParserResultModel(packet, PacketStatusEnum.Complete, null);
    }

    [ParserFunction(typeof(UDPPacket))]
    public static ParserResultModel UDPPacketParser(ReadOnlySpan<byte> data)
    {
        var packet = new UDPPacket(
            SourcePort: BinaryPrimitives.ReadUInt16BigEndian(data[..2]),
            DestinationPort: BinaryPrimitives.ReadUInt16BigEndian(data[2..4]),
            DataLength: BinaryPrimitives.ReadUInt16BigEndian(data[4..6]),
            Checksum: BinaryPrimitives.ReadUInt16BigEndian(data[6..8])
        );

        var type = packet.DestinationPort switch
        {
            123 => typeof(NTPPacket),
            _ => typeof(MMTPPacket)
        };

        return new ParserResultModel(packet, PacketStatusEnum.Complete, [new PayloadInfo(type, 8, data.Length)]);
    }

    [ParserFunction(typeof(NTPPacket))]
    public static ParserResultModel NTPPacketParser(ReadOnlySpan<byte> data)
    {
        var packet = new NTPPacket(
            LeapIndicator: (LeapIndicator)((data[0] & 0xc0) >> 6),
            Version: (byte)((data[0] & 0x38) >> 3),
            Mode: (WorkingMode)(data[0] & 0x07),
            Stratum: data[1],
            Poll: data[2],
            Precision: data[3],
            RootDelay: BinaryPrimitives.ReadUInt32BigEndian(data[4..8]),
            RootDispresion: BinaryPrimitives.ReadUInt32BigEndian(data[8..12]),
            ReferenceIdentification: BinaryPrimitives.ReadUInt32BigEndian(data[12..16]),
            ReferenceTimestamp: BinaryPrimitives.ReadUInt64BigEndian(data[16..24]),
            OriginTimestamp: BinaryPrimitives.ReadUInt64BigEndian(data[24..32]),
            ReceiveTimestamp: BinaryPrimitives.ReadUInt64BigEndian(data[32..40]),
            TransmitTimestamp: BinaryPrimitives.ReadUInt64BigEndian(data[40..48])
        );
        return new ParserResultModel(packet, PacketStatusEnum.Complete, null);
    }
}
