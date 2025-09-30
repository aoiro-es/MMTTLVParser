using System.Buffers.Binary;
using System.Text;

namespace MMTTLVParser.PacketDefinitions.MMT.Table;

public abstract record DataTransmissionMessageTable;

public record DataDirectoryManagementTable : DataTransmissionMessageTable
{
    public record FileInfo(ushort NodeTag, string FileName);
    public record DirectoryNode(ushort NodeTag, byte DirectoryNodeVersion, string DirectoryNodePath, FileInfo[] Files);

    public byte TableId { get; init; }
    public bool SectionSyntaxIndicator { get; init; }
    public ushort SectionLength { get; init; }
    public byte DataTransmissionSessionId { get; init; }
    public byte VersionNumber { get; init; }
    public bool CurrentNextIndicator { get; init; }
    public byte SectionNumber { get; init; }
    public byte LastSectionNumber { get; init; }
    public string BaseDirectoryPath { get; init; }
    public DirectoryNode[] DirectoryNodes { get; init; }

    public DataDirectoryManagementTable(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        SectionSyntaxIndicator = Convert.ToBoolean((data[1] & 0x80) >> 7);
        SectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        DataTransmissionSessionId = data[3];
        VersionNumber = (byte)((data[5] & 0x3e) >> 1);
        CurrentNextIndicator = Convert.ToBoolean(data[5] & 0x01);
        SectionNumber = data[6];
        LastSectionNumber = data[7];
        var baseDirPathLength = data[8];
        BaseDirectoryPath = Encoding.UTF8.GetString(data[9..(9 + baseDirPathLength)]);
        var numOfDirNodes = data[9 + baseDirPathLength];

        var pos = 10 + baseDirPathLength;
        DirectoryNodes = new DirectoryNode[numOfDirNodes];
        for (var j = 0; j < numOfDirNodes; j++)
        {
            var nodeTag = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var dirNodeVersion = data[pos + 2];
            var dirNodePathLength = data[pos + 3];
            var dirNodePath = Encoding.UTF8.GetString(data[(pos + 4)..(pos + 4 + dirNodePathLength)]);
            pos += 4 + dirNodePathLength;
            var numOfFiles = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            pos += 2;
            var files = new List<FileInfo>();
            for (var k = 0; k < numOfFiles; k++)
            {
                var fileNodeTag = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
                var fileNameLength = data[pos + 2];
                var fileName = Encoding.UTF8.GetString(data[(pos + 3)..(pos + 3 + fileNameLength)]);
                files.Add(new FileInfo(nodeTag, fileName));
                pos += 3 + fileNameLength;
            }
            DirectoryNodes[j] = new DirectoryNode(nodeTag, dirNodeVersion, dirNodePath, files.ToArray());
        }
    }
}

public record DataAssetManagementTable : DataTransmissionMessageTable
{
    public record ItemInfo(ushort NodeTag, uint? ItemId, uint? ItemSize, byte? ItemVersion, bool? ChecksumFlag, uint? ItemChecksum, Descriptor[]? ItemInfos);
    public record MPUInfo(uint MPUSequenceNumber, uint MPUSize, bool IndexItemFlag, bool IndexItemIdFlag, byte IndexItemCompressionType, uint? IndexItemId, ItemInfo[] Items, Descriptor[] MPUInfos);

    public byte TableId { get; init; }
    public bool SectionSyntaxIndicator { get; init; }
    public ushort SectionLength { get; init; }
    public byte DataTransmissionSessionId { get; init; }
    public byte VersionNumber { get; init; }
    public bool CurrentNextIndicator { get; init; }
    public byte SectionNumber { get; init; }
    public byte LastSectionNumber { get; init; }
    public uint TransactionId { get; init; }
    public ushort ComponentTag { get; init; }
    public uint DownloadId { get; init; }
    public MPUInfo[] MPUInfos { get; init; }
    public Descriptor[] ComponentInfos { get; init; }

