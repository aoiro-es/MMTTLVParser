using MMTTLVParser.PacketDefinitions.MMT;
using MMTTLVParser.PacketDefinitions.MMT.Table;
using System.Buffers.Binary;

namespace MMTTLVParser.Parser.ParserFunctions;

public partial class ParserFunctionCollection
{
    [ParserFunction(typeof(MMTPPacket))]
    public static ParserResultModel MMTPPacketParser(ReadOnlySpan<byte> data)
    {
        var version = (byte)((data[0] & 0xc0) >> 6);
        if (version != 0) throw new InvalidDataException("Invalid MMTP version");

        var packetCounterFlag = Convert.ToBoolean((data[0] & 0x20) >> 5);
        var fecType = (MMTPFECType)((data[0] & 0x18) >> 3);
        var extensionHeaderFlag = Convert.ToBoolean((data[0] & 0x02) >> 1);
        var rapFlag = Convert.ToBoolean(data[0] & 0x01);
        var payloadType = (MMTPPayloadType)(data[1] & 0x3f);

        var packetId = BinaryPrimitives.ReadUInt16BigEndian(data[2..4]);
        var deliveryTimeStamp = BinaryPrimitives.ReadUInt32BigEndian(data[4..8]);
        var packetSequenceNumber = BinaryPrimitives.ReadUInt32BigEndian(data[8..12]);
        uint? packetCounter = null;

        if (packetCounterFlag)
        {
            packetCounter = BinaryPrimitives.ReadUInt32BigEndian(data[12..16]);
        }

        var pos = packetCounterFlag ? 16 : 12;
        ushort? extensionType = null, extensionLength = null;
        List<HeaderExtension>? extHeaders = null;
        if (extensionHeaderFlag)
        {
            extensionType = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            extensionLength = BinaryPrimitives.ReadUInt16BigEndian(data[(pos + 2)..(pos + 4)]);
            pos += 4;
            var extHeaderEndPos = pos + extensionLength;

            if (extensionType == 0)
            {
                extHeaders = new List<HeaderExtension>();
                var headerExtEnd = false;
                while (!headerExtEnd)
                {
                    if (pos > extHeaderEndPos) throw new InvalidDataException();

                    headerExtEnd = Convert.ToBoolean((data[pos] & 0x80) >> 7);
                    var extHeaderType = (MultiExtensionHeaderType)(((data[pos] & 0x7f) << 8) | data[pos + 1]);
                    var length = BinaryPrimitives.ReadUInt16BigEndian(data[(pos + 2)..(pos + 4)]);
                    extHeaders.Add(extHeaderType switch
                    {
                        MultiExtensionHeaderType.B61 => new B61MMTPHeaderExtension(data[(pos + 4)..]),
                        MultiExtensionHeaderType.DownloadID => new DownloadIdHeaderExtension(data[(pos + 4)..]),
                        MultiExtensionHeaderType.ItemFragmentation => new ItemFragmentationHeaderExtension(data[(pos + 4)..]),
                        _ => throw new NotImplementedException()
                    });
                    pos += 4 + length;
                }
            }
        }

        var b61 = extHeaders?.FirstOrDefault(h => h is B61MMTPHeaderExtension);
        if (b61 is B61MMTPHeaderExtension b61c && b61c.MMTScramblingControl is not MMTScramblingControlType.Unscrambled)
        {
            // スクランブルされてる
            return new ParserResultModel(PacketStatusEnum.Scrambled);
        }

        var type = payloadType switch
        {
            MMTPPayloadType.MPU => typeof(MPUPayload),
            MMTPPayloadType.ContainsOneOrMoreControlMessages => typeof(ControlMessages),
            _ => throw new InvalidDataException("Unknown payload type")
        };

        var packet = new MMTPPacket(version, packetCounterFlag, fecType, extensionHeaderFlag, rapFlag, payloadType, packetId, deliveryTimeStamp, packetSequenceNumber, packetCounter, extensionType, extensionLength, extHeaders?.ToArray());
        return new ParserResultModel(packet, PacketStatusEnum.Complete, [new PayloadInfo(type, pos, data.Length)]);
    }

