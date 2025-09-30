using System.Buffers.Binary;

namespace MMTTLVParser.Sample.CC;

public record CCMFUDataByte
{
    public record SubsampleInfo(DataTypeEnum DataType, uint DataSize);

    public enum DataTypeEnum : byte
    {
        TTML = 0b0000,
        PNGVideo = 0b0001,
        SVGVideo = 0b0010,
        PCMAudio = 0b0011,
        MP3Audio = 0b0100,
        MPEG4AACAudio = 0b0101,
        SVGFont = 0b0110,
        WOFFFont = 0b0111
    }

    public byte SubtitleTag { get; init; }
    public byte SubtitleSequenceNumber { get; init; }
    public byte SubsampleNumber { get; init; }
    public byte LastSubsampleNumber { get; init; }
    public DataTypeEnum DataType { get; init; }
    public bool LengthExtensionFlag { get; init; }
    public bool SubsampleInfoListFlag { get; init; }
    public uint DataSize { get; init; }
    public SubsampleInfo[]? SubsampleInfos { get; init; }
    public byte[] DataByte { get; init; }

    public CCMFUDataByte(ReadOnlySpan<byte> data)
    {
        SubtitleTag = data[0];
        SubtitleSequenceNumber = data[1];
        SubsampleNumber = data[2];
        LastSubsampleNumber = data[3];
        DataType = (DataTypeEnum)((data[4] & 0xf0) >> 4);
        LengthExtensionFlag = Convert.ToBoolean((data[4] & 0x08) >> 3);
        SubsampleInfoListFlag = Convert.ToBoolean((data[4] & 0x04) >> 2);
        var pos = 5;

        if (LengthExtensionFlag)
        {
            DataSize = BinaryPrimitives.ReadUInt32BigEndian(data[pos..(pos + 4)]);
            pos += 4;
        }
        else
        {
            DataSize = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            pos += 2;
        }

        // 運用上はここを通ることはない
        if (SubsampleNumber == 0
            && LastSubsampleNumber > 0
            && SubsampleInfoListFlag)
        {
            SubsampleInfos = new SubsampleInfo[LastSubsampleNumber];
            for (var i = 0; i < SubsampleInfos.Length; i++)
            {
                var subsampleIDataType = (DataTypeEnum)((data[pos] & 0xf0) >> 4);
                uint subsampleIDataSize;
                if (LengthExtensionFlag)
                {
                    subsampleIDataSize = BinaryPrimitives.ReadUInt32BigEndian(data[(pos + 1)..(pos + 5)]);
                    pos += 5;
                }
                else
                {
                    subsampleIDataSize = BinaryPrimitives.ReadUInt16BigEndian(data[(pos + 1)..(pos + 3)]);
                    pos += 3;
                }
            }
        }
        DataByte = data[pos..].ToArray();
    }
}