    public DataAssetManagementTable(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        SectionSyntaxIndicator = Convert.ToBoolean((data[1] & 0x80) >> 7);
        SectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        DataTransmissionSessionId = data[3];
        VersionNumber = (byte)((data[5] & 0x3e) >> 1);
        CurrentNextIndicator = Convert.ToBoolean(data[5] & 0x01);
        SectionNumber = data[6];
        LastSectionNumber = data[7];
        TransactionId = BinaryPrimitives.ReadUInt32BigEndian(data[8..12]);
        ComponentTag = BinaryPrimitives.ReadUInt16BigEndian(data[12..14]);
        DownloadId = BinaryPrimitives.ReadUInt32BigEndian(data[14..18]);
        var numOfMpus = data[18];
        MPUInfos = new MPUInfo[numOfMpus];
        var pos = 19;

        for (var j = 0; j < numOfMpus; j++)
        {
            var mpuSequenceNumber = BinaryPrimitives.ReadUInt32BigEndian(data[pos..(pos + 4)]);
            var mpuSize = BinaryPrimitives.ReadUInt32BigEndian(data[(pos + 4)..(pos + 8)]);
            var indexItemFlag = Convert.ToBoolean((data[pos + 8] & 0x80) >> 7);
            var indexItemIdFlag = Convert.ToBoolean((data[pos + 8] & 0x40) >> 6);
            var indexItemCompressionType = (byte)((data[pos + 8] & 0x30) >> 4);
            pos += 9;
            uint? indexItemId = null;
            if (indexItemFlag && indexItemIdFlag)
            {
                indexItemId = BinaryPrimitives.ReadUInt32BigEndian(data[pos..(pos + 4)]);
                pos += 4;
            }

            var numOfItems = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            pos += 2;
            var items = new ItemInfo[numOfItems];
            for (var k = 0; k < numOfItems; k++)
            {
                var nodeTag = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
                pos += 2;

                uint? itemId = null;
                uint? itemSize = null;
                byte? itemVersion = null;
                bool? checksumFlag = null;
                uint? itemChecksum = null;
                Descriptor[]? itemInfos = null;

                if (!indexItemFlag)
                {
                    itemId = BinaryPrimitives.ReadUInt32BigEndian(data[pos..(pos + 4)]);
                    itemSize = BinaryPrimitives.ReadUInt32BigEndian(data[(pos + 4)..(pos + 8)]);
                    itemVersion = data[pos + 8];
                    checksumFlag = Convert.ToBoolean((data[pos + 9] & 0x80) >> 7);
                    pos += 10;
                    if (checksumFlag.Value)
                    {
                        itemChecksum = BinaryPrimitives.ReadUInt32BigEndian(data[pos..(pos + 4)]);
                        pos += 4;
                    }
                    var itemInfoLength = data[pos];
                    itemInfos = Descriptor.ParseDescriptors(data[(pos + 1)..(pos + 1 + itemInfoLength)], typeof(DataAssetManagementTable));
                    pos += 1 + itemInfoLength;
                }

                items[k] = new ItemInfo(nodeTag, itemId, itemSize, itemVersion, checksumFlag, itemChecksum, itemInfos);
            }
            var mpuInfoLength = data[pos];
            var mpuInfos = Descriptor.ParseDescriptors(data[(pos + 1)..(pos + 1 + mpuInfoLength)], typeof(DataAssetManagementTable));
            MPUInfos[j] = new MPUInfo(mpuSequenceNumber, mpuSize, indexItemFlag, indexItemIdFlag, indexItemCompressionType, indexItemId, items, mpuInfos);
            pos += 1 + mpuInfoLength;
        }
        var componentInfoLength = data[pos];
        ComponentInfos = Descriptor.ParseDescriptors(data[(pos + 1)..(pos + 1 + componentInfoLength)], typeof(DataAssetManagementTable));
    }
}

public record DataContentConfigurationTable : DataTransmissionMessageTable
{
    public record PUInfo(byte PUTag, uint PUSize, ushort[] NodeTags, Descriptor[] PUDescriptors);

    public byte TableId { get; init; }
    public bool SectionSyntaxIndicator { get; init; }
    public ushort SectionLength { get; init; }
    public byte DataTransmissionSessionId { get; init; }
    public byte VersionNumber { get; init; }
    public bool CurrentNextIndicator { get; init; }
    public byte SectionNumber { get; init; }
    public byte LastSectionNumber { get; init; }
    public ushort ContentId { get; init; }
    public byte ContentVersion { get; init; }
    public uint ContentSize { get; init; }
    public bool PUInfoFlag { get; init; }
    public bool ContentInfoFlag { get; init; }
    public PUInfo[]? PUInfos { get; init; }
    public ushort[]? NodeTags { get; init; }
    public Descriptor[]? ContentDescriptors { get; init; }

    public DataContentConfigurationTable(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        SectionSyntaxIndicator = Convert.ToBoolean((data[1] & 0x80) >> 7);
        SectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        DataTransmissionSessionId = data[3];
        VersionNumber = (byte)((data[5] & 0x3e) >> 1);
        CurrentNextIndicator = Convert.ToBoolean(data[5] & 0x01);
        SectionNumber = data[6];
        LastSectionNumber = data[7];
        ContentId = BinaryPrimitives.ReadUInt16BigEndian(data[8..10]);
        ContentVersion = data[10];
        ContentSize = BinaryPrimitives.ReadUInt32BigEndian(data[11..15]);
        PUInfoFlag = Convert.ToBoolean((data[15] & 0x80) >> 7);
        ContentInfoFlag = Convert.ToBoolean((data[15] & 0x40) >> 6);

        var pos = 16;
        if (PUInfoFlag)
        {
            var numberOfPUs = data[pos];
            PUInfos = new PUInfo[numberOfPUs];

            pos++;
            for (var j = 0; j < numberOfPUs; j++)
            {
                var PUTag = data[pos];
                var PUSize = BinaryPrimitives.ReadUInt32BigEndian(data[(pos + 1)..(pos + 5)]);
                var numOfMemberNodes = data[pos + 5];
                var nodeTags = data[(pos + 6)..(pos + 6 + numOfMemberNodes * 2)].ToArray().Chunk(2).Select(d => BinaryPrimitives.ReadUInt16BigEndian(d)).ToArray();
                pos += 6 + numOfMemberNodes * 2;
                var puDescriptorsLength = data[pos];
                var puDescriptors = Descriptor.ParseDescriptors(data[(pos + 1)..(pos + 1 + puDescriptorsLength)], typeof(DataContentConfigurationTable));
                pos += 1 + puDescriptorsLength;
                PUInfos[j] = new PUInfo(PUTag, PUSize, nodeTags, puDescriptors);
            }
        }
        else
        {
            var numOfNodes = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            NodeTags = data[(pos + 2)..(pos + 2 + numOfNodes * 2)].ToArray().Chunk(2).Select(d => BinaryPrimitives.ReadUInt16BigEndian(d)).ToArray();
            pos += 2 + numOfNodes * 2;
        }
        if (ContentInfoFlag)
        {
            var contentDescriptorLoopLength = data[pos];
            ContentDescriptors = Descriptor.ParseDescriptors(data[(pos + 1)..(pos + 1 + contentDescriptorLoopLength)], typeof(DataContentConfigurationTable));
        }
    }
}