    // MMTP_payload()のpayload_typeが0x02の場合
    [ParserFunction(typeof(ControlMessages))]
    public static ParserResultModel ControlMessagesParser(ReadOnlySpan<byte> data)
    {
        Type? GetMessageType(ReadOnlySpan<byte> data) =>
            BinaryPrimitives.ReadUInt16BigEndian(data[0..2]) switch
            {
                0x0000 => typeof(PAMessage),
                0x8000 => typeof(M2SectionMessage),
                0x8001 => typeof(CAMessage),
                0x8002 => typeof(M2ShortSectionMessage),
                0x8003 => typeof(DataTransmissionMessage),
                // 不明なメッセージ
                _ => null
            };

        var divisionIndex = (DivisionIndexType)((data[0] & 0xc0) >> 6);
        var lengthInformationExtensionFlag = Convert.ToBoolean((data[0] & 0x02) >> 1);
        var aggregaateFlag = Convert.ToBoolean(data[0] & 0x01);
        var divisionNumberCounter = data[1];
        var messages = new List<PayloadInfo>();
        var status = PacketStatusEnum.Complete;

        if (aggregaateFlag)
        {
            if (divisionIndex != DivisionIndexType.Undivided)
            {
                throw new InvalidOperationException();
            }

            var pos = 2;
            while (pos < data.Length)
            {
                if (lengthInformationExtensionFlag)
                {
                    var length = (int)BinaryPrimitives.ReadUInt32BigEndian(data[pos..(pos + 4)]);
                    // オーバーフローする可能性がある？
                    if (length > int.MaxValue)
                        throw new InvalidDataException("Invalid length");
                    messages.Add(new PayloadInfo(GetMessageType(data[(pos + 4)..]), pos + 4, pos + 4 + length));
                    pos += length + 4;
                }
                else
                {
                    var length = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
                    messages.Add(new PayloadInfo(GetMessageType(data[(pos + 2)..]), pos + 2, pos + 2 + length));
                    pos += length + 2;
                }
            }
        }
        else
        {
            switch (divisionIndex)
            {
                case DivisionIndexType.Undivided:
                    messages = [new PayloadInfo(GetMessageType(data[2..]), 2, data.Length)];
                    break;
                case DivisionIndexType.DividedIncludingHead:
                    status = PacketStatusEnum.Fragmented;
                    messages = [new PayloadInfo(GetMessageType(data[2..]), 2, data.Length)];
                    break;
                default:
                    status = PacketStatusEnum.Fragmented;
                    messages = [new PayloadInfo(null, 2, data.Length)];
                    break;
            }
        }

        var packet = new ControlMessages(divisionIndex, lengthInformationExtensionFlag, aggregaateFlag, divisionNumberCounter);
        return new ParserResultModel(packet, status, messages.ToArray());
    }

    [ParserFunction(typeof(MPUPayload))]
    public static ParserResultModel MPUPayloadParser(ReadOnlySpan<byte> data)
    {
        var packet = new MPUPayload(
            PayloadLength: BinaryPrimitives.ReadUInt16BigEndian(data[0..2]),
            FragmentType: (FragmentType)((data[2] & 0xf0) >> 4),
            TimeDataFlag: Convert.ToBoolean((data[2] & 0x08) >> 3),
            DivisionIndex: (DivisionIndexType)((data[2] & 0x6) >> 1),
            AggregateFlag: Convert.ToBoolean(data[2] & 0x01),
            DivisionNumberCounter: data[3],
            MPUSequenceNumber: BinaryPrimitives.ReadUInt32BigEndian(data[4..8])
        );

        if (packet.FragmentType != FragmentType.MFU)
        {
            // MFUのみが送出される
            throw new InvalidDataException("Only MFU is supported");
        }

        if (packet.TimeDataFlag)
        {
            if (packet.AggregateFlag)
            {
                var pos = 8;
                var mfuList = new List<PayloadInfo>();
                while (pos + 1 < data.Length)
                {
                    var dataUnitLength = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
                    pos += 2;

                    if (pos + dataUnitLength > data.Length)
                        throw new InvalidDataException("Invalid MFU length");

                    mfuList.Add(new PayloadInfo(typeof(TimedMFUPayload), pos, (pos + dataUnitLength)));
                    pos += dataUnitLength;
                }
                return new ParserResultModel(packet, PacketStatusEnum.Complete, mfuList.ToArray());
            }
            else
            {
                return new ParserResultModel(packet,
                    packet.DivisionIndex == DivisionIndexType.Undivided ? PacketStatusEnum.Complete : PacketStatusEnum.Fragmented,
                    [new PayloadInfo(typeof(TimedMFUPayload), 8, data.Length)]);
            }
        }
        else
        {
            if (packet.AggregateFlag)
            {
                var pos = 8;
                var timedMfuList = new List<PayloadInfo>();
                while (pos < data.Length)
                {
                    var dataUnitLength = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
                    pos += 2;
                    timedMfuList.Add(new PayloadInfo(typeof(NonTimedMFUPayload), pos, (pos + dataUnitLength)));
                    pos += dataUnitLength;
                }
                return new ParserResultModel(packet, PacketStatusEnum.Complete, timedMfuList.ToArray());
            }
            else
            {
                return new ParserResultModel(packet,
                    packet.DivisionIndex == DivisionIndexType.Undivided ? PacketStatusEnum.Complete : PacketStatusEnum.Fragmented,
                    [new PayloadInfo(typeof(NonTimedMFUPayload), 8, data.Length)]);
            }
        }
    }

