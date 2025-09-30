using MMTTLVParser.PacketDefinitions;
using System.Buffers.Binary;
using System.Net;

namespace MMTTLVParser.Parser.ParserFunctions;

public partial class ParserFunctionCollection
{
    [ParserFunction(typeof(TLVSIPacket))]
    public static ParserResultModel TLVSIPacketParser(ReadOnlySpan<byte> data)
    {
        if (!Utilities.CheckCRC32(data[..^4], data[^4..]))
        {
            throw new InvalidDataException("CRC error");
        }

        var packet = new TLVSIPacket(
            CRC: data[^4..].ToArray(),
            TableId: data[0],
            SectionSyntaxIndicator: Convert.ToBoolean(data[1] >> 7),
            SectionLength: (ushort)(((data[1] & 0x0f) << 8) | data[2]),
            TableIdExtension: BinaryPrimitives.ReadUInt16BigEndian(data[3..5]),
            VersionNumber: (byte)((data[5] & 0x3e) >> 1),
            CurrentNextIndicator: Convert.ToBoolean(data[5] & 0x01),
            SectionNumber: data[6],
            LastSectionNumber: data[7]
        );

        var type = (packet.TableId, packet.TableIdExtension) switch
        {
            (0x40 or 0x41, _) => typeof(TLVNITPacket),
            (0xfe, 0) => typeof(AMTPacket),
            _ => throw new InvalidDataException()
        };

        if (packet.LastSectionNumber != 0)
        {
            return new ParserResultModel(packet, PacketStatusEnum.Fragmented, [new PayloadInfo(type, 8, data.Length - 4)]);
        }
        else
        {
            return new ParserResultModel(packet, PacketStatusEnum.Complete, [new PayloadInfo(type, 8, data.Length - 4)]);
        }
    }

    [ParserFunction(typeof(AMTPacket))]
    public static ParserResultModel AMTPacketParser(ReadOnlySpan<byte> data)
    {
        var numOfServiceId = (ushort)(((data[0] << 8) | data[1]) >> 6);

        var pos = 2;
        var addresses = new List<AMTPacket.AddressData>();
        while (pos < data.Length)
        {
            var serviceId = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var ipVersion = Convert.ToBoolean(data[pos + 2] >> 7);
            var serviceLoopLength = (ushort)(((data[pos + 2] & 0x03) << 8) | data[pos + 3]);
            IPAddress src, dst;
            byte srcMask, dstMask;
            if (ipVersion)
            {
                // IPv6
                src = new IPAddress(data[(pos + 4)..(pos + 20)]);
                srcMask = data[pos + 20];
                dst = new IPAddress(data[(pos + 21)..(pos + 37)]);
                dstMask = data[pos + 37];
            }
            else
            {
                // IPv4
                src = new IPAddress(data[(pos + 4)..(pos + 8)]);
                srcMask = data[pos + 8];
                dst = new IPAddress(data[(pos + 9)..(pos + 13)]);
                dstMask = data[pos + 13];
            }
            var privateDataByte = data[(pos + (ipVersion ? 38 : 14))..(pos + 4 + serviceLoopLength)].ToArray();
            addresses.Add(new AMTPacket.AddressData(serviceId, ipVersion, serviceLoopLength, src, srcMask, dst, dstMask, privateDataByte));
            pos += 4 + serviceLoopLength;
        }
        var packet = new AMTPacket(numOfServiceId, addresses.ToArray());
        return new ParserResultModel(packet, PacketStatusEnum.Complete, null);
    }

    [ParserFunction(typeof(TLVNITPacket))]
    public static ParserResultModel TLVNITPacketParser(ReadOnlySpan<byte> data)
    {
        TLVSIDescriptor ParseDescriptor(ReadOnlySpan<byte> data)
          => data[0] switch
          {
              0x40 => new NetworkNameDescriptor(data),
              0x41 => new ServiceListDescriptor(data),
              0x43 => new SatelliteDeliverySystemDescriptor(data),
              0xcd => new RemoteControlKeyDescriptor(data),
              0xfe => new SystemManagementDescriptor(data),
              _ => new UnIdentifiedDescriptor(data),
          };

        var networkDescriptorsLength = BinaryPrimitives.ReadUInt16BigEndian([(byte)(data[0] & 0x0f), data[1]]);

        // Descriptor1
        var descriptor1Data = data[2..(2 + networkDescriptorsLength)];
        var descriptor1 = new List<TLVSIDescriptor>();
        var pos = 0;
        while (pos < descriptor1Data.Length)
        {
            var length = descriptor1Data[pos + 1];
            var descriptorData = descriptor1Data[pos..(pos + 2 + length)];
            descriptor1.Add(ParseDescriptor(descriptorData));
            pos += 2 + length;
        }
        var tlvStreamLoopLength = (ushort)(((data[2 + networkDescriptorsLength] & 0x0f) << 8) | data[3 + networkDescriptorsLength]);

        // Descriptor2
        var tlvStreamDescriptors = new List<TLVNITPacket.TLVStreamDescriptorInformation>();
        var tlvStreamDescriptorsData = data[(4 + networkDescriptorsLength)..(4 + networkDescriptorsLength + tlvStreamLoopLength)];
        pos = 0;
        while (pos < tlvStreamDescriptorsData.Length)
        {
            var tlvStreamId = BinaryPrimitives.ReadUInt16BigEndian(tlvStreamDescriptorsData[pos..(pos + 2)]);
            var originalNetworkId = BinaryPrimitives.ReadUInt16BigEndian(tlvStreamDescriptorsData[(pos + 2)..(pos + 4)]);
            var tlvStreamDescriptorsLength = (ushort)(((tlvStreamDescriptorsData[pos + 4] & 0x0f) << 8) | tlvStreamDescriptorsData[pos + 5]);
            var innerDescriptor2Data = tlvStreamDescriptorsData[(pos + 6)..(pos + 6 + tlvStreamDescriptorsLength)];
            var innerPos = 0;
            var descriptors2 = new List<TLVSIDescriptor>();
            while (innerPos < innerDescriptor2Data.Length)
            {
                var length = innerDescriptor2Data[innerPos + 1];
                var descriptorData = innerDescriptor2Data[innerPos..(innerPos + 2 + length)];
                innerPos += 2 + length;
                descriptors2.Add(ParseDescriptor(descriptorData));
            }
            tlvStreamDescriptors.Add(new TLVNITPacket.TLVStreamDescriptorInformation(tlvStreamId, originalNetworkId, tlvStreamDescriptorsLength, descriptors2.ToArray()));
            pos += 6 + tlvStreamDescriptorsLength;
        }

        var packet = new TLVNITPacket(networkDescriptorsLength, descriptor1.ToArray(), tlvStreamLoopLength, tlvStreamDescriptors.ToArray());
        return new ParserResultModel(packet, PacketStatusEnum.Complete, null);
    }
}