    [ParserFunction(typeof(NonTimedMFUPayload))]
    public static ParserResultModel NonTimedMFUPayloadParser(ReadOnlySpan<byte> data)
    {
        var packet = new NonTimedMFUPayload(
            ItemId: BinaryPrimitives.ReadUInt32BigEndian(data[..4]),
            MFUDataByte: data[4..].ToArray()
        );
        return new ParserResultModel(packet, PacketStatusEnum.Complete, null);
    }

    [ParserFunction(typeof(TimedMFUPayload))]
    public static ParserResultModel TimedMFUPayloadParser(ReadOnlySpan<byte> data)
    {
        var packet = new TimedMFUPayload(
            MovieFragmentSequenceNumber: BinaryPrimitives.ReadUInt32BigEndian(data[..4]),
            SampleNumber: BinaryPrimitives.ReadUInt32BigEndian(data[4..8]),
            Offset: BinaryPrimitives.ReadUInt32BigEndian(data[8..12]),
            Priority: data[12],
            DependencyCounter: data[13],
            MFUDataByte: data[14..].ToArray()
        );
        return new ParserResultModel(packet, PacketStatusEnum.Complete, null);
    }

    [ParserFunction(typeof(PAMessage))]
    public static ParserResultModel PAMessageParser(ReadOnlySpan<byte> data)
    {
        var messageId = BinaryPrimitives.ReadUInt16BigEndian(data[0..2]);
        var version = data[2];
        var length = BinaryPrimitives.ReadUInt32BigEndian(data[3..7]);

        // TR-B39 によるとここが使われることは無い
        var numberOfTables = data[7];
        var extensions = new List<(byte tableId, byte tableVersion, ushort tableLength)>();
        var pos = 8;
        for (int i = 0; i < numberOfTables; i++)
        {
            extensions.Add((data[pos], data[pos + 1], BinaryPrimitives.ReadUInt16BigEndian(data[(pos + 2)..(pos + 4)])));
            pos += 4;
        }

        var tableInfos = new List<PAMessageTable?>();
        while (pos < data.Length)
        {
            var tableId = data[pos];
            var tableLength = BinaryPrimitives.ReadUInt16BigEndian(data[(pos + 2)..(pos + 4)]) + 4;

            if (pos + tableLength > data.Length)
                throw new InvalidDataException("Invalid table length");

            tableInfos.Add(tableId switch
            {
                >= 0x11 and <= 0x20 => new MMTPackageTable(data[pos..(pos + tableLength)]),
                0x80 => new PackageListTable(data[pos..(pos + tableLength)]),
                0x81 => new LayoutConfigurationTable(data[pos..(pos + tableLength)]),
                // 不明なメッセージ
                _ => null
            });
            pos += tableLength;
        }

        var packet = new PAMessage(messageId, version, length, tableInfos.OfType<PAMessageTable>().ToArray());
        return new ParserResultModel(packet, PacketStatusEnum.Complete, null);
    }

    [ParserFunction(typeof(M2SectionMessage))]
    public static ParserResultModel M2SectionMessageParser(ReadOnlySpan<byte> data)
    {
        if (!Utilities.CheckCRC32(data[5..^4], data[^4..]))
        {
            throw new InvalidDataException("CRC error");
        }

        var packet = new M2SectionMessage(
            MessageId: BinaryPrimitives.ReadUInt16BigEndian(data[..2]),
            Version: data[2],
            Length: BinaryPrimitives.ReadUInt16BigEndian(data[3..5]),
            Table: data[5] switch
            {
                0x82 or 0x83 => new EntitlementControlMessage(data[5..^4]),
                0x84 or 0x85 => new EntitlementManagementMessage(data[5..^4]),
                0x87 or 0x88 => new DownloadControlMessage(data[5..^4]),
                0x89 or 0x8a => new DownloadManagementMessage(data[5..^4]),
                >= 0x8b and <= 0x9b => new MHEventInformationTable(data[5..^4]),
                0x9c => new MHApplicationInformationTable(data[5..^4]),
                0x9d => new MHBroadcasterInformationTable(data[5..^4]),
                0x9e => new MHSoftwareDownloadTriggerTable(data[5..^4]),
                0x9f or 0xa0 => new MHServiceDescriptionTable(data[5..^4]),
                0xa2 => new MHCommonDataTable(data[5..^4]),
                0xa8 => new MHSelectionInformationTable(data[5..^4]),
                // 不明なメッセージ
                _ => null
            },
            CRC32: data[^4..].ToArray()
        );
        return new ParserResultModel(packet, PacketStatusEnum.Complete, null);
    }

    [ParserFunction(typeof(CAMessage))]
    public static ParserResultModel CAMessageParser(ReadOnlySpan<byte> data)
    {
        var packet = new CAMessage(
            MessageId: BinaryPrimitives.ReadUInt16BigEndian(data[..2]),
            Version: data[2],
            Length: BinaryPrimitives.ReadUInt16BigEndian(data[3..5]),
            Table: data[5] switch
            {
                0x86 => new CATable(data[5..]),
                // 不明なメッセージ
                _ => null,
            }
        );
        return new ParserResultModel(packet, PacketStatusEnum.Complete, null);
    }

    [ParserFunction(typeof(M2ShortSectionMessage))]
    public static ParserResultModel M2ShortSectionMessageParser(ReadOnlySpan<byte> data)
    {
        if (data[5] == 0xa1 && !Utilities.CheckCRC32(data[5..^4], data[^4..]))
        {
            // TOTの場合CRCがあるのでチェックしておく
            throw new InvalidDataException("CRC error");
        }

        var packet = new M2ShortSectionMessage(
            MessageId: BinaryPrimitives.ReadUInt16BigEndian(data[..2]),
            Version: data[2],
            Length: BinaryPrimitives.ReadUInt16BigEndian(data[3..5]),
            TableId: data[5],
            SectionSyntaxIndicator: Convert.ToBoolean((data[6] & 0x80) >> 7),
            SectionLength: (ushort)(((data[6] & 0x0f) << 8) | data[7]),
            Table: data[5] switch
            {
                0xa1 => new MHTimeOffsetTable(data[8..]),
                0xa7 => new MHDiscontinuityInformationTable(data[8..]),
                // 不明なメッセージ
                _ => null
            }
        );
        return new ParserResultModel(packet, PacketStatusEnum.Complete, null);
    }

    [ParserFunction(typeof(DataTransmissionMessage))]
    public static ParserResultModel DataTransmissionMessageParser(ReadOnlySpan<byte> data)
    {
        if (!Utilities.CheckCRC32(data[7..^4], data[^4..]))
        {
            throw new InvalidDataException("CRC error");
        }

        var packet = new DataTransmissionMessage(
            MessageId: BinaryPrimitives.ReadUInt16BigEndian(data[..2]),
            Version: data[2],
            Length: BinaryPrimitives.ReadUInt32BigEndian(data[3..7]),
            Table: data[7] switch
            {
                0xa3 => new DataDirectoryManagementTable(data[7..^4]),
                0xa4 => new DataAssetManagementTable(data[7..^4]),
                0xa5 => new DataContentConfigurationTable(data[7..^4]),
                // 不明なメッセージ
                _ => null
            },
            CRC32: data[^4..].ToArray()
        );
        return new ParserResultModel(packet, PacketStatusEnum.Complete, null);
    }
}
